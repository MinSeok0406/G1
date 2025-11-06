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
        private Color[] _originalColors; // 원본 색상 저장

        private void Awake()
        {
            // 버튼 이미지 및 원본 색상 캐싱
            if (colorButtons != null && colorButtons.Length > 0)
            {
                _buttonImages = new Image[colorButtons.Length];
                _originalColors = new Color[colorButtons.Length];

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

                // 버튼 색상 설정 및 원본 색상 저장
                if (_buttonImages[i] != null)
                {
                    _originalColors[i] = allColors[i];
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

            // 내 색상이 할당되어 있으면 사용 가능한 색상 리스트에 추가
            if (myColorIndex >= 0 && !availableColors.Contains(myColorIndex))
            {
                availableColors.Add(myColorIndex);
            }

            for (int i = 0; i < colorButtons.Length; i++)
            {
                if (colorButtons[i] == null) continue;

                bool isMyColor = (i == myColorIndex);
                bool isAvailable = availableColors.Contains(i);

                // 버튼 활성화/비활성화 (어몽어스 스타일)
                colorButtons[i].interactable = isAvailable;

                // 선택된 버튼 강조
                var transform = colorButtons[i].transform;
                transform.localScale = isMyColor ? Vector3.one * 1.2f : Vector3.one;

                // 버튼 색상 복원 (원본 색상 유지)
                if (_buttonImages[i] != null && _originalColors != null && i < _originalColors.Length)
                {
                    // 원본 색상에서 알파 값만 조정
                    Color displayColor = _originalColors[i];
                    displayColor.a = isAvailable ? 1f : 0.3f; // 사용 불가능한 색상은 투명하게
                    _buttonImages[i].color = displayColor;
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
