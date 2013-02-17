using System;
using System.Collections.Generic;
using PacketParser.Enums;
using PacketParser.Enums.Version;
using PacketParser.Misc;
using PacketParser.Processing;
using PacketDumper.Enums;
using PacketDumper.Misc;
using PacketParser.DataStructures;
using PacketParser.SQL;

namespace PacketDumper.Processing.SQLData
{
    public class GossipPOIStore : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return null; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return ProcessPacket; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return null; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return null; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        // Points of Interest
        public readonly TimeSpanDictionary<uint, GossipPOI> GossipPOIs = new TimeSpanDictionary<uint, GossipPOI>();
        public uint LastGossipPOIEntry = 0;

        public bool Init(PacketFileProcessor file)
        {
            return Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.points_of_interest);
        }

        public void ProcessPacket(Packet packet)
        {
            if (packet.Status != ParsedStatus.Success)
                return;

            if (Opcode.SMSG_GOSSIP_POI == Opcodes.GetOpcode(packet.Opcode))
            {
                GossipPOIs.Add(++LastGossipPOIEntry, packet.GetData().GetNode<GossipPOI>("GosipPOI"), packet.TimeSpan);
            }
        }

        public void Finish()
        {

        }

        public string Build()
        {
            if (GossipPOIs.IsEmpty())
                return string.Empty;

            var result = string.Empty;

            if (Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.gossip_menu_option))
            {
                var GossipSelects = PacketFileProcessor.Current.GetProcessor<NpcGossipStore>().GossipSelects;
                var gossipPOIsTable = new Dictionary<Tuple<uint, uint>, uint>();

                foreach (var poi in GossipPOIs)
                {
                    foreach (var gossipSelect in GossipSelects)
                    {
                        var tuple = Tuple.Create(gossipSelect.Key.Item1, gossipSelect.Key.Item2);

                        if (gossipPOIsTable.ContainsKey(tuple))
                            continue;

                        var timeSpan = poi.Value.Item2 - gossipSelect.Value.Item2;
                        if (timeSpan != null && timeSpan.Value.Duration() <= TimeSpan.FromSeconds(1))
                            gossipPOIsTable.Add(tuple, poi.Key);
                    }
                }

                var rowsUpd = new List<QueryBuilder.SQLUpdateRow>();

                foreach (var u in gossipPOIsTable)
                {
                    var row = new QueryBuilder.SQLUpdateRow();

                    row.AddValue("action_poi_id", u.Value);

                    row.AddWhere("menu_id", u.Key.Item1);
                    row.AddWhere("id", u.Key.Item2);

                    row.Table = "gossip_menu_option";

                    rowsUpd.Add(row);
                }

                result += new QueryBuilder.SQLUpdate(rowsUpd).Build();
            }

            const string tableName = "points_of_interest";
            var rowsIns = new List<QueryBuilder.SQLInsertRow>();

            uint count = 0;

            foreach (var poi in GossipPOIs)
            {
                var row = new QueryBuilder.SQLInsertRow();

                row.AddValue("entry", "@ID+" + count, noQuotes: true);
                row.AddValue("x", poi.Value.Item1.XPos);
                row.AddValue("y", poi.Value.Item1.YPos);
                row.AddValue("icon", poi.Value.Item1.Icon);
                row.AddValue("flags", poi.Value.Item1.Flags);
                row.AddValue("data", poi.Value.Item1.Data);
                row.AddValue("icon_name", poi.Value.Item1.IconName);

                rowsIns.Add(row);
                count++;
            }

            result += new QueryBuilder.SQLDelete(Tuple.Create("@ID+0", "@ID+" + (count - 1)), "entry", tableName).Build();
            result += new QueryBuilder.SQLInsert(tableName, rowsIns, withDelete: false).Build();

            return result;
        }
    }
}
