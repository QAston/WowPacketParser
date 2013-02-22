using System;
namespace PacketParser.Enums
{
    [Flags]
    public enum ObjectTypeFlags1 : ushort
    {
        TYPEMASK_OBJECT = 0x0001,
        TYPEMASK_ITEM = 0x0002,
        TYPEMASK_CONTAINER = 0x0004,
        TYPEMASK_UNIT = 0x0008,   
        TYPEMASK_PLAYER = 0x0010,
        TYPEMASK_GAMEOBJECT = 0x0020,
        TYPEMASK_DYNAMICOBJECT = 0x0040,
        TYPEMASK_CORPSE = 0x0080,
        TYPEMASK_UNK8 = 0x0100,
        TYPEMASK_UNK9 = 0x0200,
        TYPEMASK_UNK10 = 0x0400,
        TYPEMASK_UNK11 = 0x0800,
        TYPEMASK_UNK12 = 0x1000,
        TYPEMASK_UNK13 = 0x2000,
        TYPEMASK_UNK14 = 0x4000,
        TYPEMASK_UNK15 = 0x8000,
    }
}
