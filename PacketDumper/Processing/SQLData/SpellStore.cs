using System;
using System.Collections.Generic;
using PacketParser.Misc;
using PacketParser.Enums;
using PacketParser.Enums.Version;
using PacketParser.Processing;
using PacketDumper.Enums;
using Guid = PacketParser.DataStructures.Guid;
using PacketDumper.Misc;
using PacketDumper.DataStructures;
using PacketParser.DataStructures;
using PacketParser.SQL;
using System.IO;
using System.Text;

namespace PacketDumper.Processing
{
    public class SpellStore : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return null; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return null; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return ProcessedPacket; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return ProcessData; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        public static readonly List<QueryBuilder.SQLInsertRow> SpellAuraRows = new List<QueryBuilder.SQLInsertRow>();
        public static readonly List<QueryBuilder.SQLInsertRow> SpellStartRows = new List<QueryBuilder.SQLInsertRow>();
        public static readonly List<QueryBuilder.SQLInsertRow> SpellGoRows = new List<QueryBuilder.SQLInsertRow>();
        public static readonly List<QueryBuilder.SQLInsertRow> SpellOpcodeRows = new List<QueryBuilder.SQLInsertRow>();
        public static string fileName;

        public bool Init(PacketFileProcessor file)
        {
            fileName = Path.GetFileName(file.FileName);
            return Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.SpellStart | SQLOutput.SpellGo | SQLOutput.SpellAura | SQLOutput.SpellOpcode);
        }

        public void ProcessData(string name, int? index, Object obj, Type t, Packet packet)
        {
            if (obj.GetType() == typeof(StoreEntry))
            {
                if (Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.SpellOpcode))
                {
                    var entry = (StoreEntry)obj;

                    if (entry._type == StoreNameType.Spell)
                    {
                        var row = new QueryBuilder.SQLInsertRow();
                        row.AddValue("build", ClientVersion.Build);
                        row.AddValue("sniffName", fileName);
                        row.AddValue("spellId", entry._data);
                        row.AddValue("opcode", Opcodes.GetOpcode(packet.Opcode).ToString());
                        SpellOpcodeRows.Add(row);
                    }
                }
            }
        }

        public void ProcessedPacket(Packet packet)
        {
            if (packet.Status == ParsedStatus.Success)
            {
                switch(Opcodes.GetOpcode(packet.Opcode))
                {
                    case Opcode.SMSG_AURA_UPDATE_ALL:
                    case Opcode.SMSG_AURA_UPDATE:
                        {
                            var guid = packet.GetData().GetNode<Guid>("GUID");
                            if (Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.SpellAura))
                            {
                                var auras = packet.GetData().GetNode<IndexedTreeNode>("Auras");
                                foreach (var aura in auras)
                                {
                                    NamedTreeNode a;
                                    if (aura.Value.TryGetNode<NamedTreeNode>(out a, "Aura"))
                                    {
                                        var spellId = a.GetNode<UInt32>("Spell ID");
                                        if (spellId > 0)
                                        {
                                            var row = new QueryBuilder.SQLInsertRow();
                                            row.AddValue("build", ClientVersion.Build);
                                            row.AddValue("sniffName", fileName);
                                            row.AddValue("spellId", spellId);
                                            row.AddValue("target", guid.Full);
                                            Guid casterGuid;
                                            if (!a.TryGetNode<Guid>(out casterGuid, "Caster GUID"))
                                                casterGuid = new Guid(0);
                                            row.AddValue("caster", casterGuid.Full);
                                            row.AddValue("auraFlags", (UInt32)a.GetNode<StoreEnum>("Flags").rawVal);

                                            SpellAuraRows.Add(row);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case Opcode.SMSG_SPELL_START:
                        {
                            if (Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.SpellStart))
                            {
                                var d = packet.GetData();

                                var row = new QueryBuilder.SQLInsertRow();
                                row.AddValue("build", ClientVersion.Build);
                                row.AddValue("sniffName", fileName);
                                row.AddValue("spellId", d.GetNode<Int32>("Spell ID"));
                                row.AddValue("caster", d.GetNode<Guid>("Caster GUID").Full);
                                row.AddValue("casterUnit", d.GetNode<Guid>("Caster Unit GUID").Full);

                                Guid targetGuid;
                                if (!d.TryGetNode<Guid>(out targetGuid, "Target GUID"))
                                    targetGuid = new Guid(0);
                                row.AddValue("target", targetGuid.Full);
                                Guid itemTargetGuid;
                                if (!d.TryGetNode<Guid>(out itemTargetGuid, "Item Target GUID"))
                                    itemTargetGuid = new Guid(0);
                                row.AddValue("itemTarget", itemTargetGuid.Full);
                                row.AddValue("castFlags", (UInt32)d.GetNode<StoreEnum>("Cast Flags").rawVal);
                                row.AddValue("targetFlags", (UInt32)d.GetNode<StoreEnum>("Target Flags").rawVal);
                                SpellStartRows.Add(row);
                            }
                        }
                        break;
                    case Opcode.SMSG_SPELL_GO:
                        {
                            if (Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.SpellGo))
                            {
                                var d = packet.GetData();
                                var row = new QueryBuilder.SQLInsertRow();
                                row.AddValue("build", ClientVersion.Build);
                                row.AddValue("sniffName", fileName);
                                row.AddValue("spellId", d.GetNode<Int32>("Spell ID"));
                                row.AddValue("caster", d.GetNode<Guid>("Caster GUID").Full);
                                row.AddValue("casterUnit", d.GetNode<Guid>("Caster Unit GUID").Full);

                                Guid targetGuid;
                                if (!d.TryGetNode<Guid>(out targetGuid, "Target GUID"))
                                    targetGuid = new Guid(0);
                                row.AddValue("target", targetGuid.Full);
                                Guid itemTargetGuid;
                                if (!d.TryGetNode<Guid>(out itemTargetGuid, "Item Target GUID"))
                                    itemTargetGuid = new Guid(0);
                                row.AddValue("itemTarget", itemTargetGuid.Full);
                                row.AddValue("castFlags", (UInt32)d.GetNode<StoreEnum>("Cast Flags").rawVal);
                                row.AddValue("targetFlags", (UInt32)d.GetNode<StoreEnum>("Target Flags").rawVal);
                                
                                row.AddValue("hitCount", d.GetNode<byte>("Hit Count"));
                                row.AddValue("missCount", d.GetNode<byte>("Miss Count"));
                                byte extraTargetsCount;
                                if (!d.TryGetNode<byte>(out extraTargetsCount, "Extra Targets Count"))
                                    extraTargetsCount = 0;
                                row.AddValue("extraTargetsCount", extraTargetsCount);
                                var misses = d.GetNode<IndexedTreeNode>("Miss Targets");
                                uint missMask = 0;
                                foreach (var miss in misses)
                                {
                                    missMask |= (uint)(1 << ((byte)miss.Value.GetNode<StoreEnum>("Miss Type").rawVal));
                                }
                                row.AddValue("missMask", missMask);
                                SpellGoRows.Add(row);
                            }
                        }
                        break;
                }
            }
        }

        public string Build()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(new QueryBuilder.SQLInsert("spell_start", SpellStartRows, ignore: true, withDelete: false, deleteDuplicates: true).Build());
            stringBuilder.AppendLine(new QueryBuilder.SQLInsert("spell_go", SpellGoRows, ignore: true, withDelete: false, deleteDuplicates: true).Build());
            stringBuilder.AppendLine(new QueryBuilder.SQLInsert("spell_opcode", SpellGoRows, ignore: true, withDelete: false, deleteDuplicates: true).Build());
            stringBuilder.AppendLine(new QueryBuilder.SQLInsert("spell_aura", SpellGoRows, ignore: true, withDelete: false, deleteDuplicates: true).Build());
            return stringBuilder.ToString();
        }

        public void Finish()
        {

        }
    }
}
