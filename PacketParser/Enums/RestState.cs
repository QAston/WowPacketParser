using System;

namespace PacketParser.Enums
{
    // PLAYER_BYTES_2 byte 3
    public enum RestState : byte
    {
        REST_STATE_RESTED = 0x01,
        REST_STATE_NOT_RAF_LINKED = 0x02,
        REST_STATE_RAF_LINKED = 0x06
    };
}
