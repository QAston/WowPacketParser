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
    public class ParseErrorFileOutput : IPacketProcessor
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
            if (!Settings.PacketErrorsOutput)
                return false;
            _logPrefix = file.LogPrefix;
            if (Settings.PacketErrorsFileName == string.Empty)
                _outFileName = Path.ChangeExtension(file.FileName, null) + "_errors.txt";
            else
                _outFileName = Settings.PacketErrorsFileName;
            if (Utilities.FileIsInUse(_outFileName))
            {
                // If our dump format requires a .txt to be created,
                // check if we can write to that .txt before starting parsing
                Trace.WriteLine(string.Format("Packet error output file {0} is in use, output will not be saved.", _outFileName));
                return false;
            }
            if (Settings.PacketErrorsFileName == string.Empty)
                File.Delete(_outFileName);
            writer = new StreamWriter(_outFileName, false);
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
                if (packet.Status == ParsedStatus.WithErrors)
                {
                    subPackets.Append(packet.GetHeader());
                    subPackets.AppendLine(PacketFileProcessor.Current.GetProcessor<TextBuilder>().LastPacket);
                }
                return;
            }
            StreamWriter w = writer;
            if (packet.Status == ParsedStatus.WithErrors)
            {
                w.Write(packet.GetHeader());
                w.WriteLine(PacketFileProcessor.Current.GetProcessor<TextBuilder>().LastPacket);
            }
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
