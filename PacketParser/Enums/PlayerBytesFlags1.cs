using System;

namespace PacketParser.Enums
{
    // byte flags value (PLAYER_FIELD_BYTES, 1)
    [Flags]
    public enum PlayerBytesFlags1 : byte
    {
        PLAYER_FIELD_BYTE_FLAG1_HAS_GRANTABLE_LEVELS = 0x01,
        PLAYER_FIELD_BYTE_FLAG1_UNK1 = 0x02,
        PLAYER_FIELD_BYTE_FLAG1_UNK2 = 0x04,
        PLAYER_FIELD_BYTE_FLAG1_UNK3 = 0x08,
        PLAYER_FIELD_BYTE_FLAG1_UNK4 = 0x10,
        PLAYER_FIELD_BYTE_FLAG1_UNK5 = 0x20,
        PLAYER_FIELD_BYTE_FLAG1_UNK6 = 0x40,
        PLAYER_FIELD_BYTE_FLAG1_UNK7 = 0x80
    };
}
