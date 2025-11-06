using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 장난감 오브젝트
    /// 드래그하여 상자에 넣을 수 있음
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(DraggableObject))]
    public class ToyObject : MonoBehaviour
    {
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-200, -200);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(200, 200);

        public Action OnSorted;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private bool isSorted = false;

        private DragTriggerSensor dragTriggerSensor;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            dragTriggerSensor = GetComponent<DragTriggerSensor>();
            startPosition = rectTransform.localPosition;
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
            // 장난감 상자 태그 확인
            if (collider.CompareTag("ToyBox") && !isSorted)
            {
                SortToy();
            }
        }

        private void SortToy()
        {
            isSorted = true;
            OnSorted?.Invoke();
            gameObject.SetActive(false);
            Debug.Log("[ToyObject] Toy sorted!");
        }

        /// <summary>
        /// 장난감을 랜덤 위치에 배치
        /// </summary>
        public void ResetToRandomPosition()
        {
            isSorted = false;
            float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            rectTransform.localPosition = new Vector2(randomX, randomY);
        }
    }
}
