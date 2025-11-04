using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 커스터마이즈 UI 컨트롤러 (어몽어스 스타일)
    /// </summary>
    public class CustomizeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private Button closeButton;

        [Header("Color Buttons")]
        [SerializeField] private Button[] colorButtons; // 12개의 색상 버튼 배열

        private Image[] _buttonImages;

        private void Awake()
        {
            // 버튼 이미지 캐싱
            if (colorButtons != null && colorButtons.Length > 0)
            {
                _buttonImages = new Image[colorButtons.Length];
                for (int i = 0; i < colorButtons.Length; i++)
                {
                    if (colorButtons[i] != null)
                    {
                        _buttonImages[i] = colorButtons[i].GetComponent<Image>();
                    }
                }
            }
        }

        private void Start()
        {
            InitializeButtons();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void OnEnable()
        {
            // UI가 열릴 때마다 상태 업데이트
            UpdateAllButtonStates();
        }

        /// <summary>
        /// 색상 버튼 초기화
        /// </summary>
        private void InitializeButtons()
        {
            if (CustomizeManager.Instance == null)
            {
                Debug.LogError("[CustomizeUI] CustomizeManager instance not found!");
                return;
            }

            if (colorButtons == null || colorButtons.Length == 0)
            {
                Debug.LogError("[CustomizeUI] Color buttons array is not assigned or empty!");
                return;
            }

            Color[] allColors = CustomizeManager.Instance.GetAllColors();
            int buttonCount = Mathf.Min(colorButtons.Length, allColors.Length);

            for (int i = 0; i < buttonCount; i++)
            {
                if (colorButtons[i] == null) continue;

                // 버튼 색상 설정
                if (_buttonImages[i] != null)
                {
                    _buttonImages[i].color = allColors[i];
                }

                // 클릭 이벤트 등록
                int capturedIndex = i;
                colorButtons[i].onClick.RemoveAllListeners();
                colorButtons[i].onClick.AddListener(() => OnColorButtonClicked(capturedIndex));
            }

            UpdateAllButtonStates();
        }

        private void OnColorButtonClicked(int colorIndex)
        {
            if (CustomizeManager.Instance == null) return;

            // 색상 변경 요청
            CustomizeManager.Instance.RequestColorChange(colorIndex);

            // UI 업데이트 (버튼 상태)
            UpdateAllButtonStates();
        }

        /// <summary>
        /// 모든 버튼 상태 업데이트 (선택 상태 + 사용 가능 여부)
        /// </summary>
        private void UpdateAllButtonStates()
        {
            if (CustomizeManager.Instance == null) return;
            if (colorButtons == null || colorButtons.Length == 0) return;

            int myActorNum = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorNum < 0) return;

            int myColorIndex = CustomizeManager.Instance.GetPlayerColorIndex(myActorNum);
            var availableColors = CustomizeManager.Instance.GetAvailableColorIndices();

            for (int i = 0; i < colorButtons.Length; i++)
            {
                if (colorButtons[i] == null) continue;

                bool isMyColor = (i == myColorIndex);
                bool isAvailable = availableColors.Contains(i) || isMyColor;

                // 버튼 활성화/비활성화 (어몽어스 스타일)
                colorButtons[i].interactable = isAvailable;

                // 선택된 버튼 강조
                var transform = colorButtons[i].transform;
                transform.localScale = isMyColor ? Vector3.one * 1.2f : Vector3.one;

                // 비활성화된 버튼 시각적 표시 (어둡게)
                if (_buttonImages[i] != null && !isAvailable)
                {
                    var tempColor = _buttonImages[i].color;
                    tempColor.a = 0.3f; // 투명도로 비활성화 표시
                    _buttonImages[i].color = tempColor;
                }
                else if (_buttonImages[i] != null && isAvailable)
                {
                    var tempColor = _buttonImages[i].color;
                    tempColor.a = 1f; // 완전 불투명
                    _buttonImages[i].color = tempColor;
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }

            // 모든 버튼 리스너 제거
            if (colorButtons != null)
            {
                foreach (var button in colorButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
