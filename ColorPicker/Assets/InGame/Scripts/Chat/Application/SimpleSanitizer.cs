namespace ColorPicker.Chat
{
    public sealed class SimpleSanitizer : IChatSanitizer
    {
        public string Sanitize(string raw, int maxLen)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            var s = raw.Replace("\r", "").Replace("\n", " ").Trim();
            // 위험 태그 최소 제거
            s = s.Replace("<", "").Replace(">", "");

            if (s.Length > maxLen) s = s.Substring(0, maxLen);
            return s;
        }
    }
}