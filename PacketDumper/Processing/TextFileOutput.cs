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

namespace PacketDumper.Processing
{
    public class TextFileOutput : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return new Type[] {typeof(TextBuilder)}; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return ProcessPacket; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return ProcessedPacket; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { if (Filters.Enabled) return ProcessData; return Stub; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        StreamWriter writer = null;
        bool WriteToFile = true;
        string _outFileName;
        string _logPrefix;
        StringBuilder subPackets;

        public bool Init(PacketFileProcessor file)
        {
            if (!Settings.TextOutput)
                return false;
            _logPrefix = file.LogPrefix;
            bool append = true;
            if (Settings.TextFileName == string.Empty)
            {
                _outFileName = Path.ChangeExtension(file.FileName, null) + "_parsed.txt";
                append = false;
            }
            else
                _outFileName = Settings.TextFileName;

            if (Utilities.FileIsInUse(_outFileName))
            {
                // If our dump format requires a .txt to be created,
                // check if we can write to that .txt before starting parsing
                Trace.WriteLine(string.Format("Txt output file {0} is in use, output will not be saved.", _outFileName));
                return false;
            }
            if (!append)
                File.Delete(_outFileName);
            writer = new StreamWriter(_outFileName, append);
            writer.WriteLine(file.GetHeader());

            return true;
        }

        public void Stub(string name, int? index, Object obj, Type t, Packet packet)
        {
        }

        public void ProcessData(string name, int? index, Object obj, Type t, Packet packet)
        {
            if (!WriteToFile)
                return;
            if (t == typeof(Guid))
                WriteToFile = Filters.CheckFilter((Guid)obj);
            else if (t == typeof(StoreEntry))
            {
                var val = (StoreEntry)obj;
                WriteToFile = Filters.CheckFilter(val._type, val._data);
            }
        }

        public void ProcessPacket(Packet packet)
        {
            if (packet.SubPacket)
                return;
            subPackets = new StringBuilder();
            WriteToFile = true;
        }

        public void ProcessedPacket(Packet packet)
        {
            if (!WriteToFile)
                return;
            if (packet.SubPacket)
            {
                subPackets.Append(packet.GetHeader());
                subPackets.AppendLine(PacketFileProcessor.Current.GetProcessor<TextBuilder>().LastPacket);
                return;
            }
            StreamWriter w = writer;
            w.Write(packet.GetHeader());
            w.WriteLine(PacketFileProcessor.Current.GetProcessor<TextBuilder>().LastPacket);
            w.Write(subPackets);
            w.Flush();
        }

        public void Finish()
        {
            if (writer != null)
                writer.Close();

            Trace.WriteLine(string.Format("{0}: Saved file to '{1}'", _logPrefix, _outFileName));
        }
    }
}
