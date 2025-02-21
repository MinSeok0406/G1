using UnityEngine;
using UnityEngine.EventSystems;

public class HelperUtilities
{
    public static Camera mainCamera;

    //Mouse screen position to world position for Object
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 mouseScreenPosition = Input.mousePosition;

        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);

        Vector3 worldPostion = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        worldPostion.z = 0f;

        return worldPostion;
    }

    //Mouse screen position to world position for UI
    public static Vector3 GetMouseWorldPosition(PointerEventData eventData)
    {
        
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 mouseScreenPosition = eventData.position;

        mouseScreenPosition.z = 0f;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        worldPosition.z = 0f;

        return worldPosition;
    }

}
