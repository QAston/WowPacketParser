﻿using System;
using System.Collections.Generic;
using System.Reflection;
using PacketParser.Misc;
using System.Collections.Specialized;
using Guid = PacketParser.DataStructures.Guid;
using PacketParser.DataStructures;
using System.Text;

namespace PacketParser.Enums.Version
{
    public static class UpdateFields
    {
        [ThreadStatic]
        private static Dictionary<Type, BiDictionary<string, int>> UpdateFieldDictionaries = null;

        private static Dictionary<Type, BiDictionary<string, int>> LoadUFDictionaries()
        {
            Type[] enumTypes = {
                               typeof (ObjectField), typeof (ItemField), typeof (ContainerField), typeof (UnitField),
                               typeof (PlayerField), typeof (GameObjectField), typeof (DynamicObjectField),
                               typeof (CorpseField), typeof (AreaTriggerField)
                           };

            var dicts = new Dictionary<Type, BiDictionary<string, int>>();

            foreach (var enumType in enumTypes)
            {
                var vTypeString = string.Format("PacketParser.Enums.Version.{0}.{1}", GetUpdateFieldDictionaryBuildName(ClientVersion.Build), enumType.Name);
                var vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                if (vEnumType == null)
                    continue;   // versions prior to 4.3.0 do not have AreaTriggerField

                var vValues = Enum.GetValues(vEnumType);
                var vNames = Enum.GetNames(vEnumType);

                var fieldNumbers = new SortedSet<int>();

                var result = new BiDictionary<string, int>();

                for (var i = 0; i < vValues.Length; ++i)
                {
                    result.Add(vNames[i], (int)vValues.GetValue(i));
                    fieldNumbers.Add((int)vValues.GetValue(i));
                }

                dicts.Add(enumType, result);

                // add fields not defined in enums - generate names for arrays
                var numbersEnum = fieldNumbers.GetEnumerator();
                if (numbersEnum.MoveNext())
                {
                    int currentField = numbersEnum.Current;
                    while (numbersEnum.MoveNext())
                    {

                        int nextField = numbersEnum.Current;

                        string name;

                        name = result.GetBySecond(currentField);

                        var t = GetUpdateFieldType(name);
                        int size = UpdateFieldDictionary.GetFieldsCount(t);
                        int newField = currentField + size;
                        if (newField < nextField)
                        {
                            string nameBase = name.Substring(0, name.LastIndexOf('_') + 1);
                            int number;
                            if (!Int32.TryParse(name.Substring(name.LastIndexOf('_') + 1), out number))
                            {
                                number = 1;
                                nameBase = name + "_";
                            }

                            while (newField < nextField)
                            {
                                result.Add(nameBase + (++number), newField);
                                newField += size;
                            }
                        }

                        currentField = nextField;
                    }
                }
            }
            
            return dicts;
        }

        public static void InitForClientVersion()
        {
            UpdateFieldDictionaries = LoadUFDictionaries();
        }

        // returns update field offset by generic - crossversion enum
        public static int? GetUpdateFieldOffset<T>(T field) where T: struct, IConvertible
        {
            if (UpdateFieldDictionaries.ContainsKey(typeof(T)))
            {
                int offset;
                if (UpdateFieldDictionaries[typeof(T)].TryGetByFirst(Enum<T>.ToString(field), out offset))
                    return offset;
            }

            return null;
        }

        // returns update field name by offset
        private static string GetUpdateFieldName(int fieldOffset, Type t)
        {
            if (UpdateFieldDictionaries.ContainsKey(t))
            {
                string name;
                if (UpdateFieldDictionaries[t].TryGetBySecond(fieldOffset, out name))
                    return name;
            }

            return null;
        }

        public static string GetUpdateFieldNameByOffset(Int32 offset, ObjectType type)
        {
            return GetUpdateFieldName(offset, GetUpdateFieldEnumByOffset(offset, type));
        }

