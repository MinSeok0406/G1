namespace ColorPicker.Chat
{
    public interface IChatSanitizer
    {
        string Sanitize(string raw, int maxLen);
    }
}
