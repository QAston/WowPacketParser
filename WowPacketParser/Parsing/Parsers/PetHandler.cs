using System;
using PacketParser.Enums;
using PacketParser.Misc;
using PacketParser.Processing;
using Guid = PacketParser.DataStructures.Guid;
using PacketParser.DataStructures;

namespace PacketParser.Parsing.Parsers
{
    public static class PetHandler
    {
        public static void ReadPetFlags(ref Packet packet)
        {
            var petModeFlag = packet.ReadUInt32();
            packet.Store("React state", (ReactState)((petModeFlag >> 8) & 0xFF));
            packet.Store("Command state", (CommandState)((petModeFlag >> 16) & 0xFF));
            packet.Store("Flag", (PetModeFlags)(petModeFlag & 0xFFFF0000));
        }

        [Parser(Opcode.SMSG_PET_SPELLS)]
        public static void HandlePetSpells(Packet packet)
        {
            var guid = packet.ReadGuid("GUID");
            // Equal to "Clear spells" pre cataclysm
            if (guid.Full == 0)
                return;

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                packet.ReadEnum<CreatureFamily>("Pet Family", TypeCode.UInt16); // vehicles -> 0

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_1_0_16309))
                packet.ReadUInt16("Unk UInt16");

            packet.ReadUInt32("Expiration Time");

            ReadPetFlags(ref packet);

            packet.StoreBeginList("Spells/Actions");
            for (var i = 0; i < 10; i++) // Read pet/vehicle spell ids
            {
                var spell16 = packet.ReadUInt16();
                var spell8 = packet.ReadByte();
                var spellId = spell16 + (spell8 << 16);
                var slot = packet.ReadByte("Slot", i);

                if (spellId <= 4)
                    packet.Store("Action", spellId, i);
                else
                    packet.Store("Spell", new StoreEntry(StoreNameType.Spell, spellId), i);
            }
            packet.StoreEndList();

            var spellCount = packet.ReadByte("Spell Count"); // vehicles -> 0, pets -> != 0. Could this be auras?
            packet.StoreBeginList("Spells/auras?");
            for (var i = 0; i < spellCount; i++)
            {
                packet.ReadEntryWithName<UInt16>(StoreNameType.Spell, "Spell", i);
                packet.ReadInt16("Active", i);
            }
            packet.StoreEndList();

            var cdCount = packet.ReadByte("Cooldown count");
            packet.StoreBeginList("Cooldowns");
            for (var i = 0; i < cdCount; i++)
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell", i);
                else
                    packet.ReadEntryWithName<UInt16>(StoreNameType.Spell, "Spell", i);

