using System;

namespace PacketParser.Enums
{
    // byte flags value (PLAYER_FIELD_BYTES2, 3)
    [Flags]
    public enum PlayerBytes2Flags3 : byte
    {
        PLAYER_FIELD_BYTE2_FLAG1_UNK0 = 0x01,
        PLAYER_FIELD_BYTE2_FLAG1_UNK1 = 0x02,
        PLAYER_FIELD_BYTE2_FLAG1_UNK2 = 0x04,
        PLAYER_FIELD_BYTE2_FLAG1_UNK3 = 0x08,
        PLAYER_FIELD_BYTE2_FLAG1_UNK4 = 0x10,
        PLAYER_FIELD_BYTE2_FLAG1_STEALTH = 0x20,
        PLAYER_FIELD_BYTE2_FLAG1_INVISIBILITY_GLOW = 0x40,
        PLAYER_FIELD_BYTE2_FLAG1_UNK7 = 0x80
    };
}