        public static string GetUpdateFieldNameByOffset<T>(int fieldOffset)
        {
            return GetUpdateFieldName(fieldOffset, typeof(T));
        }

        public static Type GetUpdateFieldType(string name)
        {
            var ret = typeof(int);
            int i = name.Length - 1;
            // trim indexes from field name
            for (; i > 0; --i)
            {
                if ((name[i] >= '0' && name[i] <= '9') || name[i]=='_')
                    continue;
                break;
            }
            switch (name.Substring(0, i+1))
            {
                case "PLAYER_FIELD_PACK_SLOT":
                case "PLAYER_FIELD_BANK_SLOT":
                case "PLAYER_FIELD_BANKBAG_SLOT":
                case "PLAYER_FIELD_VENDORBUYBACK_SLOT":
                case "PLAYER_FIELD_KEYRING_SLOT":
                case "PLAYER_FIELD_CURRENCYTOKEN_SLOT":
                case "CONTAINER_FIELD_SLOT":
                case "OBJECT_FIELD_GUID":
                case "ITEM_FIELD_OWNER":
                case "ITEM_FIELD_CONTAINED":
                case "ITEM_FIELD_GIFTCREATOR":
                case "ITEM_FIELD_CREATOR":
                case "UNIT_FIELD_CHARM":
                case "UNIT_FIELD_SUMMON":
                case "UNIT_FIELD_CRITTER":
                case "UNIT_FIELD_CHARMEDBY":
                case "UNIT_FIELD_SUMMONEDBY":
                case "UNIT_FIELD_CREATEDBY":
                case "UNIT_FIELD_TARGET":
                case "UNIT_FIELD_CHANNEL_OBJECT":
                case "PLAYER_DUEL_ARBITER":
                case "PLAYER_FIELD_INV_SLOT_HEAD":
                case "PLAYER_FARSIGHT":
                case "GAMEOBJECT_FIELD_CREATED_BY":
                case "DYNAMICOBJECT_CASTER":
                case "CORPSE_FIELD_OWNER":
                case "CORPSE_FIELD_PARTY":
                case "OBJECT_FIELD_CREATED_BY":
                    return typeof(Guid);
                case "PLAYER__FIELD_KNOWN_TITLES":
                case "PLAYER_FIELD_KNOWN_CURRENCIES":
                case "OBJECT_FIELD_DATA":
                    return typeof(UInt64);
                case "OBJECT_FIELD_SCALE_X":
                case "GAMEOBJECT_PARENTROTATION":
                case "DYNAMICOBJECT_RADIUS":
                case "UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER":
                case "UNIT_FIELD_POWER_REGEN_INTERRUPTED_FLAT_MODIFIER":
                case "UNIT_FIELD_BOUNDINGRADIUS":
                case "UNIT_FIELD_COMBATREACH":
                case "UNIT_FIELD_MINDAMAGE":
                case "UNIT_FIELD_MAXDAMAGE":
                case "UNIT_FIELD_MINOFFHANDDAMAGE":
                case "UNIT_FIELD_MAXOFFHANDDAMAGE":
                case "UNIT_MOD_CAST_SPEED":
                case "UNIT_MOD_CAST_HASTE":
                case "UNIT_FIELD_ATTACK_POWER_MULTIPLIER":
                case "UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER":
                case "UNIT_FIELD_MINRANGEDDAMAGE":
                case "UNIT_FIELD_MAXRANGEDDAMAGE":
                case "UNIT_FIELD_POWER_COST_MULTIPLIER":
                case "UNIT_FIELD_MAXHEALTHMODIFIER":
                case "UNIT_FIELD_HOVERHEIGHT":
                case "PLAYER_BLOCK_PERCENTAGE":
                case "PLAYER_DODGE_PERCENTAGE":
                case "PLAYER_PARRY_PERCENTAGE":
                case "PLAYER_CRIT_PERCENTAGE":
                case "PLAYER_RANGED_CRIT_PERCENTAGE":
                case "PLAYER_OFFHAND_CRIT_PERCENTAGE":
                case "PLAYER_SPELL_CRIT_PERCENTAGE":
                case "PLAYER_SHIELD_BLOCK_CRIT_PERCENTAGE":
                case "PLAYER_FIELD_MOD_HEALING_PCT":
                case "PLAYER_FIELD_MOD_HEALING_DONE_PCT":
                case "PLAYER_RUNE_REGEN":
                    return typeof(float);
                case "ITEM_FIELD_FLAGS":
                    return typeof(StoreEnum<ItemFlag>);
                case "UNIT_FIELD_FLAGS":
                    if (name.EndsWith("2"))
                        return typeof(StoreEnum<UnitFlags2>);
                    return typeof(StoreEnum<UnitFlags>);
                case "UNIT_DYNAMIC_FLAGS":
                    return typeof(StoreEnum<UnitDynamicFlags>);
                case "UNIT_NPC_FLAGS":
                    return typeof(StoreEnum<NPCFlags>);
                case "PLAYER_FLAGS":
                    return typeof(StoreEnum<PlayerFlags>);
                case "GAMEOBJECT_FLAGS":
                    return typeof(StoreEnum<GameObjectFlag>);
                case "CORPSE_FIELD_FLAGS":
                    return typeof(StoreEnum<CorpseFlags>);
                case "CORPSE_FIELD_DYNAMIC_FLAGS":
                    return typeof(StoreEnum<CorpseDynamicFlags>);
                case "OBJECT_FIELD_TYPE":
                    return typeof(Shorts<ObjectTypeFlags1, ObjectTypeFlags2>);
                case "GAMEOBJECT_DYNAMIC":
                    return typeof(Shorts<UInt16, GameObjectDynamicFlag>);
                case "UNIT_FIELD_BYTES":
                    if (name.EndsWith("1"))
                        // byte 1: free talent points
                        return typeof(Bytes<UnitStandStateType, byte, UnitStandFlags, UnitBytes1Flags>);
                    if (name.EndsWith("2"))
                        return typeof(Bytes<SheathState, UnitBytes2Flags1, UnitBytes2Flags2, ShapeshiftForm>);
                    // bytes 0
                    return typeof(Bytes<Race, Class, Gender, PowerType>);
                case "PLAYER_FIELD_BYTES":
                    if (name.EndsWith("2"))
                        //                  spelloverrideId(lo,hi), unk
                        return typeof(Bytes<byte, byte, byte, PlayerBytes2Flags3>);
                    // bytes 0               , actionbars, unk
                    return typeof(Bytes<PlayerBytesFlags0, PlayerBytesFlags1, byte, byte>);
                case "PLAYER_BYTES":
                    if (name.EndsWith("2"))
                        //                 facial hair, unk ,bank bag slot count
                        return typeof(Bytes<byte, byte, byte, RestState>);
                    if (name.EndsWith("3"))
                        //                  gender, drunk, unk, arenaFaction
                        return typeof(Bytes<Gender, byte, byte, byte>);
                    // bytes 0
                    //                  skin   face  hair    haircolor
                    return typeof(Bytes<byte, byte, byte, byte>);
                case "GAMEOBJECT_BYTES":
                    //                                                     artKit, animProgress
                    return typeof(Bytes<GameObjectState, GameObjectType, byte, byte>);
                case "CORPSE_FIELD_BYTES":
                    if (name.EndsWith("2"))
                        //                 face, hairstyle ,haircolor, facialhair
                        return typeof(Bytes<byte, byte, byte, byte>);
                    // bytes1:          unk,                 skin
                    return typeof(Bytes<byte, Race, Gender, byte>);
                default:
                    return typeof(int);
            }
        }

