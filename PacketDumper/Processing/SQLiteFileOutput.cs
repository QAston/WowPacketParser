using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using PacketParser.Enums;
using Guid = PacketParser.DataStructures.Guid;
using PacketParser.Processing;
using PacketParser.Misc;
using PacketDumper.Misc;
using PacketParser.DataStructures;
using System.Data.SQLite;
using PacketParser.Enums.Version;
using System.Collections.Generic;

namespace PacketDumper.Processing
{
    public class SQLiteFileOutput : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return new Type[] { typeof(TextBuilder)}; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return ProcessPacket; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return ProcessedPacket; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return ProcessData; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        SQLiteConnection _connection = null;
        string _outFileName;
        string _logPrefix;
        SQLiteTransaction tr = null;

        public static string insertInfo = @"INSERT INTO [info] ([fileName], [clientBuild], [parserBuild]) VALUES(@fileName, @clientBuild, @parserBuild)";
        public static string insertOpcode = @"INSERT INTO [opcode] ([id], [name]) 
                                                             VALUES(@id, @name)";

        public bool Init(PacketFileProcessor file)
        {
            if (!Settings.SQLiteOutput)
                return false;
            _logPrefix = file.LogPrefix;
            if (Settings.SQLiteFileName == string.Empty)
            {
                _outFileName = Path.ChangeExtension(file.FileName, null) + "_parsed.db";
            }
            else
                _outFileName = Settings.SQLiteFileName;
            if (Utilities.FileIsInUse(_outFileName))
            {
                // If our dump format requires a .txt to be created,
                // check if we can write to that .txt before starting parsing
                Trace.WriteLine(string.Format("Packet SQLite output file {0} is in use, output will not be saved.", _outFileName));
                return false;
            }
            File.Delete(_outFileName);
            File.Copy("template.db", _outFileName);
            _connection = new SQLiteConnection("Data Source=" + _outFileName);
            _connection.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(insertInfo, _connection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@fileName", file.FileName));
                cmd.Parameters.Add(new SQLiteParameter("@clientBuild", ClientVersion.BuildInt));
                cmd.Parameters.Add(new SQLiteParameter("@parserBuild", PacketFileProcessor.GetBuild()));
                cmd.ExecuteNonQuery();
            }

            tr = _connection.BeginTransaction();

            var opcodes = Opcodes.GetOpcodes();
            foreach (var o in opcodes)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(insertOpcode, _connection))
                {
                    cmd.Transaction = tr;
                    cmd.Parameters.Add(new SQLiteParameter("@id", o.Value));
                    cmd.Parameters.Add(new SQLiteParameter("@name", o.Key.ToString()));
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public static string insertPacketObject = @"INSERT INTO [packetObject] ([packetId], [packetSubId], [objectGuid]) 
                                                             VALUES(@packetId, @packetSubId, @objectGuid)";
        public Dictionary<Packet, HashSet<Guid>> packetObjects = new Dictionary<Packet, HashSet<Guid>>();
        public HashSet<Guid> guids = new HashSet<Guid>();

        public void ProcessData(string name, int? index, Object obj, Type t, Packet packet)
        {
            if (obj.GetType() == typeof(Guid))
            {
                var guid = (Guid)obj;
                var set = packetObjects[packet];
                if (guid.Full != 0 && !set.Contains(guid))
                {
                    set.Add(guid);
                    guids.Add(guid);
                    using (SQLiteCommand cmd = new SQLiteCommand(insertPacketObject, _connection))
                    {
                        cmd.Transaction = tr;
                        cmd.Parameters.Add(new SQLiteParameter("@packetId", packet.Number));
                        cmd.Parameters.Add(new SQLiteParameter("@packetSubId", packet.SubPacketNumber));
                        cmd.Parameters.Add(new SQLiteParameter("@objectGuid", (guid).Full));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        

        public void ProcessPacket(Packet packet)
        {
            packetObjects.Add(packet, new HashSet<Guid>());
        }

        public static string insertPacket = @"INSERT INTO [packet] ([id], [subId], [from], [length], [date], [opcodeId], [status], [data]) 
                                                             VALUES(@id, @subId, @from, @length, @date, @opcodeId, @status, @data)";

        public void ProcessedPacket(Packet packet)
        {
            packetObjects.Remove(packet);
            using (SQLiteCommand cmd = new SQLiteCommand(insertPacket, _connection))
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add(new SQLiteParameter("@id", packet.Number));
                cmd.Parameters.Add(new SQLiteParameter("@subId", packet.SubPacketNumber));
                cmd.Parameters.Add(new SQLiteParameter("@from", (packet.Direction == Direction.ClientToServer) ? "c" : "s"));
                cmd.Parameters.Add(new SQLiteParameter("@length", packet.Length));
                cmd.Parameters.Add(new SQLiteParameter("@date", packet.Time));
                cmd.Parameters.Add(new SQLiteParameter("@opcodeId", packet.Opcode));
                switch (packet.Status)
                {
                    case ParsedStatus.NotParsed:
                        cmd.Parameters.Add(new SQLiteParameter("@status", "n"));
                        break;
                    case ParsedStatus.Success:
                        cmd.Parameters.Add(new SQLiteParameter("@status", "s"));
                        break;
                    case ParsedStatus.WithErrors:
                        cmd.Parameters.Add(new SQLiteParameter("@status", "e"));
                        break;
                }
                StringBuilder w = new StringBuilder();
                w.Append(packet.GetHeader());
                w.Append(PacketFileProcessor.Current.GetProcessor<TextBuilder>().LastPacket);
                cmd.Parameters.Add(new SQLiteParameter("@data", w.ToString()));
                cmd.ExecuteNonQuery();
            }
        }

        public static string insertObject = @"INSERT INTO [object] ([guid], [type], [entry], [name]) 
                                                             VALUES(@guid, @type, @entry, @name)";

        public void Finish()
        {
            var names = PacketFileProcessor.Current.GetProcessor<NameStore>();
            
            foreach (var guid in guids)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(insertObject, _connection))
                {
                    cmd.Transaction = tr;
                    cmd.Parameters.Add(new SQLiteParameter("@guid", guid.Full));
                    cmd.Parameters.Add(new SQLiteParameter("@type", guid.GetHighTypeString()));
                    cmd.Parameters.Add(new SQLiteParameter("@entry", guid.GetEntry()));
                    if (guid.GetHighType() == HighGuidType.Player)
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetPlayerName(guid)));
                    else
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetName(Utilities.ObjectTypeToStore(guid.GetObjectType()), (int)guid.GetEntry(), false)));
                    cmd.ExecuteNonQuery();
                }
            }

            /* this does not contain players - useless
            var objects = PacketFileProcessor.Current.GetProcessor<ObjectStore>().Objects;
            foreach (var pair in objects)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(insertObject, _connection))
                {
                    var guid = pair.Key;
                    var obj = pair.Value.Item1;
                    cmd.Transaction = tr;
                    cmd.Parameters.Add(new SQLiteParameter("@guid", guid.Full));
                    cmd.Parameters.Add(new SQLiteParameter("@type", guid.GetHighTypeString()));
                    cmd.Parameters.Add(new SQLiteParameter("@entry", pair.Key));
                    if (guid.GetHighType() == HighGuidType.Player)
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetPlayerName(guid)));
                    else
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetName(Utilities.ObjectTypeToStore(guid.GetObjectType()), (int)guid.GetEntry())));
                    cmd.ExecuteNonQuery();
                }
            }*/

            tr.Commit();
            _connection.Close();
            Trace.WriteLine(string.Format("{0}: Saved file to '{1}'", _logPrefix, _outFileName));
        }
    }
}
