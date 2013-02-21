using PacketParser.Enums;

namespace PacketParser.DataStructures
{
    public sealed class Player : WoWObject
    {
        public Player()
        {
        }
        public Player(WoWObject rhs)
            : base(rhs)
        {
        }
        public Race Race;

        public Class Class;

        public string Name;

        public bool FirstLogin;

        public int Level;

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
