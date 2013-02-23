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

        public Dictionary<Packet, Dictionary<Guid, Dictionary<string, Object>>> packetObjects = new Dictionary<Packet, Dictionary<Guid, Dictionary<string, Object>>>();

        public void ProcessData(string name, int? index, Object obj, Type t, Packet packet)
        {
            if (obj.GetType() == typeof(Guid))
            {
                var guid = (Guid)obj;
                var set = packetObjects[packet];
                if (guid.Full != 0 && !set.ContainsKey(guid))
                    set.Add(guid, new Dictionary<string, Object>() 
                    { 
                        { "@packetId", packet.Number }, 
                        { "@packetSubId", packet.SubPacketNumber },
                        { "@objectGuid", guid.Full },
                        { "@auras", null }, 
                        { "@movement", null },
                        { "@fields", null },
                        { "@unitFieldFlags", null },
                        { "@unitFieldFlags2", null }
                    });
            }
        }
        

        public void ProcessPacket(Packet packet)
        {
            packetObjects.Add(packet, new Dictionary<Guid, Dictionary<string, Object>>());
        }

        public static string insertPacket = @"INSERT INTO [packet] ([id], [subId], [from], [length], [date], [opcodeId], [status], [data]) 
                                                             VALUES(@id, @subId, @from, @length, @date, @opcodeId, @status, @data)";

        public static string insertPacketObject = @"INSERT INTO [packetObject] ([packetId], [packetSubId], [objectGuid], [auras], [movement], [fields], [unitFieldFlags], [unitFieldFlags2]) 
                                                                          VALUES(@packetId, @packetSubId, @objectGuid, @auras, @movement, @fields, @unitFieldFlags, @unitFieldFlags2)";
        public void ProcessedPacket(Packet packet)
        {
            var objects = PacketFileProcessor.Current.GetProcessor<ObjectStore>().Objects;
            var set = packetObjects[packet];
            switch(Opcodes.GetOpcode(packet.Opcode))
            {
                case Opcode.SMSG_UPDATE_OBJECT:
                    {

                        var updates = packet.GetData().GetNode<IndexedTreeNode>("Updates");
                        foreach (var update in updates)
                        {
                            string typeObj = update.Value.GetNode<string>("UpdateType");
                            if (typeObj.Contains("Create") || typeObj.Contains("Values"))
                            {
                                var guid = update.Value.GetNode<Guid>("GUID");
                                var obj = objects[guid];

                                if (Settings.SQLiteDumpCurrentFields)
                                    set[guid]["@fields"] = PrintUpdateFields(obj);
                                switch (obj.Type)
                                {
                                    case ObjectType.Player:
                                    case ObjectType.Unit:
                                        set[guid]["@unitFieldFlags"] = obj.GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_FLAGS);
                                        set[guid]["@unitFieldFlags2"] = obj.GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_FLAGS_2);
                                        break;
                                } 
                            }
                        }
                    }
                    break;
                case Opcode.SMSG_AURA_UPDATE_ALL:
                case Opcode.SMSG_AURA_UPDATE:
                    {
                        if (Settings.SQLiteDumpCurrentAuras)
                        {
                            var guid = packet.GetData().GetNode<Guid>("GUID");
                            var obj = objects[guid];
                            switch (obj.Type)
                            {
                                case ObjectType.Player:
                                case ObjectType.Unit:
                                    set[guid]["@auras"] = PrintAuras((Unit)obj);
                                    break;
                            }
                            
                        }
                    }
                    break;
            }


            foreach (var objectData in set)
            {
                // command for each object stored
                using (SQLiteCommand cmd = new SQLiteCommand(insertPacketObject, _connection))
                {
                    cmd.Transaction = tr;
                    // insert all parameters
                    foreach (var dataEntry in objectData.Value)
                    {
                        cmd.Parameters.Add(new SQLiteParameter(dataEntry.Key, dataEntry.Value));
                    }
                    cmd.ExecuteNonQuery();
                }
            }
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

        private string PrintAuras(Unit unit)
        {
            StringBuilder b = new StringBuilder();
            foreach (var slotAura in unit.Auras)
            {
                var aura = slotAura.Value;
                b.Append(slotAura.Key);
                b.Append("\tSpellID:");
                b.AppendLine(aura.SpellId.ToString());
                b.Append("\tAuraFlags:");
                b.AppendLine(aura.AuraFlags.ToString());
                b.Append("\tLevel:");
                b.AppendLine(aura.Level.ToString());
                b.Append("\tCharges:");
                b.AppendLine(aura.Charges.ToString());
                b.Append("\tCasterGuid:");
                b.AppendLine(aura.CasterGuid.ToString());
                b.Append("\tMaxDuration:");
                b.AppendLine(aura.MaxDuration.ToString());
                b.Append("\tDuration:");
                b.AppendLine(aura.Duration.ToString());
                b.AppendLine("\tEffects:");
                foreach(var effVal in aura.Effects)
                {
                    b.AppendLine("\t\t" + effVal.Key.ToString() + ":" + effVal.Value.ToString());
                }
            }
            return b.ToString();
        }

        private string PrintUpdateFields(WoWObject obj)
        {
            UpdateFieldDictionary fields = obj.UpdateFields;
            StringBuilder b = new StringBuilder();

            var itr = fields.GetEnumerator();
            int nextI = 0;
            while (itr.MoveNext())
            {
                int i = itr.Current.Key;
                bool fin = false;
                while (i < nextI)
                {
                    if (!itr.MoveNext())
                    {
                        fin = true;
                        break;
                    }
                    i = itr.Current.Key;
                }
                if (fin)
                    break;
                var name = UpdateFields.GetUpdateFieldNameByOffset(i, obj.Type);
                if (name == null)
                    continue;

                Type fieldType = UpdateFields.GetUpdateFieldType(name);
                int fieldSize = UpdateFieldDictionary.GetFieldsCount(fieldType);
                nextI = i + fieldSize;

                b.Append(name);
                b.Append(" (");
                b.Append(i);
                b.Append(")");

                b.Append(UpdateFields.GetUpdateFieldBytesInfo(name));
                b.Append(":");
                b.Append(fields.GetValue(i, fieldType));
                b.AppendLine();
            }
            return b.ToString();
        }

        public static string insertObject = @"INSERT INTO [object] ([guid], [type], [typeMask], [typeName], [entry], [name]) 
                                                             VALUES(@guid, @type, @typeMask, @typeString, @entry, @name)";

        public void Finish()
        {
            var names = PacketFileProcessor.Current.GetProcessor<NameStore>();

            var objects = PacketFileProcessor.Current.GetProcessor<ObjectStore>().Objects;
            foreach (var pair in objects)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(insertObject, _connection))
                {
                    var guid = pair.Key;
                    var obj = pair.Value;
                    cmd.Transaction = tr;
                    cmd.Parameters.Add(new SQLiteParameter("@guid", guid.Full));
                    cmd.Parameters.Add(new SQLiteParameter("@type", obj.Type));
                    cmd.Parameters.Add(new SQLiteParameter("@typeString", obj.Type.ToString()));
                    cmd.Parameters.Add(new SQLiteParameter("@typeMask", obj.GetValue<ObjectField, uint?>(ObjectField.OBJECT_FIELD_TYPE)));
                    var entry = obj.GetValue<ObjectField, uint?>(ObjectField.OBJECT_FIELD_ENTRY);
                    if (entry == null)
                        entry = guid.GetEntry();
                    cmd.Parameters.Add(new SQLiteParameter("@entry", entry));
                    if (obj.Type == ObjectType.Player)
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetPlayerName(guid)));
                    else
                        cmd.Parameters.Add(new SQLiteParameter("@name", names.GetName(Utilities.ObjectTypeToStore(obj.Type), (int)guid.GetEntry(), false)));
                    cmd.ExecuteNonQuery();
                }
            }

            tr.Commit();
            _connection.Close();
            Trace.WriteLine(string.Format("{0}: Saved file to '{1}'", _logPrefix, _outFileName));
        }
    }
}
