using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 운석 오브젝트
    /// 클릭하여 파괴
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ClickableObject))]
    public class AsteroidObject : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 100f;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-300, 300);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(300, 300);
        [SerializeField] private float despawnY = -400f; // 이 Y 좌표 이하로 가면 리스폰

        public Action OnAsteroidDestroyed;

        private RectTransform rectTransform;
        private ClickableObject clickable;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            clickable = GetComponent<ClickableObject>();

            clickable.OnClickEvent += OnClicked;
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                // 아래로 이동
                rectTransform.localPosition += Vector3.down * moveSpeed * Time.deltaTime;

                // 화면 밖으로 나가면 비활성화
                if (rectTransform.localPosition.y < despawnY)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnClicked(Vector2 localPosition)
        {
            DestroyAsteroid();
        }

        private void DestroyAsteroid()
        {
            OnAsteroidDestroyed?.Invoke();
            gameObject.SetActive(false);
            Debug.Log("[AsteroidObject] Asteroid destroyed!");
        }

        /// <summary>
        /// 운석 스폰
        /// </summary>
        public void Spawn()
        {
            // 랜덤 위치에 스폰
            float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            rectTransform.localPosition = new Vector3(randomX, spawnAreaMax.y, 0f);

            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnClicked;
            }
        }
    }
}
