using System.Collections.Generic;
using PacketParser.Enums;
using PacketParser.Misc;
using PacketParser.Enums.Version;
using System;
using System.Globalization;
using System.Reflection;

namespace PacketParser.DataStructures
{
    public class WoWObject
    {
        public WoWObject()
        {
        }
        public WoWObject(WoWObject rhs)
        {
            if (rhs == null)
                return;
            // get all the fields in the class
            FieldInfo[] fields_of_class = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // copy each value over to 'this'
            foreach (FieldInfo fi in fields_of_class)
            {
                fi.SetValue(this, fi.GetValue(rhs));
            }
        }
        public ObjectType Type;

        public bool Created = false; // if SMSG_UPDATE_OBJECT - CreateObject was received

        public ObjectState State = ObjectState.Default;

        public MovementInfo SpawnMovement;

        public MovementInfo Movement;

        public uint Map = 0;

        public int Area = 0;

        public UpdateFieldDictionary UpdateFields = new UpdateFieldDictionary();

        public UpdateFieldDictionary SpawnUpdateFields; 

        public uint PhaseMask = 0;

        public bool ForceTemporarySpawn = false;

        public virtual bool IsTemporarySpawn()
        {
            return ForceTemporarySpawn;
        }

        public bool IsOnTransport()
        {
            return SpawnMovement.TransportGuid != Guid.Empty;
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
            return GetValue<ObjectField, Guid?>(ObjectField.OBJECT_FIELD_GUID);
        }

        public virtual void LoadValuesFromUpdateFields() { }

        /// <summary>
        /// Grabs a value from a dictionary of UpdateFields
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="updateField">The update field we want</param>
        /// <returns></returns>
        public TK GetValue<T, TK>(T updateField) where T : struct,IConvertible
        {
            return UpdateFields.GetValue<TK>((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(updateField));
        }

        /// <summary>
        /// Grabs N (consecutive) values from a dictionary of UpdateFields
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="firstUpdateField">The first update field of the sequence</param>
        /// <param name="count">Number of values to retrieve</param>
        /// <returns></returns>
        public TK[] GetArray<T, TK>(T firstUpdateField, int count) where T : struct,IConvertible
        {
            return UpdateFields.GetArray<TK>((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(firstUpdateField), count);
        }

        /// <summary>
        /// Grabs a value from a dictionary of UpdateFields and converts it to an enum val
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (a NULLABLE enum)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="updateField">The update field we want</param>
        /// <returns></returns>
        public TK GetEnum<T, TK>(T updateField) where T : struct,IConvertible
        {
            return UpdateFields.GetEnum<TK>((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(updateField));
        }
    }
}
