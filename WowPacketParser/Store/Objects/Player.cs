using PacketParser.Enums;
using PacketParser.DataStructures;

namespace PacketParser.DataStructures
{
    public sealed class Player : Unit
    {
        public Player()
        {
        }
        public Player(WoWObject rhs)
            : base(rhs)
        {
        }

        public string Name;

        public bool FirstLogin;

        // Used when inserting data from SMSG_CHAR_ENUM into the Objects container
        public void UpdatePlayerInfo(Player newInfo)
        {
            Race = newInfo.Race;
            Class = newInfo.Class;
            Name = newInfo.Name;
            FirstLogin = newInfo.FirstLogin;
            Level = newInfo.Level;
        }
    }
}
