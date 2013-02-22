using System;

namespace PacketParser.Enums
{
    // byte flags value (UNIT_FIELD_BYTES_2, 2)
    [Flags]
    public enum UnitBytes2Flags2 : byte
    {
        UNIT_BYTE2_FLAG2_CAN_BE_RENAMED = 0x01,
        UNIT_BYTE2_FLAG2_CAN_BE_ABANDONED = 0x02,
        UNIT_BYTE2_FLAG2_UNK2 = 0x04,
        UNIT_BYTE2_FLAG2_UNK3 = 0x08,
        UNIT_BYTE2_FLAG2_UNK4 = 0x10,
        UNIT_BYTE2_FLAG2_UNK5 = 0x20,
        UNIT_BYTE2_FLAG2_UNK6 = 0x40,
        UNIT_BYTE2_FLAG2_UNK7 = 0x80
    };
}
