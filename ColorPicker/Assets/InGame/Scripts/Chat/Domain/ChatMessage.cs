using System;

namespace ColorPicker.Chat
{
    [Serializable]
    public struct ChatMessage
    {
        public int Seq;
        public int PlayerId;
        public string Text;
        public int ServerTimestamp;
        public ChatChannel Channel;

        public ChatMessage(int seq, int pid, string text, int ts, ChatChannel ch)
        {
            Seq = seq; PlayerId = pid; Text = text; ServerTimestamp = ts; Channel = ch;
        }
    }
}
