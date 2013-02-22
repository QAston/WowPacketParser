using System;

namespace PacketParser.Enums
{
    // byte flags value (UNIT_FIELD_BYTES_2, 1)
    [Flags]
    public enum UnitBytes2Flags1 : byte
    {
        UNIT_BYTE2_FLAG1_PVP = 0x01,
        UNIT_BYTE2_FLAG1_UNK1 = 0x02,
        UNIT_BYTE2_FLAG1_FFA_PVP = 0x04,
        UNIT_BYTE2_FLAG1_SANCTUARY = 0x08,
        UNIT_BYTE2_FLAG1_UNK4 = 0x10,
        UNIT_BYTE2_FLAG1_UNK5 = 0x20,
        UNIT_BYTE2_FLAG1_UNK6 = 0x40,
        UNIT_BYTE2_FLAG1_UNK7 = 0x80
    };
}
