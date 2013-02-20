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
    public class HexFileOutput : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return new Type[] {typeof(TextBuilder)}; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return ProcessPacket; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return ProcessedPacket; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return null; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        StreamWriter writer = null;
        bool WriteToFile = true;
        string _outFileName;
        string _logPrefix;
        StringBuilder subPackets;

        public bool Init(PacketFileProcessor file)
        {
            if (!Settings.HexOutput)
                return false;
            _logPrefix = file.LogPrefix;
            bool append = true;
            if (Settings.HexFileName == string.Empty)
            {
                _outFileName = Path.ChangeExtension(file.FileName, null) + "_hex.txt";
                append = false;
            }
            else
                _outFileName = Settings.HexFileName;
            if (Utilities.FileIsInUse(_outFileName))
            {
                // If our dump format requires a .txt to be created,
                // check if we can write to that .txt before starting parsing
                Trace.WriteLine(string.Format("Packet hex output file {0} is in use, output will not be saved.", _outFileName));
                return false;
            }
            if (!append)
                File.Delete(_outFileName);
            writer = new StreamWriter(_outFileName, append);
            writer.WriteLine(file.GetHeader());
            return true;
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
                subPackets.AppendLine(packet.GetHeader());
                subPackets.AppendLine(packet.ToHex());
                return;
            }
            StreamWriter w = writer;
            w.WriteLine(packet.GetHeader());
            w.WriteLine(packet.ToHex());
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
