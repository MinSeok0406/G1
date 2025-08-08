using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class MiniGameInputHandler
    {
        private static IDraggable currentDraggable = null;

        public static void ProcessInput()
        {
            if (!MiniGameInputContext.IsInputEnabled) return;

            Vector2 pointerPos = GetPosition();

            // Ŭ��/�巡�� ����
            if (IsDown())
            {
                GameObject target = RaycastUI(pointerPos);
                if (target == null) return;

                if (target.TryGetComponent<IDraggable>(out var draggable))
                {
                    currentDraggable = draggable;
                    draggable.OnBeginDrag();
                }

                else if (target.TryGetComponent<IClickable>(out var clickable))
                {
                    clickable.OnClick();
                }
            }

            if (IsHold() && currentDraggable != null)
            {
                currentDraggable.OnDrag(pointerPos);
            }

            if (IsUp() && currentDraggable != null)
            {
                currentDraggable.OnEndDrag();
                currentDraggable = null;
            }
        }


        private static GameObject RaycastUI(Vector2 screenPos)
        {
            PointerEventData pointerData = new(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        public static bool IsDown()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Input.GetMouseButtonDown(0);
#elif UNITY_IOS || UNITY_ANDROID
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
            return false;
#endif
        }

        public static bool IsHold()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Input.GetMouseButton(0);
#elif UNITY_IOS || UNITY_ANDROID
    return Input.touchCount > 0 &&
           (Input.GetTouch(0).phase == TouchPhase.Moved ||
            Input.GetTouch(0).phase == TouchPhase.Stationary);
#else
    return false;
#endif
        }

        public static bool IsUp()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Input.GetMouseButtonUp(0);
#elif UNITY_IOS || UNITY_ANDROID
    return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#else
    return false;
#endif
        }

        public static Vector2 GetPosition()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Input.mousePosition;
#elif UNITY_IOS || UNITY_ANDROID
            return Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#else
            return Vector2.zero;
#endif
        }
    }
}