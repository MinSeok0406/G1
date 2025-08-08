
namespace ColorPicker.InGame
{
    public static class MiniGameInputContext
    {
        public static bool IsInputEnabled { get; private set; } = false;

        public static void EnableInput() => IsInputEnabled = true;
        public static void DisableInput() => IsInputEnabled = false;
    }
}
