﻿using System;

namespace PacketParser.Enums
{
    [Flags]
    public enum CorpseFlags
    {
        CORPSE_FLAG_NONE = 0x00,
        CORPSE_FLAG_BONES = 0x01,
        CORPSE_FLAG_UNK1 = 0x02,
        CORPSE_FLAG_UNK2 = 0x04,
        CORPSE_FLAG_HIDE_HELM = 0x08,
        CORPSE_FLAG_HIDE_CLOAK = 0x10,
        CORPSE_FLAG_LOOTABLE = 0x20
    }
}
