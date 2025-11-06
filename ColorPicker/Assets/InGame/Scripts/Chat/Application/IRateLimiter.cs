namespace ColorPicker.Chat
{
    public interface IRateLimiter
    {
        bool CanSend(int actorNumber, double now, double minIntervalSec);
        void Reset(int actorNumber);
    }
}
