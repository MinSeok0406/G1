using System.Collections.Generic;

namespace ColorPicker.Chat
{
    public interface IChatHistory
    {
        void Clear(int capacityHint);
        void Add(ChatMessage msg, int maxMessages);
        void GetTailFiltered(
            int takeLastN, bool includeGhost, List<ChatMessage> outBuffer);
        int Count { get; }
    }
}
