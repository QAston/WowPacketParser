using System;
namespace PacketParser.Enums
{
    [Flags]
    public enum ObjectTypeFlags2 : ushort
    {
        TYPE_FLAGS2_IN_GUILD = 0x0001,
        TYPE_FLAGS2_UNK1 = 0x0002,
        TYPE_FLAGS2_UNK2 = 0x0004,
        TYPE_FLAGS2_UNK3 = 0x0008,
        TYPE_FLAGS2_UNK4 = 0x0010,
        TYPE_FLAGS2_UNK5 = 0x0020,
        TYPE_FLAGS2_UNK6 = 0x0040,
        TYPE_FLAGS2_UNK7 = 0x0080,
        TYPE_FLAGS2_UNK8 = 0x0100,
        TYPE_FLAGS2_UNK9 = 0x0200,
        TYPE_FLAGS2_UNK10 = 0x0400,
        TYPE_FLAGS2_UNK11 = 0x0800,
        TYPE_FLAGS2_UNK12 = 0x1000,
        TYPE_FLAGS2_UNK13 = 0x2000,
        TYPE_FLAGS2_UNK14 = 0x4000,
        TYPE_FLAGS2_UNK15 = 0x8000,
    }
}
