using UnityEngine;

namespace ColorPicker.InGame
{
    public static class HelperUtilities
    {
        public static Color GetUnityColor(ColorType playerColor)
        {
            switch (playerColor)
            {
                case ColorType.Red: return Color.red;
                case ColorType.Blue: return Color.blue;
                case ColorType.Green: return Color.green;
                case ColorType.Yellow: return Color.yellow;
                case ColorType.White: return Color.white;

                default: return Color.white;
            }
        }
    }
}
