using PacketParser.Enums;
using PacketParser.Misc;

namespace PacketParser.DataStructures
{
    //[DBTableName("points_of_interest")]
    public sealed class GossipPOI : ITextOutputDisabled
    {
        //[DBFieldName("x")]
        public float XPos;

        //[DBFieldName("y")]
        public float YPos;

        //[DBFieldName("icon")]
        public GossipPOIIcon Icon;

        //[DBFieldName("flags")]
        public uint Flags;

        //[DBFieldName("data")]
        public uint Data;

        //[DBFieldName("icon_name")]
        public string IconName;
    }
}
