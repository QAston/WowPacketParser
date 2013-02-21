using PacketParser.Enums;
using PacketParser.Misc;

namespace PacketParser.DataStructures
{
    public sealed class GameObject : WoWObject
    {
        public GameObject(WoWObject rhs)
            : base(rhs)
        {
        }
        // Fields from UPDATE_FIELDS
        public float? Size;
        public uint? Faction;
        public GameObjectFlag? Flags;
        public uint? Bytes;

        public override void LoadValuesFromUpdateFields()
        {
            Size = GetValue<ObjectField, float?>(ObjectField.OBJECT_FIELD_SCALE_X);
            Faction = GetValue<GameObjectField, uint?>(GameObjectField.GAMEOBJECT_FACTION);
            Flags = GetEnum<GameObjectField, GameObjectFlag?>(GameObjectField.GAMEOBJECT_FLAGS);
            Bytes = GetValue<GameObjectField, uint?>(GameObjectField.GAMEOBJECT_BYTES_1);
        }

        public override bool IsTemporarySpawn()
        {
            if (ForceTemporarySpawn)
                return true;

            // If our gameobject got the following update field set,
            // it's probably a temporary spawn
            UpdateField uf;
            if (UpdateFields.TryGetValue((int)(Enums.Version.UpdateFields.GetUpdateFieldOffset(GameObjectField.GAMEOBJECT_FIELD_CREATED_BY)), out uf))
                return uf.UInt32Value != 0;
            return false;
        }

        public float[] GetRotation()
        {
            return GetArray<GameObjectField, float>(GameObjectField.GAMEOBJECT_PARENTROTATION, 4);
        }

        public bool IsTransport()
        {
            UpdateField uf;
            if (UpdateFields.TryGetValue((int)Enums.Version.UpdateFields.GetUpdateFieldOffset(GameObjectField.GAMEOBJECT_BYTES_1), out uf))
                return (GameObjectType)((uf.UInt32Value & 0x0000FF00) >> 8) == GameObjectType.MOTransport;

            return false;
        }
    }
}