        public static string GetUpdateFieldBytesInfo(string name)
        {
            StringBuilder keyBuilder = new StringBuilder();
            switch (name)
            {
                case "UNIT_FIELD_BYTES_0":
                    keyBuilder.Append("<Race, Class, Gender, PowerType>");
                    break;
                case "UNIT_FIELD_BYTES_1":
                    keyBuilder.Append("<StandStateType, FreeTalentPoints, StandFlags, Bytes1Flags>");
                    break;
                case "UNIT_FIELD_BYTES_2":
                    keyBuilder.Append("<SheathState, Bytes2Flags1, Bytes2Flags2, ShapeshiftForm>");
                    break;
                case "PLAYER_FIELD_BYTES":
                    keyBuilder.Append("<PlayerBytesFlags0, PlayerBytesFlags1, Actionbars, Unk>");
                    break;
                case "PLAYER_FIELD_BYTES2":
                    keyBuilder.Append("<SpellOverrideId(lo,hi), Unk, PlayerBytes2Flags3>");
                    break;
                case "PLAYER_BYTES":
                    keyBuilder.Append("<Skin, Face, Hair, Haircolor>");
                    break;
                case "PLAYER_BYTES_2":
                    keyBuilder.Append("<FacialHair, Unk, BankBagSlotCount, RestState>");
                    break;
                case "PLAYER_BYTES_3":
                    keyBuilder.Append("<Gender, Drunk, Unk, ArenaFaction>");
                    break;
                case "GAMEOBJECT_BYTES_1":
                    keyBuilder.Append("<GameObjectState, GameObjectType, ArtKit, AnimProgress>");
                    break;
                case "CORPSE_FIELD_BYTES_1":
                    keyBuilder.Append("<Face, Hairstyle, Haircolor, Facialhair>");
                    break;
                case "CORPSE_FIELD_BYTES_2":
                    keyBuilder.Append("<Unk, Race, Gender, Skin>");
                    break;
                case "DYNAMICOBJECT_BYTES":
                    keyBuilder.Append("<[0..27]SpellVisual, DynObjType>");
                    break;
            }
            return keyBuilder.ToString();
        }


