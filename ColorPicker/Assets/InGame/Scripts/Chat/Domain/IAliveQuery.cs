namespace ColorPicker.Chat
{
    public interface IAliveQuery
    {
        bool TryGetAliveState(int actorNumber, out bool hasInGame, out bool isAlive);
    }
}
