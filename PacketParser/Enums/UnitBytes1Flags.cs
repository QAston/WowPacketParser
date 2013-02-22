using System;

namespace PacketParser.Enums
{
    // byte flags value (UNIT_FIELD_BYTES_1, 3)
    [Flags]
    public enum UnitBytes1Flags : byte
    {
        UNIT_BYTE1_FLAG_ALWAYS_STAND = 0x01,
        UNIT_BYTE1_FLAG_HOVER = 0x02,
        UNIT_BYTE1_FLAG_UNK_3 = 0x04,
        UNIT_BYTE1_FLAG_UNK_4 = 0x08,
        UNIT_BYTE1_FLAG_UNK_5 = 0x10,
        UNIT_BYTE1_FLAG_UNK_6 = 0x20,
        UNIT_BYTE1_FLAG_UNK_7 = 0x40,
        UNIT_BYTE1_FLAG_UNK_8 = 0x80,
    };
}
