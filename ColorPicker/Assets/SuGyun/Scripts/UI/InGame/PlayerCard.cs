using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 플레이어 카드
    /// </summary>
    public class PlayerCard : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Button cardButton;                 // 카드 전체 버튼
        [SerializeField] private Image playerAvatar;                // 플레이어 아바타
        [SerializeField] private TextMeshProUGUI playerNameText;    // 플레이어 이름
        [SerializeField] private Image colorIndicator;              // 색상 표시 이미지
        [SerializeField] private GameObject selectedFrame;          // 선택 상태 프레임
        
        [Header("설정")]
        [SerializeField] private Color defaultColor = Color.white;  // 기본 색상
        [SerializeField] private Color selectedFrameColor = Color.yellow; // 선택 프레임 색상
        
        // 플레이어 정보
        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        
        // 상태
        private bool isSelected = false;
        private bool hasColor = false;
        private Color assignedColor = Color.white;
        
        // 이벤트
        public System.Action<PlayerCard> OnPlayerCardClicked;
        
        private void Start()
        {
            SetupCard();
        }
        
        /// <summary>
        /// 카드 설정
        /// </summary>
        private void SetupCard()
        {
            // 버튼 이벤트 연결
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => {
                    Debug.Log($"플레이어 카드 버튼 클릭됨: {PlayerName}");
                    OnPlayerCardClicked?.Invoke(this);
                });
            }
            else
            {
                Debug.LogError($"cardButton이 할당되지 않았습니다! {gameObject.name}");
            }
            
            // 초기 상태 설정
            SetSelected(false);
            ResetColor();
        }
        
        /// <summary>
        /// 플레이어 카드 초기화
        /// </summary>
        public void Initialize(string playerId, string playerName, Sprite avatar = null)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            
            // 이름 설정
            if (playerNameText != null)
            {
                playerNameText.text = playerName;
            }
            else
            {
                Debug.LogError("playerNameText가 할당되지 않았습니다!");
            }
            
            // 아바타 설정
            if (playerAvatar != null && avatar != null)
            {
                playerAvatar.sprite = avatar;
            }
            
            Debug.Log($"플레이어 카드 초기화됨: {playerName} (ID: {playerId})");
        }
        
        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            Debug.Log($"플레이어 카드 선택 상태 변경: {PlayerId} → {selected}");
            isSelected = selected;
            
            if (selectedFrame != null)
            {
                selectedFrame.SetActive(selected);
                
                if (selected)
                {
                    Image frameImage = selectedFrame.GetComponent<Image>();
                    if (frameImage != null)
                    {
                        frameImage.color = selectedFrameColor;
                    }
                }
                
                Debug.Log($"플레이어 카드 선택 상태: {PlayerName} → {selected}");
            }
            else
            {
                Debug.LogWarning($"selectedFrame이 할당되지 않았습니다: {PlayerName}");
            }
        }
        
        /// <summary>
        /// 플레이어 색상 설정
        /// </summary>
        public void SetPlayerColor(Color color)
        {
            assignedColor = color;
            hasColor = true;
            
            if (colorIndicator != null)
            {
                colorIndicator.color = color;
                // 색상 표시는 항상 활성화 상태로 유지
                colorIndicator.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("colorIndicator가 할당되지 않았습니다!");
            }
            
            Debug.Log($"플레이어 색상 설정됨: {PlayerName} → {ColorUtility.ToHtmlStringRGB(color)}");
        }
        
        /// <summary>
        /// 색상 리셋
        /// </summary>
        public void ResetColor()
        {
            assignedColor = defaultColor;
            hasColor = false;
            
            if (colorIndicator != null)
            {
                colorIndicator.color = defaultColor;
                // 색상 표시는 항상 활성화 상태로 유지 (기본 색상으로 표시)
                colorIndicator.gameObject.SetActive(true);
            }
            
            Debug.Log($"플레이어 색상 리셋됨: {PlayerName}");
        }
        
        /// <summary>
        /// 색상 할당 여부 확인
        /// </summary>
        public bool HasColor()
        {
            return hasColor;
        }
        
        /// <summary>
        /// 할당된 색상 가져오기
        /// </summary>
        public Color GetAssignedColor()
        {
            return assignedColor;
        }
        
        /// <summary>
        /// 선택 상태 확인
        /// </summary>
        public bool IsSelected()
        {
            return isSelected;
        }
        
        private void OnDestroy()
        {
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
            }
        }
    }
}
