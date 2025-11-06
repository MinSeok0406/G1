using System.Collections.Generic;

namespace ColorPicker.Chat
{
    public sealed class SimpleRateLimiter : IRateLimiter
    {
        private readonly Dictionary<int,double> lastAt = new(32);

        public bool CanSend(int actorNumber, double now, double minIntervalSec)
        {
            if (actorNumber <= 0) return true;
            if (lastAt.TryGetValue(actorNumber, out var last))
            {
                if (now - last < minIntervalSec) return false;
            }
            lastAt[actorNumber] = now;
            return true;
        }

        public void Reset(int actorNumber)
        {
            if (actorNumber <= 0) return;
            lastAt.Remove(actorNumber);
        }
    }
}
