using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 심박수 디스플레이
    /// 정상/비정상 심박수를 표시하고 클릭하여 처리
    /// 파티클 트레일로 심박수 그래프를 자동으로 그림
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ClickableObject))]
    public class HeartRateDisplay : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0f, 1f, 0f); // 밝은 초록 (Lime Green)
        [SerializeField] private Color abnormalColor = new Color(1f, 0f, 0f); // 밝은 빨강 (Red)
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 어두운 배경

        [Header("UI")]
        [SerializeField] private TMPro.TMP_Text bpmText; // BPM 텍스트
        [SerializeField] private LineRenderer heartbeatLine; // 심박수 그래프 라인

        [Header("Heartbeat Settings")]
        [SerializeField] private float lineHeight = 80f; // 라인 높이 (세로 진폭)
        [SerializeField] private int lineResolution = 200; // 라인 해상도 (더 부드러운 곡선)
        [SerializeField] private float lineThickness = 1.5f; // 라인 두께
        [SerializeField] private float normalBPM = 75f; // 정상 BPM
        [SerializeField] private float abnormalBPM = 180f; // 비정상 BPM
        [SerializeField] private float animationSpeed = 2f; // 애니메이션 속도

        public Action OnAbnormalFixed;

        private Image image;
        private ClickableObject clickable;
        private RectTransform rectTransform;
        private bool isAbnormal = false;
        private float animationTime = 0f;
        private float lineWidth = 0f; // 동적으로 계산됨

        private void Awake()
        {
            image = GetComponent<Image>();
            clickable = GetComponent<ClickableObject>();
            rectTransform = GetComponent<RectTransform>();

            clickable.OnClickEvent += OnClicked;

            // 배경색 설정
            if (image != null)
            {
                image.color = backgroundColor;
            }

            InitializeLineRenderer();
            ResetDisplay();
        }

        private void InitializeLineRenderer()
        {
            if (heartbeatLine != null)
            {
                // RectTransform의 실제 너비를 가져와서 라인 너비로 설정
                if (rectTransform != null)
                {
                    lineWidth = rectTransform.rect.width;
                }

                heartbeatLine.positionCount = lineResolution;
                heartbeatLine.startWidth = lineThickness; // 얇은 선
                heartbeatLine.endWidth = lineThickness;
                heartbeatLine.useWorldSpace = false;
                heartbeatLine.material = new Material(Shader.Find("Sprites/Default"));

                Debug.Log($"[HeartRateDisplay] LineRenderer initialized - Width: {lineWidth}, Thickness: {lineThickness}");
            }
        }

        private void Update()
        {
            if (heartbeatLine != null)
            {
                animationTime += Time.deltaTime * animationSpeed;
                UpdateHeartbeatWave();
            }
        }

        private void OnClicked(Vector2 localPosition)
        {
            if (isAbnormal)
            {
                FixAbnormal();
            }
        }

        /// <summary>
        /// 심박수 파형 업데이트 (ECG 스타일)
        /// </summary>
        private void UpdateHeartbeatWave()
        {
            float bpm = isAbnormal ? abnormalBPM : normalBPM;
            float frequency = bpm / 60f; // BPM을 Hz로 변환

            for (int i = 0; i < lineResolution; i++)
            {
                float t = (float)i / lineResolution;
                float x = (t - 0.5f) * lineWidth;

                // ECG 파형 생성 (시간에 따라 스크롤)
                float phase = (t + animationTime * frequency) % 1f;
                float y = GenerateECGWaveform(phase, isAbnormal) * lineHeight;

                heartbeatLine.SetPosition(i, new Vector3(x, y, 0f));
            }

            // 라인 색상 설정
            Color lineColor = isAbnormal ? abnormalColor : normalColor;
            heartbeatLine.startColor = lineColor;
            heartbeatLine.endColor = lineColor;
        }

        /// <summary>
        /// ECG 파형 생성 (P-QRS-T 파형)
        /// </summary>
        private float GenerateECGWaveform(float phase, bool abnormal)
        {
            float value = 0f;

            if (abnormal)
            {
                // 비정상: 불규칙하고 높은 진폭
                value = Mathf.Sin(phase * Mathf.PI * 2f) * 0.8f;
                value += Mathf.Sin(phase * Mathf.PI * 8f) * 0.3f; // 불규칙한 노이즈
                value += UnityEngine.Random.Range(-0.2f, 0.2f); // 랜덤 떨림
            }
            else
            {
                // 정상: 규칙적인 ECG 파형
                if (phase < 0.15f) // P wave
                {
                    float t = phase / 0.15f;
                    value = Mathf.Sin(t * Mathf.PI) * 0.15f;
                }
                else if (phase < 0.25f) // Q wave
                {
                    float t = (phase - 0.15f) / 0.1f;
                    value = -Mathf.Sin(t * Mathf.PI) * 0.1f;
                }
                else if (phase < 0.35f) // R wave (QRS complex)
                {
                    float t = (phase - 0.25f) / 0.1f;
                    value = Mathf.Sin(t * Mathf.PI) * 0.9f;
                }
                else if (phase < 0.45f) // S wave
                {
                    float t = (phase - 0.35f) / 0.1f;
                    value = -Mathf.Sin(t * Mathf.PI) * 0.15f;
                }
                else if (phase < 0.7f) // T wave
                {
                    float t = (phase - 0.45f) / 0.25f;
                    value = Mathf.Sin(t * Mathf.PI) * 0.25f;
                }
                // else: baseline (0)
            }

            return value;
        }

        /// <summary>
        /// 정상 심박수 표시
        /// </summary>
        public void ShowNormal()
        {
            isAbnormal = false;
            // 배경색은 유지 (backgroundColor)

            if (bpmText != null)
            {
                int bpm = UnityEngine.Random.Range(60, 100);
                bpmText.text = $"{bpm} BPM"; // 정상: 60-100
                bpmText.color = normalColor;
            }

            Debug.Log("[HeartRateDisplay] Normal heartbeat displayed!");
        }

        /// <summary>
        /// 비정상 심박수 표시
        /// </summary>
        public void ShowAbnormal()
        {
            isAbnormal = true;
            // 배경색은 유지 (backgroundColor)

            if (bpmText != null)
            {
                int bpm = UnityEngine.Random.Range(150, 200);
                bpmText.text = $"{bpm} BPM"; // 비정상: 150-200
                bpmText.color = abnormalColor;
            }

            Debug.Log("[HeartRateDisplay] Abnormal heartbeat detected!");
        }

        /// <summary>
        /// 비정상 심박수 처리
        /// </summary>
        private void FixAbnormal()
        {
            ShowNormal();
            OnAbnormalFixed?.Invoke();
            Debug.Log("[HeartRateDisplay] Abnormal heartbeat fixed!");
        }

        /// <summary>
        /// 디스플레이 리셋
        /// </summary>
        public void ResetDisplay()
        {
            ShowNormal();
            animationTime = 0f;
        }

        public bool IsAbnormal() => isAbnormal;

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnClicked;
            }
        }
    }
}
