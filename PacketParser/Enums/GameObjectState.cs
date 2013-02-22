namespace PacketParser.Enums
{
    public enum GameObjectState : byte
    {
        GO_STATE_ACTIVE = 0,                        // show in world as used and not reset (closed door open)
        GO_STATE_READY = 1,                        // show in world as ready (closed door close)
        GO_STATE_ACTIVE_ALTERNATIVE = 2                         // show in world as used in alt way and not reset (closed door open by cannon fire)
    }
}