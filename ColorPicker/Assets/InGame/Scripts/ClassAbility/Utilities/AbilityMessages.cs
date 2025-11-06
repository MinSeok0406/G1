namespace ColorPicker.InGame
{
    public enum ColorPickResult { Error = -1, Fail = 0, Success = 1 }

    internal static class AbilityMessages
    {
        public const string ColorPickError = "지정할 수 없는 대상입니다.";
        public const string ColorPickFail  = "실패했습니다.";
        public const string ColorPickOk    = "성공!";
    }
}
