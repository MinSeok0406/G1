using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorPicker.InGame
{
    public class ColorPickerButtonUI : MonoBehaviour
    {
        private Camera mainCamera;

        private void Awake()
        {
            if(mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject == gameObject) return;
                }

                CloseUIPanel();
            }
        }

        void CloseUIPanel()
        {
            gameObject.SetActive(false);
        }
    }
}
