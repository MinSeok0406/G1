    using System.Collections.Generic;

namespace ColorPicker.Chat
{
    public sealed class RingChatHistory : IChatHistory
    {
        private readonly List<ChatMessage> _list = new(256);

        public int Count => _list.Count;

        public void Clear(int capacityHint)
        {
            _list.Clear();
            if (_list.Capacity < capacityHint) _list.Capacity = capacityHint;
        }

        public void Add(ChatMessage msg, int maxMessages)
        {
            _list.Add(msg);
            if (_list.Count > maxMessages)
            {
                int remove = _list.Count - maxMessages;
                _list.RemoveRange(0, remove);
            }
        }

        public void GetTailFiltered(
            int takeLastN, bool includeGhost, List<ChatMessage> outBuffer)
        {
            outBuffer.Clear();
            int start = _list.Count > takeLastN ? _list.Count - takeLastN : 0;
            for (int i = start; i < _list.Count; i++)
            {
                var m = _list[i];
                if (m.Channel == ChatChannel.Global || (includeGhost && m.Channel == ChatChannel.Ghost))
                    outBuffer.Add(m);
            }
        }
    }
}
