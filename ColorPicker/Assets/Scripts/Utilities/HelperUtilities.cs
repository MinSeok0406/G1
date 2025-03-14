using ColorPicker.InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  static class HelperUtilities 
{
    // for customiz player color 
    public static Color GetUnityColor(CustomizationColor color)
    {
        switch (color)
        {
            case CustomizationColor.Red: return Color.red;
            case CustomizationColor.Green: return Color.green;
            case CustomizationColor.Blue: return Color.blue;
            case CustomizationColor.Yellow: return Color.yellow;
            case CustomizationColor.Cyan: return Color.cyan;
            case CustomizationColor.Magenta: return Color.magenta;
            case CustomizationColor.Orange: return new Color(1.0f, 0.647f, 0.0f);
            case CustomizationColor.Purple: return new Color(0.5f, 0.0f, 0.5f);
            case CustomizationColor.Pink: return new Color(1.0f, 192 / 255f, 203 / 255f); 
            case CustomizationColor.Brown: return new Color(0.65f, 0.16f, 0.16f);
            case CustomizationColor.White: return Color.white;

            default: return Color.white;
        }
    }
}
