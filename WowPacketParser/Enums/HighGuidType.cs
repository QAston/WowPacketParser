namespace PacketParser.Enums
{
    public enum HighGuidType
    {
        None            = -1,
        Player          = 0x000, // in fact any with 0xx is a player
        BattleGround1   = 0x101,
        InstanceSave    = 0x104,
        Group           = 0x105,
        BattleGround2   = 0x109,
        MOTransport     = 0x10C,
        Guild           = 0x10F,
        Item            = 0x400,   // in fact any with 4xx is an item
        Corpse          = 0xF00,
        GameObject      = 0xF01,
        Transport       = 0xF02,
        Unit            = 0xF03,
        Pet             = 0xF04,
        Vehicle         = 0xF05,
    }
}
