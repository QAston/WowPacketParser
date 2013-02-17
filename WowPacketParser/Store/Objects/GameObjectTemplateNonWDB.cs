using PacketParser.Enums;
using PacketParser.SQL;

namespace PacketDumper.DataStructures
{
    [DBTableName("gameobject_template")]
    public class GameObjectTemplateNonWDB
    {
        [DBFieldName("size")] public float Size;
        [DBFieldName("faction")] public uint Faction;
        [DBFieldName("flags")] public GameObjectFlag Flags;
    }
}
