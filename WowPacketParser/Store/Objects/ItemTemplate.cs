﻿using PacketParser.Enums;
using WowPacketParser.SQL;

namespace PacketParser.DataStructures
{
    [DBTableName("item_template")]
    {
        [DBFieldName("class")]
        public ItemClass Class;

        [DBFieldName("subclass")]
        public uint SubClass;

        [DBFieldName("SoundOverrideSubclass")]
        public int SoundOverrideSubclass;

        [DBFieldName("name")]
        public string Name;

        [DBFieldName("displayid")]
        public uint DisplayId;

        [DBFieldName("Quality")]
        public ItemQuality Quality;

        [DBFieldName("Flags")]
        public ItemFlag Flags;

        [DBFieldName("FlagsExtra")]
        public ItemFlagExtra ExtraFlags;

        [DBFieldName("Unk430_1")]
        public float Unk430_1;

        [DBFieldName("Unk430_2")]
        public float Unk430_2;

        [DBFieldName("BuyCount")]
        public uint BuyCount;

        [DBFieldName("BuyPrice")]
        public long BuyPrice;

        [DBFieldName("SellPrice")]
        public uint SellPrice;

        [DBFieldName("InventoryType")]
        public InventoryType InventoryType;

        [DBFieldName("AllowableClass")]
        public ClassMask AllowedClasses;

        [DBFieldName("AllowableRace")]
        public RaceMask AllowedRaces;

        [DBFieldName("ItemLevel")]
        public uint ItemLevel;

        [DBFieldName("RequiredLevel")]
        public uint RequiredLevel;

        [DBFieldName("RequiredSkill")]
        public uint RequiredSkillId;

        [DBFieldName("RequiredSkillRank")]
        public uint RequiredSkillLevel;

        [DBFieldName("requiredspell")]
        public uint RequiredSpell;

        [DBFieldName("requiredhonorrank")]
        public uint RequiredHonorRank;

        [DBFieldName("RequiredCityRank")]
        public uint RequiredCityRank;

        [DBFieldName("RequiredReputationFaction")]
        public uint RequiredRepFaction;

        [DBFieldName("RequiredReputationRank")]
        public uint RequiredRepValue;

        [DBFieldName("maxcount")]
        public int MaxCount;

        [DBFieldName("stackable")]
        public int MaxStackSize;

        [DBFieldName("ContainerSlots")]
        public uint ContainerSlots;

        public uint StatsCount;

        [DBFieldName("stat_type", Count = 10, StartAtZero = false)]
        public ItemModType[] StatTypes;

        [DBFieldName("stat_value", Count = 10, StartAtZero = false)]
        public int[] StatValues;

        [DBFieldName("stat_unk1_", Count = 10, StartAtZero = false)]
        public int[] StatUnk1;

        [DBFieldName("stat_unk2_", Count = 10, StartAtZero = false)]
        public int[] StatUnk2;

        [DBFieldName("ScalingStatDistribution")]
        public int ScalingStatDistribution;

        public uint ScalingStatValue;

        public float[] DamageMins;
        public float[] DamageMaxs;
        public DamageType[] DamageTypes;
        public DamageType[] Resistances;

        [DBFieldName("DamageType")]
        public DamageType DamageType;

        [DBFieldName("delay")]
        public uint Delay;

        public AmmoType AmmoType;

        [DBFieldName("RangedModRange")]
        public float RangedMod;

        [DBFieldName("spellid_", Count = 5, StartAtZero = false)]
        public int[] TriggeredSpellIds;

        [DBFieldName("spelltrigger_", Count = 5, StartAtZero = false)]
        public ItemSpellTriggerType[] TriggeredSpellTypes;

        [DBFieldName("spellcharges_", Count = 5, StartAtZero = false)]
        public int[] TriggeredSpellCharges;

        [DBFieldName("spellcooldown_", Count = 5, StartAtZero = false)]
        public int[] TriggeredSpellCooldowns;

        [DBFieldName("spellcategory_", Count = 5, StartAtZero = false)]
        public uint[] TriggeredSpellCategories;

        [DBFieldName("spellcategorycooldown_", Count = 5, StartAtZero = false)]
        public int[] TriggeredSpellCategoryCooldowns;

        [DBFieldName("bonding")]
        public ItemBonding Bonding;

        [DBFieldName("description")]
        public string Description;

        [DBFieldName("PageText")]
        public uint PageText;

        [DBFieldName("LanguageID")]
        public Language Language;

        [DBFieldName("PageMaterial")]
        public PageMaterial PageMaterial;

        [DBFieldName("startquest")]
        public uint StartQuestId;

        [DBFieldName("lockid")]
        public uint LockId;

        [DBFieldName("Material")]
        public Material Material;

        [DBFieldName("sheath")]
        public SheathType SheathType;

        [DBFieldName("RandomProperty")]
        public int RandomPropery;

        [DBFieldName("RandomSuffix")]
        public uint RandomSuffix;

        public uint Block;

        [DBFieldName("itemset")]
        public uint ItemSet;

        public uint MaxDurability;

        [DBFieldName("area")]
        public uint AreaId;

        [DBFieldName("Map")]
        public int MapId;

        [DBFieldName("BagFamily")]
        public BagFamilyMask BagFamily;

        [DBFieldName("TotemCategory")]
        public TotemCategory TotemCategory;

        [DBFieldName("socketColor_", Count = 3, StartAtZero = false)]
        public ItemSocketColor[] ItemSocketColors;

        [DBFieldName("socketContent_", Count = 3, StartAtZero = false)]
        public uint[] SocketContent;

        [DBFieldName("socketBonus")]
        public int SocketBonus;

        [DBFieldName("GemProperties")]
        public int GemProperties;

        public int RequiredDisenchantSkill;

        [DBFieldName("ArmorDamageModifier")]
        public float ArmorDamageModifier;

        [DBFieldName("duration")]
        public uint Duration;

        [DBFieldName("ItemLimitCategory")]
        public int ItemLimitCategory;

        [DBFieldName("HolidayId")]
        public Holiday HolidayId;

        [DBFieldName("StatScalingFactor")]
        public float StatScalingFactor;

        [DBFieldName("CurrencySubstitutionId")]
        public uint CurrencySubstitutionId;

        [DBFieldName("CurrencySubstitutionCount")]
        public uint CurrencySubstitutionCount;

        [DBFieldName("WDBVerified")]
        public int WDBVerified;
    }
}
