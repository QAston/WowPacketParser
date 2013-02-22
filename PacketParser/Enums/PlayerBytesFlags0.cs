using System;

namespace PacketParser.Enums
{
    // byte flags value (PLAYER_FIELD_BYTES, 0)
    [Flags]
    public enum PlayerBytesFlags0 : byte
    {
        PLAYER_FIELD_BYTE_FLAG0_UNK0 = 0x01,
        PLAYER_FIELD_BYTE_FLAG0_TRACK_STEALTHED = 0x02,
        PLAYER_FIELD_BYTE_FLAG0_UNK2 = 0x04,
        PLAYER_FIELD_BYTE_FLAG0_RELEASE_TIMER = 0x08,
        PLAYER_FIELD_BYTE_FLAG0_NO_RELEASE_WINDOW = 0x10,
        PLAYER_FIELD_BYTE_FLAG0_HAS_TEMP_PET_BEAR = 0x20,
        PLAYER_FIELD_BYTE_FLAG0_UNK6 = 0x40,
        PLAYER_FIELD_BYTE_FLAG0_UNK7 = 0x80
    };
}
