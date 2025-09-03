using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class MiniGameInputHandler
    {
        public static RectTransform rawImageRect;
        public static RectTransform targetRect;
        public static Camera rtCamera;
        public static float zDepth => (-rtCamera.transform.position.z);
        private static IDraggable currentDraggable = null;

        public static void ProcessInput()
        {
            if (!MiniGameInputContext.IsInputEnabled || rawImageRect == null)
                return;

            Vector2 pointerPos = GetPosition();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, pointerPos, null)) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, pointerPos, null, out Vector2 localPos);

            var r = rawImageRect.rect;
            float normX = Mathf.InverseLerp(r.xMin, r.xMax, localPos.x);
            float normY = Mathf.InverseLerp(r.yMin, r.yMax, localPos.y);

            float planeZ = 0f;
            Ray ray = rtCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
            float t = (planeZ - ray.origin.z) / ray.direction.z;
            Vector3 worldPos = ray.origin + ray.direction * t;

            Vector2 targetPos = targetRect.InverseTransformPoint(worldPos);

            if (IsDown())
            {
                Collider2D hit = Physics2D.OverlapPoint(worldPos,layerMask: LayerMask.GetMask("CRT"));
                if (hit != null)
                {
                    if (hit.TryGetComponent<IDraggable>(out var drag))
                    {
                        currentDraggable = drag;
                        drag.OnBeginDrag(targetPos);
                    }
                    else if (hit.TryGetComponent<IClickable>(out var click))
                    {
                        click.OnClick();
                    }
                }
            }

            if (IsHold() && currentDraggable != null)
            {
                currentDraggable.OnDrag(targetPos);
            }

            if (IsUp() && currentDraggable != null)
            {
                currentDraggable.OnEndDrag();
                currentDraggable = null;
            }
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