                packet.ReadUInt16("Category", i);
                packet.ReadUInt32("Cooldown", i);
                packet.ReadUInt32("Category Cooldown", i);
            }
            packet.StoreEndList();

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_1_0_16309))
            {
                var unkLoopCounter = packet.ReadByte("Unk count");
                packet.StoreBeginList("UnkList");
                for (var i = 0; i < unkLoopCounter; i++)
                {
                    packet.ReadUInt32("Unk UInt32 1", i);
                    packet.ReadByte("Unk Byte", i);
                    packet.ReadUInt32("Unk UInt32 2", i);
                }
                packet.StoreEndList();
            }
        }

        [Parser(Opcode.SMSG_PET_TAME_FAILURE)]
        public static void HandlePetTameFailure(Packet packet)
        {
            packet.ReadEnum<PetTameFailureReason>("Reason", TypeCode.Byte);
        }

        [Parser(Opcode.CMSG_PET_NAME_QUERY)]
        public static void HandlePetNameQuery(Packet packet)
        {
            var number = packet.ReadUInt32("Pet number");
            var guid = packet.ReadGuid("Guid");

            // TimeSpanContainer temporary name (will be replaced in SMSG_PET_NAME_QUERY_RESPONSE)
            PacketFileProcessor.Current.GetProcessor<SessionStore>().PetGuids[number] = guid;
        }

        [Parser(Opcode.SMSG_PET_NAME_QUERY_RESPONSE)]
        public static void HandlePetNameQueryResponse(Packet packet)
        {
            var number = packet.ReadUInt32("Pet number");

            var petName = packet.ReadCString("Pet name");
            if (petName.Length == 0)
            {
                packet.ReadBytes(7); // 0s
                return;
            }

            Guid petGuid;
            if (PacketFileProcessor.Current.GetProcessor<SessionStore>().PetGuids.TryGetValue(number, out petGuid))
                PacketFileProcessor.Current.GetProcessor<NameStore>().AddPlayerName(petGuid, petName);

            packet.ReadTime("Time");
            var declined = packet.ReadBoolean("Declined");

            const int maxDeclinedNameCases = 5;

            if (declined)
            {
                packet.StoreBeginList("Declined names");
                for (var i = 0; i < maxDeclinedNameCases; i++)
                    packet.ReadCString("Declined name", i);
                packet.StoreEndList();
            }
        }

        [Parser(Opcode.SMSG_PET_MODE)]
        public static void HandlePetMode(Packet packet)
        {
            packet.ReadGuid("Guid");
            ReadPetFlags(ref packet);
        }

        [Parser(Opcode.SMSG_PET_ACTION_SOUND)]
        public static void HandlePetSound(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadInt32("Sound ID");
        }

        [Parser(Opcode.SMSG_PET_DISMISS_SOUND)]
        public static void HandlePetDismissSound(Packet packet)
        {
            packet.ReadInt32("Sound ID"); // CreatureSoundData.dbc - iRefID_soundPetDismissID
            packet.ReadVector3("Position");
        }

        [Parser(Opcode.CMSG_PET_SET_ACTION)]
        public static void HandlePetSetAction(Packet packet)
        {
            var i = 0;
            packet.ReadGuid("GUID");
            packet.StoreBeginList("Actions");
            while (packet.CanRead())
            {
                packet.ReadUInt32("Position", i);
                var action = (uint)packet.ReadUInt16() + (packet.ReadByte() << 16);
                packet.Store("Action", action, i);
                packet.ReadEnum<ActionButtonType>("Type", TypeCode.Byte, i++);
            }
            packet.StoreEndList();
        }

        [Parser(Opcode.CMSG_PET_ACTION)]
        public static void HandlePetAction(Packet packet)
        {
            packet.ReadGuid("GUID");
            var action = (uint)packet.ReadUInt16() + (packet.ReadByte() << 16);
            packet.Store("Action", action);
            packet.ReadEnum<ActionButtonType>("Type", TypeCode.Byte);
            packet.ReadGuid("GUID2");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_0_6_13596))
                packet.ReadVector3("Position");
        }

        [Parser(Opcode.CMSG_PET_CANCEL_AURA)]
        public static void HandlePetCancelAura(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
        }

        [Parser(Opcode.SMSG_PET_LEARNED_SPELL)]
        [Parser(Opcode.SMSG_PET_REMOVED_SPELL)]
        public static void HandlePetSpellsLearnedRemoved(Packet packet)
        {
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
        }

        [Parser(Opcode.SMSG_PET_ACTION_FEEDBACK)]
        public static void HandlePetActionFeedback(Packet packet)
        {
            var state = packet.ReadEnum<PetFeedback>("Pet state", TypeCode.Byte);

            switch (state)
            {
                case PetFeedback.NothingToAttack:
                    if (ClientVersion.AddedInVersion(ClientType.Cataclysm) || packet.CanRead())
                        packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
                    break;
                case PetFeedback.CantAttackTarget:
                    if (ClientVersion.AddedInVersion(ClientType.Cataclysm))
                        packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");    // sub_8ADA60 2nd parameter is SpellID, check sub_8B22C0
                    break;
            }
        }

        [Parser(Opcode.CMSG_PET_STOP_ATTACK)]
        [Parser(Opcode.CMSG_DISMISS_CRITTER)]
        [Parser(Opcode.CMSG_PET_ABANDON)]
        public static void HandlePetMiscGuid(Packet packet)
        {
            packet.ReadGuid("GUID");
        }

        [Parser(Opcode.SMSG_PET_UPDATE_COMBO_POINTS)]
        public static void HandlePetUpdateComboPoints(Packet packet)
        {
            packet.ReadPackedGuid("Guid 1");
            packet.ReadPackedGuid("Guid 2");
            packet.ReadByte("Combo points");
        }

        [Parser(Opcode.SMSG_PET_GUIDS)]
        public static void HandlePetGuids(Packet packet)
        {
            var count = packet.ReadInt32("Count");
            packet.StoreBeginList("Pet Guids");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadGuid("Guid", i);
            }
            packet.StoreEndList();
        }

        [Parser(Opcode.MSG_LIST_STABLED_PETS)]
        public static void HandleListStabledPets(Packet packet)
        {
            packet.ReadGuid("GUID");

            if (packet.Direction == Direction.ClientToServer)
                return;

            var count = packet.ReadByte("Count");
            packet.ReadByte("Stable Slots");
            packet.StoreBeginList("Stable pets");
            for (var i = 0; i < count; i++)
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545)) // not verified
                    packet.ReadInt32("Pet Slot", i);

                packet.ReadInt32("Pet Number", i);
                packet.ReadEntryWithName<UInt32>(StoreNameType.Unit, "Pet Entry", i);
                packet.ReadInt32("Pet Level", i);
                packet.ReadCString("Pet Name", i);
                packet.ReadByte("Stable Type", i); // 1 = current, 2/3 = in stable
            }
            packet.StoreEndList();
        }

        [Parser(Opcode.CMSG_PET_CAST_SPELL)]
        public static void HandlePetCastSpell(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadByte("Cast Count");
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
            var castFlags = packet.ReadEnum<CastFlag>("Cast Flags", TypeCode.Byte);
            SpellHandler.ReadSpellCastTargets(ref packet);
            if (castFlags.HasAnyFlag(CastFlag.HasTrajectory))
                SpellHandler.HandleSpellMissileAndMove(ref packet);
        }

        [Parser(Opcode.CMSG_REQUEST_PET_INFO)]
        public static void HandlePetNull(Packet packet)
        {
        }

        [Parser(Opcode.SMSG_PET_ADDED)] // 4.3.4
        public static void HandlePetAdded(Packet packet)
        {
            packet.ReadInt32("Pet Level");
            packet.ReadInt32("Pet Slot");
            packet.ReadByte("Stable Type");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Unit, "Entry");
            packet.ReadInt32("Pet Number");

            var len = packet.ReadBits(8);
            packet.ReadWoWString("Pet Name", len);
        }

        [Parser(Opcode.CMSG_PET_RENAME)]
        public static void HandlePetRename(Packet packet)
        {
            packet.ReadGuid("Pet Guid");
            packet.ReadCString("Name");
            var declined = packet.ReadBoolean("Is Declined");
            if (declined)
            {
                packet.StoreBeginList("Declined names");
                for (var i = 0; i < 5; ++i)
                    packet.ReadCString("Declined Name", i);
                packet.StoreEndList();
            }
        }

        [Parser(Opcode.CMSG_PET_SPELL_AUTOCAST)]
        public static void HandlePetSpellAutocast(Packet packet)
        {
            packet.ReadGuid("Pet Guid");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell Id");
            packet.ReadByte("State");
        }
    }
}
