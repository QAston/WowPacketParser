using System;

namespace PacketParser.Enums
{
    // byte flag value (UNIT_FIELD_BYTES_1, 2)
    [Flags]
    public enum UnitStandFlags : byte
    {
        UNIT_STAND_FLAGS_UNK1 = 0x01,
        UNIT_STAND_FLAGS_CREEP = 0x02,
        UNIT_STAND_FLAGS_UNTRACKABLE = 0x04,
        UNIT_STAND_FLAGS_UNK4 = 0x08,
        UNIT_STAND_FLAGS_UNK5 = 0x10,
        UNIT_STAND_FLAGS_UNK_6 = 0x20,
        UNIT_STAND_FLAGS_UNK_7 = 0x40,
        UNIT_STAND_FLAGS_UNK_8 = 0x80,
    }
}
