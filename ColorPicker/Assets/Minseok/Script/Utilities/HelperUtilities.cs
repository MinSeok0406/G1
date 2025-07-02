using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Minseok
{
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

        public static float GetAngleFromVector(Vector3 vector)
        {
            float radians = Mathf.Atan2(vector.y, vector.x);

            float degrees = radians * Mathf.Rad2Deg;
            return degrees;
        }

        public static MoveDir GetMoveDirection(float angleDegrees)
        {
            MoveDir moveDirection = MoveDir.None;

            if ((0f < angleDegrees && angleDegrees < 90f) || (-90f < angleDegrees && angleDegrees <= 0))
            {
                moveDirection = MoveDir.Right;
            }
            else if ((90f < angleDegrees && angleDegrees <= 180f) || (-180f < angleDegrees && angleDegrees < -90f))
            {
                moveDirection = MoveDir.Left;
            }
            else if (angleDegrees == 90f)
            {
                moveDirection = MoveDir.Up;
            }
            else if (angleDegrees == -90f)
            {
                moveDirection = MoveDir.Down;
            }


            return moveDirection;
        }

    }
}