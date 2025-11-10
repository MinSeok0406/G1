using UnityEngine;

public static class bl_JoystickUtils
{

    /// <summary>
    /// 스크린 좌표를 Canvas 기준(Overlay/Camera 모두) 좌표로 변환
    /// </summary>
    public static Vector3 ToCanvasPosition(this Canvas canvas, Vector2 screenPos)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return screenPos;

        var rt = canvas.transform as RectTransform;
        var cam = canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out var local);
        return canvas.transform.TransformPoint(local);
    }
}