        public static Type GetUpdateFieldEnumByOffset(Int32 offset, ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Object:
                    return typeof(ObjectField);
                case ObjectType.Item:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<ItemField>(ItemField.ITEM_END))
                            throw new Exception("Incorrect object type" + type + " for given offset "+ offset);
                        return typeof(ItemField);
                    }
                case ObjectType.Container:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ItemField>(ItemField.ITEM_END);
                        if (offset < max)
                            goto case ObjectType.Item;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<ContainerField>(ContainerField.CONTAINER_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(ContainerField);
                    }
                case ObjectType.Unit:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<UnitField>(UnitField.UNIT_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(UnitField);
                    }
                case ObjectType.Player:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<UnitField>(UnitField.UNIT_END);
                        if (offset < max)
                            goto case ObjectType.Unit;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<PlayerField>(PlayerField.PLAYER_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(PlayerField);
                    }
                case ObjectType.GameObject:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<GameObjectField>(GameObjectField.GAMEOBJECT_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(GameObjectField);
                    }
                case ObjectType.DynamicObject:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<DynamicObjectField>(DynamicObjectField.DYNAMICOBJECT_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(DynamicObjectField);
                    }
                case ObjectType.Corpse:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<CorpseField>(CorpseField.CORPSE_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(CorpseField);
                    }
                case ObjectType.AreaTrigger:
                    {
                        int max = (int)Enums.Version.UpdateFields.GetUpdateFieldOffset<ObjectField>(ObjectField.OBJECT_END);
                        if (offset < max)
                            goto case ObjectType.Object;
                        if (offset >= Enums.Version.UpdateFields.GetUpdateFieldOffset<AreaTriggerField>(AreaTriggerField.AREATRIGGER_END))
                            throw new Exception("Incorrect object type" + type + " for given offset " + offset);
                        return typeof(AreaTriggerField);
                    }
                default:
                    return typeof(Object);
            }
        }

        private static string GetUpdateFieldDictionaryBuildName(ClientVersionBuild build)
        {
            switch (build)
            {
                case ClientVersionBuild.V2_4_3_8606:
                case ClientVersionBuild.V3_0_2_9056:
                case ClientVersionBuild.V3_0_3_9183:
                case ClientVersionBuild.V3_0_8_9464:
                case ClientVersionBuild.V3_0_8a_9506:
                case ClientVersionBuild.V3_0_9_9551:
                case ClientVersionBuild.V3_1_0_9767:
                case ClientVersionBuild.V3_1_1_9806:
                case ClientVersionBuild.V3_1_1a_9835:
                case ClientVersionBuild.V3_1_2_9901:
                case ClientVersionBuild.V3_1_3_9947:
                case ClientVersionBuild.V3_2_0_10192:
                case ClientVersionBuild.V3_2_0a_10314:
                case ClientVersionBuild.V3_2_2_10482:
                case ClientVersionBuild.V3_2_2a_10505:
                case ClientVersionBuild.V3_3_0_10958:
                case ClientVersionBuild.V3_3_0a_11159:
                {
                    return "V3_3_0_10958";
                }
                case ClientVersionBuild.V3_3_3_11685:
                case ClientVersionBuild.V3_3_3a_11723:
                case ClientVersionBuild.V3_3_5a_12340:
                {
                    return "V3_3_5a_12340";
                }
                case ClientVersionBuild.V4_0_3_13329:
                {
                    return "V4_0_3_13329";
                }
                case ClientVersionBuild.V4_0_6_13596:
                case ClientVersionBuild.V4_0_6a_13623:
                case ClientVersionBuild.V4_1_0_13914:
                case ClientVersionBuild.V4_1_0a_14007:
                {
                    return "V4_0_6_13596";
                }
                case ClientVersionBuild.V4_2_0_14333:
                case ClientVersionBuild.V4_2_0a_14480:
                {
                    return "V4_2_0_14480";
                }
                case ClientVersionBuild.V4_2_2_14545:
                {
                    return "V4_2_2_14545";
                }
                case ClientVersionBuild.V4_3_0_15005:
                case ClientVersionBuild.V4_3_0a_15050:
                {
                    return "V4_3_0_15005";
                }
                case ClientVersionBuild.V4_3_2_15211:
                {
                    return "V4_3_2_15211";
                }
                case ClientVersionBuild.V4_3_3_15354:
                {
                    return "V4_3_3_15354";
                }
                case ClientVersionBuild.V4_3_4_15595:
                {
                    return "V4_3_4_15595";
                }
                case ClientVersionBuild.V5_0_4_16016:
                {
                    return "V5_0_4_16016";
                }
                case ClientVersionBuild.V5_0_5_16048:
                case ClientVersionBuild.V5_0_5a_16057:
                case ClientVersionBuild.V5_0_5b_16135:
                {
                    return "V5_0_5_16048";
                }
                case ClientVersionBuild.V5_1_0_16309:
                case ClientVersionBuild.V5_1_0a_16357:
                {
                    return "V5_1_0_16309";
                }
                case ClientVersionBuild.V5_2_0_16650:
                case ClientVersionBuild.V5_2_0_16669:
                case ClientVersionBuild.V5_2_0_16683:
                case ClientVersionBuild.V5_2_0_16685:
                case ClientVersionBuild.V5_2_0_16701:
                case ClientVersionBuild.V5_2_0_16709:
                case ClientVersionBuild.V5_2_0_16716:
                case ClientVersionBuild.V5_2_0_16733:
                case ClientVersionBuild.V5_2_0_16769:
                case ClientVersionBuild.V5_2_0_16826:
                {
                    return "V5_2_0_16650";
                }
                case ClientVersionBuild.V5_3_0_16981:
                case ClientVersionBuild.V5_3_0_16983:
                case ClientVersionBuild.V5_3_0_16992:
                case ClientVersionBuild.V5_3_0_17055:
                case ClientVersionBuild.V5_3_0_17116:
                case ClientVersionBuild.V5_3_0_17128:
                {
                    return "V5_3_0_16981";
                }
                default:
                {
                    return "V3_3_5a_12340";
                }
            }
        }
    }
}
