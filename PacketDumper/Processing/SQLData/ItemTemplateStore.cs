using System;
using System.Collections.Generic;
using PacketParser.Enums;
using PacketParser.Enums.Version;
using PacketParser.Misc;
using PacketParser.Processing;
using PacketParser.SQL;
using PacketParser.DataStructures;
using System.Reflection;
using PacketDumper.Enums;
using PacketDumper.Misc;

namespace PacketDumper.Processing.SQLData
{
    public class ItemTemplateStore : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return null; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return ProcessPacket; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return null; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return null; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        public readonly TimeSpanDictionary<uint, ItemTemplate> ItemTemplates = new TimeSpanDictionary<uint, ItemTemplate>();
        public bool Init(PacketFileProcessor file)
        {
            return Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.item_template);
        }

        public void ProcessPacket(Packet packet)
        {
            if (packet.Status != ParsedStatus.Success)
                return;

            if (Opcode.SMSG_ITEM_QUERY_SINGLE_RESPONSE == Opcodes.GetOpcode(packet.Opcode))
            {
                var entry = packet.GetData().GetNode<KeyValuePair<int, bool>>("Entry");

                if (entry.Value) // entry is masked
                    return;

                ItemTemplates.Add((uint)entry.Key, packet.GetNode<ItemTemplate>("ItemTemplateObject"), packet.TimeSpan);
            }
            else if (Opcode.SMSG_DB_REPLY == Opcodes.GetOpcode(packet.Opcode))
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_3_0_15005))
                {
                    var itemId2 = packet.ReadEntryWithName<UInt32>(StoreNameType.Item, "Entry");
                    // not found - add
                    if (!ItemTemplates.ContainsKey((uint)itemId2))
                    {
                        ItemTemplates.Add((uint)itemId2, packet.GetNode<ItemTemplate>("ItemTemplateObject"), packet.TimeSpan);
                    }
                    else
                    {
                        // item found - replace all properties of 
                        var item = ItemTemplates[(uint)itemId2].Item1;

                        var newItem = packet.GetNode<ItemTemplate>("ItemTemplateObject");

                        var emptyItem = new ItemTemplate();

                        Type t = typeof(ItemTemplate);

                        FieldInfo[] properties = t.GetFields();

                        foreach (FieldInfo pi in properties)
                        {
                            if (pi.GetValue(item) == pi.GetValue(emptyItem))
                                pi.SetValue(item, pi.GetValue(newItem));
                        }
                    }
                }
            }
        }

        public void Finish()
        {

        }

        public string Build()
        {
            if (ItemTemplates.IsEmpty())
                return String.Empty;

            var entries = ItemTemplates.Keys();
            var tempatesDb = SQLDatabase.GetDict<uint, ItemTemplate>(entries);

            return SQLUtil.CompareDicts(ItemTemplates, tempatesDb, StoreNameType.Item);
        }
    }
}
