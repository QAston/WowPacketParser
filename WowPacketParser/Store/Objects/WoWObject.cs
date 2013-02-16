using System.Collections.Generic;
using PacketParser.Enums;
using PacketParser.Misc;
using PacketParser.Enums.Version;

namespace PacketParser.DataStructures
{
    public class WoWObject
    {
        public ObjectType Type;

        public MovementInfo Movement;

        public uint Map;

        public int Area;

        public Dictionary<int, UpdateField> UpdateFields; // SMSG_UPDATE_OBJECT - CreateObject

        public ICollection<Dictionary<int, UpdateField>> ChangedUpdateFieldsList; // SMSG_UPDATE_OBJECT - Values

        public uint PhaseMask;

        public bool ForceTemporarySpawn;

        public virtual bool IsTemporarySpawn()
        {
            return ForceTemporarySpawn;
        }

        public bool IsOnTransport()
        {
            return Movement.TransportGuid != Guid.Empty;
        }

        public int GetDefaultSpawnTime()
        {
            // If map is Continent use a lower respawn time
            // TODO: Rank and if npc is needed for quest kill should change spawntime as well
            switch (Map)
            {
                case 0:     // Eastern Kingdoms
                case 1:     // Kalimdor
                case 530:   // Outland
                case 571:   // Northrend
                case 609:   // Ebon Hold
                case 638:   // Gilneas 1
                case 655:   // Gilneas 2
                case 656:   // Gilneas 3
                case 646:   // Deepholm
                case 648:   // Kezan 1
                case 659:   // Kezan 2
                case 661:   // Kezan 3
                case 732:   // Tol Barad
                case 861:   // Firelands Dailies
                    return 120;
                default:
                    return 7200;
            }
        }
        public Guid? GetGuid()
        {
            UpdateField low;
            UpdateField high;
            if (UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(ObjectField.OBJECT_FIELD_GUID), out low))
                if (UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(ObjectField.OBJECT_FIELD_GUID + 1), out high))
                {
                    ulong lowg = low.UInt32Value;
                    ulong highg = high.UInt32Value;
                    return new Guid(lowg | (highg<<32));
                }
            return null;
        }

        public virtual void LoadValuesFromUpdateFields() { }
    }
}
