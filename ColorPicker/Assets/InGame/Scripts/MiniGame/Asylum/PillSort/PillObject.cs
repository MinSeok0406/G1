using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 약물 오브젝트
    /// 색상별로 분류하여 보관함에 넣어야 함
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(DraggableObject))]
    public class PillObject : MonoBehaviour
    {
        [SerializeField] private PillType pillType = PillType.Red;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-250, -200);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(250, 200);

        public Action OnPillSorted;

        private RectTransform rectTransform;
        private Image image;
        private bool isSorted = false;
        private DragTriggerSensor dragTriggerSensor;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            dragTriggerSensor = GetComponent<DragTriggerSensor>();

            // 타입에 따라 색상 설정
            SetColorByType();
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
            if (isSorted) return;

            // PillContainer 확인
            if (collider.TryGetComponent(out PillContainer container))
            {
                if (container.GetContainerType() == pillType)
                {
                    SortPill();
                }
            }
        }

        private void SortPill()
        {
            isSorted = true;
            OnPillSorted?.Invoke();
            gameObject.SetActive(false);
            Debug.Log($"[PillObject] {pillType} pill sorted!");
        }

        public void ResetToRandomPosition()
        {
            isSorted = false;
            float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            rectTransform.localPosition = new Vector2(randomX, randomY);
        }

        private void SetColorByType()
        {
            switch (pillType)
            {
                case PillType.Red:
                    image.color = new Color(1f, 0.2f, 0.2f); // 빨강 (진정제)
                    break;
                case PillType.Blue:
                    image.color = new Color(0.2f, 0.5f, 1f); // 파랑 (수면제)
                    break;
                case PillType.Green:
                    image.color = new Color(0.3f, 1f, 0.3f); // 초록 (항우울제)
                    break;
                case PillType.Yellow:
                    image.color = new Color(1f, 0.9f, 0.2f); // 노랑 (항불안제)
                    break;
            }
        }

        public PillType GetPillType() => pillType;
    }



}

    /// <summary>
    /// 약물 타입
    /// </summary>
    public enum PillType
    {
        Red,    // 진정제 (Sedative)
        Blue,   // 수면제 (Sleeping pill)
        Green,  // 항우울제 (Antidepressant)
        Yellow  // 항불안제 (Anti-anxiety)
    }