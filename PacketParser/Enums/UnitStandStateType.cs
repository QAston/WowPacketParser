﻿using System;

namespace PacketParser.Enums
{
    // byte value (UNIT_FIELD_BYTES_1, 0)
    public enum UnitStandStateType : byte
    {
        UNIT_STAND_STATE_STAND             = 0,
        UNIT_STAND_STATE_SIT               = 1,
        UNIT_STAND_STATE_SIT_CHAIR         = 2,
        UNIT_STAND_STATE_SLEEP             = 3,
        UNIT_STAND_STATE_SIT_LOW_CHAIR     = 4,
        UNIT_STAND_STATE_SIT_MEDIUM_CHAIR  = 5,
        UNIT_STAND_STATE_SIT_HIGH_CHAIR    = 6,
        UNIT_STAND_STATE_DEAD              = 7,
        UNIT_STAND_STATE_KNEEL             = 8,
        UNIT_STAND_STATE_SUBMERGED         = 9
    }
}
