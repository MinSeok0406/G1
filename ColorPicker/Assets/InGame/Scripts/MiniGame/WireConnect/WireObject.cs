using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 전선 오브젝트
    /// 드래그하여 맞는 색상의 단자에 연결
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(DraggableObject))]
    public class WireObject : MonoBehaviour
    {
        [SerializeField] private Color wireColor;
        [SerializeField] private string targetTerminalTag = "Terminal"; // 단자 태그

        public Action OnWireConnected;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private bool isConnected = false;
        private DragTriggerSensor dragTriggerSensor;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            dragTriggerSensor = GetComponent<DragTriggerSensor>();
            startPosition = rectTransform.localPosition;

            GetComponent<Image>().color = wireColor;
        }

        private void OnEnable()
        {
            if (dragTriggerSensor != null)
            {
                dragTriggerSensor.OnTriggerEntered += DragTriggerSensor_OnTriggerEntered;
            }
        }

        private void OnDisable()
        {
            if (dragTriggerSensor != null)
            {
                dragTriggerSensor.OnTriggerEntered -= DragTriggerSensor_OnTriggerEntered;
            }
        }

        private void DragTriggerSensor_OnTriggerEntered(Collider2D collider)
        {
            if (isConnected) return;

            // 단자인지 확인
            if (collider.CompareTag(targetTerminalTag))
            {
                // 같은 색상인지 확인
                if (collider.TryGetComponent(out Terminal terminal))
                {
                    if (ColorMatch(terminal.GetTerminalColor(), wireColor))
                    {
                        ConnectWire(terminal.transform.position);
                    }
                }
            }
        }

        private bool ColorMatch(Color c1, Color c2)
        {
            return Mathf.Approximately(c1.r, c2.r) &&
                   Mathf.Approximately(c1.g, c2.g) &&
                   Mathf.Approximately(c1.b, c2.b);
        }

        private void ConnectWire(Vector3 terminalPosition)
        {
            isConnected = true;
            rectTransform.position = terminalPosition;
            OnWireConnected?.Invoke();
            Debug.Log("[WireObject] Wire connected!");
        }

        public void ResetWire()
        {
            isConnected = false;
            rectTransform.localPosition = startPosition;
        }
    }

    /// <summary>
    /// 단자 컴포넌트 (전선이 연결되는 목표 지점)
    /// </summary>
    public class Terminal : MonoBehaviour
    {
        [SerializeField] private Color terminalColor;

        public Color GetTerminalColor() => terminalColor;
    }
}
