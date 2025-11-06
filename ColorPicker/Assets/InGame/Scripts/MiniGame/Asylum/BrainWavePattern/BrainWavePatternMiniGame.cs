using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 뇌파 패턴 맞추기 미니게임
    /// 슬라이더를 조절하여 목표 뇌파 패턴과 일치시키기
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class BrainWavePatternMiniGame : MiniGameBase
    {
        [SerializeField] private List<BrainWaveSlider> sliders = new();
        [SerializeField] private float matchThreshold = 0.1f; // 허용 오차 범위

        private int playerId;
        private MiniGameTag miniGameTag;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            foreach (var slider in sliders)
            {
                slider.OnValueChanged += CheckAllSliders;
            }
        }

        public override void StartGame()
        {
            foreach (var slider in sliders)
            {
                slider.ResetSlider();
            }
        }

        private void CheckAllSliders()
        {
            bool allMatched = true;

            foreach (var slider in sliders)
            {
                if (!slider.IsMatched(matchThreshold))
                {
                    allMatched = false;
                    break;
                }
            }

            if (allMatched)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
        {
            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
        }

        public void ResetMission()
        {
            foreach (var slider in sliders)
            {
                slider.ResetSlider();
            }
        }

        private void OnDestroy()
        {
            foreach (var slider in sliders)
            {
                slider.OnValueChanged -= CheckAllSliders;
            }
        }
    }

    /// <summary>
    /// 뇌파 슬라이더 컴포넌트
    /// </summary>
    public class BrainWaveSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float targetValue; // 목표 값 (0~1)
        [SerializeField] private Image fillImage;
        [SerializeField] private Color normalColor = Color.red;
        [SerializeField] private Color matchedColor = Color.green;

        public System.Action OnValueChanged;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        private void OnSliderValueChanged(float value)
        {
            UpdateVisuals();
            OnValueChanged?.Invoke();
        }

        public bool IsMatched(float threshold)
        {
            return Mathf.Abs(slider.value - targetValue) <= threshold;
        }

        private void UpdateVisuals()
        {
            if (fillImage != null)
            {
                fillImage.color = IsMatched(0.1f) ? matchedColor : normalColor;
            }
        }

        public void ResetSlider()
        {
            if (slider != null)
            {
                slider.value = Random.Range(0f, 1f); // 랜덤 시작 위치
            }
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
}
