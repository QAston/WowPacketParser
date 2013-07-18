using System;
using System.Collections.Generic;
using System.Globalization;
using PacketParser.Enums;
using PacketParser.Misc;

namespace PacketParser.DataStructures
{
    public class Unit : WoWObject
    {
        public Unit()
        {
        }
        public Unit(WoWObject rhs) : base(rhs)
        {
        }
        public List<Aura> SpawnAuras;

        public Dictionary<uint, Aura> Auras = new  Dictionary<uint, Aura>();

        public override bool IsTemporarySpawn()
        {
            if (ForceTemporarySpawn)
                return true;

            // If our unit got any of the following update fields set,
            // it's probably a temporary spawn
            UpdateField uf;
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_0_5_16048))
            {
                if (UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(UnitField.UNIT_FIELD_SUMMONEDBY), out uf) ||
                    UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(UnitField.UNIT_FIELD_CREATEDBY), out uf))
                    return uf.UInt32Value != 0;
            }
            else
            {
                if (UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(UnitField.UNIT_FIELD_SUMMONEDBY), out uf) ||
                    UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(UnitField.UNIT_CREATED_BY_SPELL), out uf) ||
                    UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(UnitField.UNIT_FIELD_CREATEDBY), out uf))
                    return uf.UInt32Value != 0;
            }

            return false;
        }

        public uint GossipId;

        // Fields from UPDATE_FIELDS - populated on SMSG_UPDATE_OBJECT in Create
        public float? Size;
        public uint? Bytes0;
        public uint? MaxHealth;
        public uint? Level;
        public uint? Faction;
        public uint?[] Equipment;
        public UnitFlags? UnitFlags;
        public UnitFlags2? UnitFlags2;
        public uint? MeleeTime;
        public uint? RangedTime;
        public uint? Model;
        public uint? Mount;
        public uint? Bytes1;
        public UnitDynamicFlags? DynamicFlags;
        public NPCFlags? NpcFlags;
        public EmoteType? EmoteState;
        public uint? ManaMod;
        public uint? HealthMod;
        public uint? Bytes2;
        public float? BoundingRadius;
        public float? CombatReach;
        public float? HoverHeight;

        // Fields calculated with bytes0
        public PowerType? PowerType;
        public Gender? Gender;
        public Class? Class;
        public Race? Race;

        // Must be called AFTER LoadValuesFromUpdateFields
        private void ComputeBytes0()
        {
            if (Bytes0 == null || Bytes0 == 0)
            {
                PowerType = null;
                Gender = null;
                Class = null;
                Race = null;
                return;
            }

            PowerType = (PowerType) ((Bytes0 & 0x7FFFFFFF) >> 24);
            Gender    = (Gender)    ((Bytes0 & 0x00FF0000) >> 16);
            Class     = (Class)     ((Bytes0 & 0x0000FF00) >>  8);
            Race      = (Race)      ((Bytes0 & 0x000000FF) >>  0);
        }

        public override void LoadValuesFromUpdateFields()
        {
            Size          = GetValue<ObjectField, float?>(ObjectField.OBJECT_FIELD_SCALE_X);
            Bytes0        = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BYTES_0);
            MaxHealth     = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_MAXHEALTH);
            Level         = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_LEVEL);
            Faction       = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_FACTIONTEMPLATE);
            Equipment     = GetArray<UnitField, uint?>(UnitField.UNIT_VIRTUAL_ITEM_SLOT_ID1, 3);
            UnitFlags     = GetEnum<UnitField, UnitFlags?>(UnitField.UNIT_FIELD_FLAGS);
            UnitFlags2    = GetEnum<UnitField, UnitFlags2?>(UnitField.UNIT_FIELD_FLAGS_2);
            MeleeTime     = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BASEATTACKTIME);
            RangedTime    = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_RANGEDATTACKTIME);
            Model         = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_DISPLAYID);
            Mount         = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_MOUNTDISPLAYID);
            Bytes1        = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BYTES_1);
            DynamicFlags  = GetEnum<UnitField, UnitDynamicFlags?>(UnitField.UNIT_DYNAMIC_FLAGS);
            NpcFlags      = GetEnum<UnitField, NPCFlags?>(UnitField.UNIT_NPC_FLAGS);
            EmoteState    = GetEnum<UnitField, EmoteType?>(UnitField.UNIT_NPC_EMOTESTATE);
            //Resistances   = GetArray<UnitField, uint>(UnitField.UNIT_FIELD_RESISTANCES_ARMOR, 7);
            ManaMod       = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BASE_MANA);
            HealthMod     = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BASE_HEALTH);
            Bytes2        = GetValue<UnitField, uint?>(UnitField.UNIT_FIELD_BYTES_2);
            BoundingRadius= GetValue<UnitField, float?>(UnitField.UNIT_FIELD_BOUNDINGRADIUS);
            CombatReach   = GetValue<UnitField, float?>(UnitField.UNIT_FIELD_COMBATREACH);
            HoverHeight   = GetValue<UnitField, float?>(UnitField.UNIT_FIELD_HOVERHEIGHT);

            ComputeBytes0();
        }
    }


}
