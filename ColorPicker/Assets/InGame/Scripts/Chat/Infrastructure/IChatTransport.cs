using System.Collections.Generic;

namespace ColorPicker.Chat
{
    public interface IChatTransport
    {
        bool IsServer { get; }
        double NowSeconds { get; } // Time.realtimeSinceStartupAsDouble 래핑
        bool ValidateSenderContext(object ctx, int declaredActorNumber);
        void BroadcastToAll(ChatMessage msg, bool isGhost);
        void BroadcastToGhostOnly(ChatMessage msg);
        void SendBulkTo(int targetActorNumber, List<ChatMessage> batch);
    }
}
