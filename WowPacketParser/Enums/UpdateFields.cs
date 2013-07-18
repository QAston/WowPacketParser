namespace PacketParser.Enums
{
    // ReSharper disable InconsistentNaming, UnusedMember.Global
    public enum ObjectField
    {
        OBJECT_FIELD_GUID,
        OBJECT_FIELD_TYPE,
        OBJECT_FIELD_ENTRY,
        OBJECT_FIELD_SCALE_X,
        OBJECT_FIELD_PADDING,
        OBJECT_END,
        OBJECT_FIELD_DATA,
    }

    public enum ItemField
    {
        ITEM_END,
        ITEM_FIELD_CONTAINED,
        ITEM_FIELD_CREATE_PLAYED_TIME,
        ITEM_FIELD_CREATOR,
        ITEM_FIELD_DURABILITY,
        ITEM_FIELD_DURATION,
        ITEM_FIELD_ENCHANTMENT_10_1,
        ITEM_FIELD_ENCHANTMENT_10_3,
        ITEM_FIELD_ENCHANTMENT_11_1,
        ITEM_FIELD_ENCHANTMENT_11_3,
        ITEM_FIELD_ENCHANTMENT_12_1,
        ITEM_FIELD_ENCHANTMENT_12_3,
        ITEM_FIELD_ENCHANTMENT_13_1,
        ITEM_FIELD_ENCHANTMENT_13_3,
        ITEM_FIELD_ENCHANTMENT_14_1,
        ITEM_FIELD_ENCHANTMENT_14_3,
        ITEM_FIELD_ENCHANTMENT_15_1,
        ITEM_FIELD_ENCHANTMENT_15_3,
        ITEM_FIELD_ENCHANTMENT_1_1,
        ITEM_FIELD_ENCHANTMENT_1_3,
        ITEM_FIELD_ENCHANTMENT_2_1,
        ITEM_FIELD_ENCHANTMENT_2_3,
        ITEM_FIELD_ENCHANTMENT_3_1,
        ITEM_FIELD_ENCHANTMENT_3_3,
        ITEM_FIELD_ENCHANTMENT_4_1,
        ITEM_FIELD_ENCHANTMENT_4_3,
        ITEM_FIELD_ENCHANTMENT_5_1,
        ITEM_FIELD_ENCHANTMENT_5_3,
        ITEM_FIELD_ENCHANTMENT_6_1,
        ITEM_FIELD_ENCHANTMENT_6_3,
        ITEM_FIELD_ENCHANTMENT_7_1,
        ITEM_FIELD_ENCHANTMENT_7_3,
        ITEM_FIELD_ENCHANTMENT_8_1,
        ITEM_FIELD_ENCHANTMENT_8_3,
        ITEM_FIELD_ENCHANTMENT_9_1,
        ITEM_FIELD_ENCHANTMENT_9_3,
        ITEM_FIELD_FLAGS,
        ITEM_FIELD_GIFTCREATOR,
        ITEM_FIELD_ITEM_TEXT_ID,
        ITEM_FIELD_MAXDURABILITY,
        ITEM_FIELD_OWNER,
        ITEM_FIELD_PAD,
        ITEM_FIELD_PROPERTY_SEED,
        ITEM_FIELD_RANDOM_PROPERTIES_ID,
        ITEM_FIELD_SPELL_CHARGES,
        ITEM_FIELD_STACK_COUNT,
    }

    public enum ContainerField
    {
        CONTAINER_ALIGN_PAD,
        CONTAINER_END,
        CONTAINER_FIELD_NUM_SLOTS,
        CONTAINER_FIELD_SLOT_1,
    }

    public enum UnitField
    {
        UNIT_CHANNEL_SPELL,
        UNIT_CREATED_BY_SPELL,
        UNIT_DYNAMIC_FLAGS,
        UNIT_END,
        UNIT_FIELD_ATTACK_POWER,
        UNIT_FIELD_ATTACK_POWER_MODS,
        UNIT_FIELD_ATTACK_POWER_MOD_NEG,
        UNIT_FIELD_ATTACK_POWER_MOD_POS,
        UNIT_FIELD_ATTACK_POWER_MULTIPLIER,
        UNIT_FIELD_AURASTATE,
        UNIT_FIELD_BASEATTACKTIME,
        UNIT_FIELD_BASE_HEALTH,
        UNIT_FIELD_BASE_MANA,
        UNIT_FIELD_BOUNDINGRADIUS,
        UNIT_FIELD_BYTES_0,
        UNIT_FIELD_BYTES_1,
        UNIT_FIELD_BYTES_2,
        UNIT_FIELD_CHANNEL_OBJECT,
        UNIT_FIELD_CHARM,
        UNIT_FIELD_CHARMEDBY,
        UNIT_FIELD_COMBATREACH,
        UNIT_FIELD_CREATEDBY,
        UNIT_FIELD_CRITTER,
        UNIT_FIELD_DISPLAYID,
        UNIT_FIELD_FACTIONTEMPLATE,
        UNIT_FIELD_FLAGS,
        UNIT_FIELD_FLAGS_2,
        UNIT_FIELD_HEALTH,
        UNIT_FIELD_HOVERHEIGHT,
        UNIT_FIELD_VIRTUAL_ITEM_ID1,
        UNIT_FIELD_VIRTUAL_ITEM_ID2,
        UNIT_FIELD_VIRTUAL_ITEM_ID3,
        UNIT_FIELD_LEVEL,
        UNIT_FIELD_MAXDAMAGE,
        UNIT_FIELD_MAXHEALTH,
        UNIT_FIELD_MAXHEALTHMODIFIER,
        UNIT_FIELD_MAXITEMLEVEL,
        UNIT_FIELD_MAXOFFHANDDAMAGE,
        UNIT_FIELD_MAXPOWER1,
        UNIT_FIELD_MAXPOWER10,
        UNIT_FIELD_MAXPOWER11,
        UNIT_FIELD_MAXPOWER2,
        UNIT_FIELD_MAXPOWER3,
        UNIT_FIELD_MAXPOWER4,
        UNIT_FIELD_MAXPOWER5,
        UNIT_FIELD_MAXPOWER6,
        UNIT_FIELD_MAXPOWER7,
        UNIT_FIELD_MAXPOWER8,
        UNIT_FIELD_MAXPOWER9,
        UNIT_FIELD_MAXRANGEDDAMAGE,
        UNIT_FIELD_MINDAMAGE,
        UNIT_FIELD_MINOFFHANDDAMAGE,
        UNIT_FIELD_MINRANGEDDAMAGE,
        UNIT_FIELD_MOUNTDISPLAYID,
        UNIT_FIELD_NATIVEDISPLAYID,
        UNIT_FIELD_NEGSTAT0,
        UNIT_FIELD_NEGSTAT1,
        UNIT_FIELD_NEGSTAT2,
        UNIT_FIELD_NEGSTAT3,
        UNIT_FIELD_NEGSTAT4,
        UNIT_FIELD_PADDING,
        UNIT_FIELD_PETEXPERIENCE,
        UNIT_FIELD_PETNEXTLEVELEXP,
        UNIT_FIELD_PETNUMBER,
        UNIT_FIELD_PET_NAME_TIMESTAMP,
        UNIT_FIELD_POSSTAT0,
        UNIT_FIELD_POSSTAT1,
        UNIT_FIELD_POSSTAT2,
        UNIT_FIELD_POSSTAT3,
        UNIT_FIELD_POSSTAT4,
        UNIT_FIELD_POWER1,
        UNIT_FIELD_POWER10,
        UNIT_FIELD_POWER11,
        UNIT_FIELD_POWER2,
        UNIT_FIELD_POWER3,
        UNIT_FIELD_POWER4,
        UNIT_FIELD_POWER5,
        UNIT_FIELD_POWER6,
        UNIT_FIELD_POWER7,
        UNIT_FIELD_POWER8,
        UNIT_FIELD_POWER9,
        UNIT_FIELD_POWER_COST_MODIFIER,
        UNIT_FIELD_POWER_COST_MULTIPLIER,
        UNIT_FIELD_POWER_COST_MULTIPLIER1,
        UNIT_FIELD_POWER_COST_MULTIPLIER2,
        UNIT_FIELD_POWER_COST_MULTIPLIER3,
        UNIT_FIELD_POWER_COST_MULTIPLIER4,
        UNIT_FIELD_POWER_COST_MULTIPLIER5,
        UNIT_FIELD_POWER_COST_MULTIPLIER6,
        UNIT_FIELD_POWER_COST_MULTIPLIER7,
        UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER,
        UNIT_FIELD_POWER_REGEN_INTERRUPTED_FLAT_MODIFIER,
        UNIT_FIELD_RANGEDATTACKTIME,
        UNIT_FIELD_RANGED_ATTACK_POWER,
        UNIT_FIELD_RANGED_ATTACK_POWER_MODS,
        UNIT_FIELD_RANGED_ATTACK_POWER_MOD_NEG,
        UNIT_FIELD_RANGED_ATTACK_POWER_MOD_POS,
        UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_ARCANE,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_ARMOR,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_FIRE,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_FROST,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_HOLY,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_NATURE,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_SHADOW,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_ARCANE,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_ARMOR,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_FIRE,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_FROST,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_HOLY,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_NATURE,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_SHADOW,
        UNIT_FIELD_RESISTANCES,
        UNIT_FIELD_RESISTANCES_ARCANE,
        UNIT_FIELD_RESISTANCES_ARMOR,
        UNIT_FIELD_RESISTANCES_FIRE,
        UNIT_FIELD_RESISTANCES_FROST,
        UNIT_FIELD_RESISTANCES_HOLY,
        UNIT_FIELD_RESISTANCES_NATURE,
        UNIT_FIELD_RESISTANCES_SHADOW,
        UNIT_FIELD_STAT0,
        UNIT_FIELD_STAT1,
        UNIT_FIELD_STAT2,
        UNIT_FIELD_STAT3,
        UNIT_FIELD_STAT4,
        UNIT_FIELD_SUMMON,
        UNIT_FIELD_SUMMONEDBY,
        UNIT_FIELD_TARGET,
        UNIT_FIELD_UNK63,
        UNIT_MOD_CAST_HASTE,
        UNIT_MOD_CAST_SPEED,
        UNIT_NPC_EMOTESTATE,
        UNIT_NPC_FLAGS,
        UNIT_FIELD_DISPLAY_POWER,
        UNIT_FIELD_ANIMTIER,
        UNIT_FIELD_SHAPESHIFT_FORM,
        UNIT_VIRTUAL_ITEM_SLOT_ID1,
        UNIT_VIRTUAL_ITEM_SLOT_ID2,
        UNIT_VIRTUAL_ITEM_SLOT_ID3,
        UNIT_FIELD_END,
    }

    public enum PlayerField
    {
        PLAYER_AMMO_ID,
        PLAYER_BLOCK_PERCENTAGE,
        PLAYER_BYTES,
        PLAYER_BYTES_2,
        PLAYER_BYTES_3,
        PLAYER_CHARACTER_POINTS,
        PLAYER_CHARACTER_POINTS1,
        PLAYER_CHARACTER_POINTS2,
        PLAYER_CHOSEN_TITLE,
        PLAYER_CRIT_PERCENTAGE,
        PLAYER_DODGE_PERCENTAGE,
        PLAYER_DUEL_ARBITER,
        PLAYER_DUEL_TEAM,
        PLAYER_END,
        PLAYER_EXPERTISE,
        PLAYER_EXPLORED_ZONES_1,
        PLAYER_FAKE_INEBRIATION,
        PLAYER_FARSIGHT,
        PLAYER_FIELD_ARENA_CURRENCY,
        PLAYER_FIELD_ARENA_TEAM_INFO_1_1,
        PLAYER_FIELD_BANKBAG_SLOT_1,
        PLAYER_FIELD_BANK_SLOT_1,
        PLAYER_FIELD_BATTLEGROUND_RATING,
        PLAYER_FIELD_BUYBACK_PRICE_1,
        PLAYER_FIELD_BUYBACK_PRICE_10,
        PLAYER_FIELD_BUYBACK_PRICE_11,
        PLAYER_FIELD_BUYBACK_PRICE_12,
        PLAYER_FIELD_BUYBACK_PRICE_2,
        PLAYER_FIELD_BUYBACK_PRICE_3,
        PLAYER_FIELD_BUYBACK_PRICE_4,
        PLAYER_FIELD_BUYBACK_PRICE_5,
        PLAYER_FIELD_BUYBACK_PRICE_6,
        PLAYER_FIELD_BUYBACK_PRICE_7,
        PLAYER_FIELD_BUYBACK_PRICE_8,
        PLAYER_FIELD_BUYBACK_PRICE_9,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_1,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_10,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_11,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_12,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_2,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_3,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_4,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_5,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_6,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_7,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_8,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_9,
        PLAYER_FIELD_BYTES,
        PLAYER_FIELD_BYTES2,
        PLAYER_FIELD_COINAGE,
        PLAYER_FIELD_COMBAT_RATING_1,
        PLAYER_FIELD_CURRENCYTOKEN_SLOT_1,
        PLAYER_FIELD_DAILY_QUESTS_1,
        PLAYER_FIELD_GLYPHS_1,
        PLAYER_FIELD_GLYPHS_2,
        PLAYER_FIELD_GLYPHS_3,
        PLAYER_FIELD_GLYPHS_4,
        PLAYER_FIELD_GLYPHS_5,
        PLAYER_FIELD_GLYPHS_6,
        PLAYER_FIELD_GLYPH_SLOTS_1,
        PLAYER_FIELD_GLYPH_SLOTS_2,
        PLAYER_FIELD_GLYPH_SLOTS_3,
        PLAYER_FIELD_GLYPH_SLOTS_4,
        PLAYER_FIELD_GLYPH_SLOTS_5,
        PLAYER_FIELD_GLYPH_SLOTS_6,
        PLAYER_FIELD_HOME_REALM_TIME_OFFSET,
        PLAYER_FIELD_HONOR_CURRENCY,
        PLAYER_FIELD_INV_SLOT_FIXME1,
        PLAYER_FIELD_INV_SLOT_FIXME10,
        PLAYER_FIELD_INV_SLOT_FIXME11,
        PLAYER_FIELD_INV_SLOT_FIXME12,
        PLAYER_FIELD_INV_SLOT_FIXME13,
        PLAYER_FIELD_INV_SLOT_FIXME14,
        PLAYER_FIELD_INV_SLOT_FIXME15,
        PLAYER_FIELD_INV_SLOT_FIXME16,
        PLAYER_FIELD_INV_SLOT_FIXME17,
        PLAYER_FIELD_INV_SLOT_FIXME18,
        PLAYER_FIELD_INV_SLOT_FIXME19,
        PLAYER_FIELD_INV_SLOT_FIXME2,
        PLAYER_FIELD_INV_SLOT_FIXME20,
        PLAYER_FIELD_INV_SLOT_FIXME21,
        PLAYER_FIELD_INV_SLOT_FIXME22,
        PLAYER_FIELD_INV_SLOT_FIXME3,
        PLAYER_FIELD_INV_SLOT_FIXME4,
        PLAYER_FIELD_INV_SLOT_FIXME5,
        PLAYER_FIELD_INV_SLOT_FIXME6,
        PLAYER_FIELD_INV_SLOT_FIXME7,
        PLAYER_FIELD_INV_SLOT_FIXME8,
        PLAYER_FIELD_INV_SLOT_FIXME9,
        PLAYER_FIELD_INV_SLOT_HEAD,
        PLAYER_FIELD_KEYRING_SLOT_1,
        PLAYER_FIELD_KILLS,
        PLAYER_FIELD_KNOWN_CURRENCIES,
        PLAYER_FIELD_KNOWN_TITLES,
        PLAYER_FIELD_KNOWN_TITLES1,
        PLAYER_FIELD_KNOWN_TITLES2,
        PLAYER_FIELD_LIFETIME_HONORABLE_KILLS,
        PLAYER_FIELD_MAX_LEVEL,
        PLAYER_FIELD_MOD_DAMAGE_DONE_NEG,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT1,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT2,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT3,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT4,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT5,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT6,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT7,
        PLAYER_FIELD_MOD_DAMAGE_DONE_POS,
        PLAYER_FIELD_MOD_HASTE,
        PLAYER_FIELD_MOD_HASTE_REGEN,
        PLAYER_FIELD_MOD_HEALING_DONE_PCT,
        PLAYER_FIELD_MOD_HEALING_DONE_POS,
        PLAYER_FIELD_MOD_HEALING_PCT,
        PLAYER_FIELD_MOD_PET_HASTE,
        PLAYER_FIELD_MOD_RANGED_HASTE,
        PLAYER_FIELD_MOD_SPELL_POWER_PCT,
        PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE,
        PLAYER_FIELD_MOD_TARGET_RESISTANCE,
        PLAYER_FIELD_PACK_SLOT_1,
        PLAYER_FIELD_PADDING,
        PLAYER_FIELD_PAD_0,
        PLAYER_FIELD_PVP_MEDALS,
        PLAYER_FIELD_RESEARCHING_1,
        PLAYER_FIELD_RESERACH_SITE_1,
        PLAYER_FIELD_TODAY_CONTRIBUTION,
        PLAYER_FIELD_UI_HIT_MODIFIER,
        PLAYER_FIELD_UI_SPELL_HIT_MODIFIER,
        PLAYER_FIELD_VENDORBUYBACK_SLOT_1,
        PLAYER_FIELD_WATCHED_FACTION_INDEX,
        PLAYER_FIELD_WEAPON_DMG_MULTIPLIERS,
        PLAYER_FIELD_YESTERDAY_CONTRIBUTION,
        PLAYER_FLAGS,
        PLAYER_GLYPHS_ENABLED,
        PLAYER_GUILDDELETE_DATE,
        PLAYER_GUILDID,
        PLAYER_GUILDLEVEL,
        PLAYER_GUILDRANK,
        PLAYER_GUILD_TIMESTAMP,
        PLAYER_MASTERY,
        PLAYER_NEXT_LEVEL_XP,
        PLAYER_NO_REAGENT_COST_1,
        PLAYER_OFFHAND_CRIT_PERCENTAGE,
        PLAYER_OFFHAND_EXPERTISE,
        PLAYER_PARRY_PERCENTAGE,
        PLAYER_PET_SPELL_POWER,
        PLAYER_PROFESSION_SKILL_LINE_1,
        PLAYER_QUEST_LOG_10_1,
        PLAYER_QUEST_LOG_10_2,
        PLAYER_QUEST_LOG_10_3,
        PLAYER_QUEST_LOG_10_5,
        PLAYER_QUEST_LOG_11_1,
        PLAYER_QUEST_LOG_11_2,
        PLAYER_QUEST_LOG_11_3,
        PLAYER_QUEST_LOG_11_5,
        PLAYER_QUEST_LOG_12_1,
        PLAYER_QUEST_LOG_12_2,
        PLAYER_QUEST_LOG_12_3,
        PLAYER_QUEST_LOG_12_5,
        PLAYER_QUEST_LOG_13_1,
        PLAYER_QUEST_LOG_13_2,
        PLAYER_QUEST_LOG_13_3,
        PLAYER_QUEST_LOG_13_5,
        PLAYER_QUEST_LOG_14_1,
        PLAYER_QUEST_LOG_14_2,
        PLAYER_QUEST_LOG_14_3,
        PLAYER_QUEST_LOG_14_5,
        PLAYER_QUEST_LOG_15_1,
        PLAYER_QUEST_LOG_15_2,
        PLAYER_QUEST_LOG_15_3,
        PLAYER_QUEST_LOG_15_5,
        PLAYER_QUEST_LOG_16_1,
        PLAYER_QUEST_LOG_16_2,
        PLAYER_QUEST_LOG_16_3,
        PLAYER_QUEST_LOG_16_5,
        PLAYER_QUEST_LOG_17_1,
        PLAYER_QUEST_LOG_17_2,
        PLAYER_QUEST_LOG_17_3,
        PLAYER_QUEST_LOG_17_5,
        PLAYER_QUEST_LOG_18_1,
        PLAYER_QUEST_LOG_18_2,
        PLAYER_QUEST_LOG_18_3,
        PLAYER_QUEST_LOG_18_5,
        PLAYER_QUEST_LOG_19_1,
        PLAYER_QUEST_LOG_19_2,
        PLAYER_QUEST_LOG_19_3,
        PLAYER_QUEST_LOG_19_5,
        PLAYER_QUEST_LOG_1_1,
        PLAYER_QUEST_LOG_1_2,
        PLAYER_QUEST_LOG_1_3,
        PLAYER_QUEST_LOG_1_5,
        PLAYER_QUEST_LOG_20_1,
        PLAYER_QUEST_LOG_20_2,
        PLAYER_QUEST_LOG_20_3,
        PLAYER_QUEST_LOG_20_5,
        PLAYER_QUEST_LOG_21_1,
        PLAYER_QUEST_LOG_21_2,
        PLAYER_QUEST_LOG_21_3,
        PLAYER_QUEST_LOG_21_5,
        PLAYER_QUEST_LOG_22_1,
        PLAYER_QUEST_LOG_22_2,
        PLAYER_QUEST_LOG_22_3,
        PLAYER_QUEST_LOG_22_5,
        PLAYER_QUEST_LOG_23_1,
        PLAYER_QUEST_LOG_23_2,
        PLAYER_QUEST_LOG_23_3,
        PLAYER_QUEST_LOG_23_5,
        PLAYER_QUEST_LOG_24_1,
        PLAYER_QUEST_LOG_24_2,
        PLAYER_QUEST_LOG_24_3,
        PLAYER_QUEST_LOG_24_5,
        PLAYER_QUEST_LOG_25_1,
        PLAYER_QUEST_LOG_25_2,
        PLAYER_QUEST_LOG_25_3,
        PLAYER_QUEST_LOG_25_5,
        PLAYER_QUEST_LOG_26_1,
        PLAYER_QUEST_LOG_26_2,
        PLAYER_QUEST_LOG_26_3,
        PLAYER_QUEST_LOG_26_5,
        PLAYER_QUEST_LOG_27_1,
        PLAYER_QUEST_LOG_27_2,
        PLAYER_QUEST_LOG_27_3,
        PLAYER_QUEST_LOG_27_5,
        PLAYER_QUEST_LOG_28_1,
        PLAYER_QUEST_LOG_28_2,
        PLAYER_QUEST_LOG_28_3,
        PLAYER_QUEST_LOG_28_5,
        PLAYER_QUEST_LOG_29_1,
        PLAYER_QUEST_LOG_29_2,
        PLAYER_QUEST_LOG_29_3,
        PLAYER_QUEST_LOG_29_5,
        PLAYER_QUEST_LOG_2_1,
        PLAYER_QUEST_LOG_2_2,
        PLAYER_QUEST_LOG_2_3,
        PLAYER_QUEST_LOG_2_5,
        PLAYER_QUEST_LOG_30_1,
        PLAYER_QUEST_LOG_30_2,
        PLAYER_QUEST_LOG_30_3,
        PLAYER_QUEST_LOG_30_5,
        PLAYER_QUEST_LOG_31_1,
        PLAYER_QUEST_LOG_31_2,
        PLAYER_QUEST_LOG_31_3,
        PLAYER_QUEST_LOG_31_5,
        PLAYER_QUEST_LOG_32_1,
        PLAYER_QUEST_LOG_32_2,
        PLAYER_QUEST_LOG_32_3,
        PLAYER_QUEST_LOG_32_5,
        PLAYER_QUEST_LOG_33_1,
        PLAYER_QUEST_LOG_33_2,
        PLAYER_QUEST_LOG_33_3,
        PLAYER_QUEST_LOG_33_5,
        PLAYER_QUEST_LOG_34_1,
        PLAYER_QUEST_LOG_34_2,
        PLAYER_QUEST_LOG_34_3,
        PLAYER_QUEST_LOG_34_5,
        PLAYER_QUEST_LOG_35_1,
        PLAYER_QUEST_LOG_35_2,
        PLAYER_QUEST_LOG_35_3,
        PLAYER_QUEST_LOG_35_5,
        PLAYER_QUEST_LOG_36_1,
        PLAYER_QUEST_LOG_36_2,
        PLAYER_QUEST_LOG_36_3,
        PLAYER_QUEST_LOG_36_5,
        PLAYER_QUEST_LOG_37_1,
        PLAYER_QUEST_LOG_37_2,
        PLAYER_QUEST_LOG_37_3,
        PLAYER_QUEST_LOG_37_5,
        PLAYER_QUEST_LOG_38_1,
        PLAYER_QUEST_LOG_38_2,
        PLAYER_QUEST_LOG_38_3,
        PLAYER_QUEST_LOG_38_5,
        PLAYER_QUEST_LOG_39_1,
        PLAYER_QUEST_LOG_39_2,
        PLAYER_QUEST_LOG_39_3,
        PLAYER_QUEST_LOG_39_5,
        PLAYER_QUEST_LOG_3_1,
        PLAYER_QUEST_LOG_3_2,
        PLAYER_QUEST_LOG_3_3,
        PLAYER_QUEST_LOG_3_5,
        PLAYER_QUEST_LOG_40_1,
        PLAYER_QUEST_LOG_40_2,
        PLAYER_QUEST_LOG_40_3,
        PLAYER_QUEST_LOG_40_5,
        PLAYER_QUEST_LOG_41_1,
        PLAYER_QUEST_LOG_41_2,
        PLAYER_QUEST_LOG_41_3,
        PLAYER_QUEST_LOG_41_5,
        PLAYER_QUEST_LOG_42_1,
        PLAYER_QUEST_LOG_42_2,
        PLAYER_QUEST_LOG_42_3,
        PLAYER_QUEST_LOG_42_5,
        PLAYER_QUEST_LOG_43_1,
        PLAYER_QUEST_LOG_43_2,
        PLAYER_QUEST_LOG_43_3,
        PLAYER_QUEST_LOG_43_5,
        PLAYER_QUEST_LOG_44_1,
        PLAYER_QUEST_LOG_44_2,
        PLAYER_QUEST_LOG_44_3,
        PLAYER_QUEST_LOG_44_5,
        PLAYER_QUEST_LOG_45_1,
        PLAYER_QUEST_LOG_45_2,
        PLAYER_QUEST_LOG_45_3,
        PLAYER_QUEST_LOG_45_5,
        PLAYER_QUEST_LOG_46_1,
        PLAYER_QUEST_LOG_46_2,
        PLAYER_QUEST_LOG_46_3,
        PLAYER_QUEST_LOG_46_5,
        PLAYER_QUEST_LOG_47_1,
        PLAYER_QUEST_LOG_47_2,
        PLAYER_QUEST_LOG_47_3,
        PLAYER_QUEST_LOG_47_5,
        PLAYER_QUEST_LOG_48_1,
        PLAYER_QUEST_LOG_48_2,
        PLAYER_QUEST_LOG_48_3,
        PLAYER_QUEST_LOG_48_5,
        PLAYER_QUEST_LOG_49_1,
        PLAYER_QUEST_LOG_49_2,
        PLAYER_QUEST_LOG_49_3,
        PLAYER_QUEST_LOG_49_5,
        PLAYER_QUEST_LOG_4_1,
        PLAYER_QUEST_LOG_4_2,
        PLAYER_QUEST_LOG_4_3,
        PLAYER_QUEST_LOG_4_5,
        PLAYER_QUEST_LOG_50_1,
        PLAYER_QUEST_LOG_50_2,
        PLAYER_QUEST_LOG_50_3,
        PLAYER_QUEST_LOG_50_5,
        PLAYER_QUEST_LOG_5_1,
        PLAYER_QUEST_LOG_5_2,
        PLAYER_QUEST_LOG_5_3,
        PLAYER_QUEST_LOG_5_5,
        PLAYER_QUEST_LOG_6_1,
        PLAYER_QUEST_LOG_6_2,
        PLAYER_QUEST_LOG_6_3,
        PLAYER_QUEST_LOG_6_5,
        PLAYER_QUEST_LOG_7_1,
        PLAYER_QUEST_LOG_7_2,
        PLAYER_QUEST_LOG_7_3,
        PLAYER_QUEST_LOG_7_5,
        PLAYER_QUEST_LOG_8_1,
        PLAYER_QUEST_LOG_8_2,
        PLAYER_QUEST_LOG_8_3,
        PLAYER_QUEST_LOG_8_5,
        PLAYER_QUEST_LOG_9_1,
        PLAYER_QUEST_LOG_9_2,
        PLAYER_QUEST_LOG_9_3,
        PLAYER_QUEST_LOG_9_5,
        PLAYER_RANGED_CRIT_PERCENTAGE,
        PLAYER_REST_STATE_EXPERIENCE,
        PLAYER_RUNE_REGEN_1,
        PLAYER_RUNE_REGEN_2,
        PLAYER_RUNE_REGEN_3,
        PLAYER_RUNE_REGEN_4,
        PLAYER_SELF_RES_SPELL,
        PLAYER_SHIELD_BLOCK,
        PLAYER_SHIELD_BLOCK_CRIT_PERCENTAGE,
        PLAYER_SKILL_INFO_1_1,
        PLAYER_SPELL_CRIT_PERCENTAGE1,
        PLAYER_SPELL_CRIT_PERCENTAGE2,
        PLAYER_SPELL_CRIT_PERCENTAGE3,
        PLAYER_SPELL_CRIT_PERCENTAGE4,
        PLAYER_SPELL_CRIT_PERCENTAGE5,
        PLAYER_SPELL_CRIT_PERCENTAGE6,
        PLAYER_SPELL_CRIT_PERCENTAGE7,
        PLAYER_TRACK_CREATURES,
        PLAYER_TRACK_RESOURCES,
        PLAYER_VISIBLE_ITEM_10_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_10_ENTRYID,
        PLAYER_VISIBLE_ITEM_11_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_11_ENTRYID,
        PLAYER_VISIBLE_ITEM_12_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_12_ENTRYID,
        PLAYER_VISIBLE_ITEM_13_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_13_ENTRYID,
        PLAYER_VISIBLE_ITEM_14_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_14_ENTRYID,
        PLAYER_VISIBLE_ITEM_15_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_15_ENTRYID,
        PLAYER_VISIBLE_ITEM_16_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_16_ENTRYID,
        PLAYER_VISIBLE_ITEM_17_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_17_ENTRYID,
        PLAYER_VISIBLE_ITEM_18_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_18_ENTRYID,
        PLAYER_VISIBLE_ITEM_19_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_19_ENTRYID,
        PLAYER_VISIBLE_ITEM_1_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_1_ENTRYID,
        PLAYER_VISIBLE_ITEM_2_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_2_ENTRYID,
        PLAYER_VISIBLE_ITEM_3_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_3_ENTRYID,
        PLAYER_VISIBLE_ITEM_4_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_4_ENTRYID,
        PLAYER_VISIBLE_ITEM_5_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_5_ENTRYID,
        PLAYER_VISIBLE_ITEM_6_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_6_ENTRYID,
        PLAYER_VISIBLE_ITEM_7_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_7_ENTRYID,
        PLAYER_VISIBLE_ITEM_8_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_8_ENTRYID,
        PLAYER_VISIBLE_ITEM_9_ENCHANTMENT,
        PLAYER_VISIBLE_ITEM_9_ENTRYID,
        PLAYER_XP,
        PLAYER__FIELD_KNOWN_TITLES,
        PLAYER__FIELD_KNOWN_TITLES1,
        PLAYER__FIELD_KNOWN_TITLES2,
    }

    public enum GameObjectField
    {
        GAMEOBJECT_BYTES_1,
        GAMEOBJECT_DISPLAYID,           // 5.x: GAMEOBJECT_FIELD_DISPLAYID
        GAMEOBJECT_DYNAMIC,
        GAMEOBJECT_END,
        GAMEOBJECT_FACTION,             // 5.x: GAMEOBJECT_FIELD_FACTION_TEMPLATE
        GAMEOBJECT_FIELD_CREATED_BY,    // 5.x: GAMEOBJECT_FIELD_CREATEDBY
        GAMEOBJECT_FLAGS,               // 5.x: GAMEOBJECT_FIELD_FLAGS
        GAMEOBJECT_LEVEL,               // 5.x: GAMEOBJECT_FIELD_LEVEL
        GAMEOBJECT_PARENTROTATION,      // 5.x: GAMEOBJECT_FIELD_PARENT_ROTATION
        GAMEOBJECT_FIELD_ANIM_PROGRESS, // 5.x only
    }

    public enum DynamicObjectField
    {
        DYNAMICOBJECT_BYTES,
        DYNAMICOBJECT_CASTER,
        DYNAMICOBJECT_CASTTIME,
        DYNAMICOBJECT_END,
        DYNAMICOBJECT_RADIUS,
        DYNAMICOBJECT_SPELLID,
    }

    public enum CorpseField
    {
        CORPSE_END,
        CORPSE_FIELD_BYTES_1,
        CORPSE_FIELD_BYTES_2,
        CORPSE_FIELD_DISPLAY_ID,
        CORPSE_FIELD_DYNAMIC_FLAGS,
        CORPSE_FIELD_FLAGS,
        CORPSE_FIELD_GUILD,
        CORPSE_FIELD_ITEM,
        CORPSE_FIELD_OWNER,
        CORPSE_FIELD_PAD,
        CORPSE_FIELD_PARTY,
    }

    public enum AreaTriggerField
    {
        AREATRIGGER_DURATION,
        AREATRIGGER_END,
        AREATRIGGER_FINAL_POS,
        AREATRIGGER_SPELLID,
        AREATRIGGER_SPELLVISUALID,
    }
    // ReSharper restore InconsistentNaming, UnusedMember.Global
}
