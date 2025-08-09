using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 색상 추리 시스템
    /// </summary>
    public class ColorDeductionSystem : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private CircularHuePicker huePicker;           // 색상 선택기 (기존 버전)
        [SerializeField] private Image currentColorDisplay;             // 현재 선택된 색상 표시
        [SerializeField] private TextMeshProUGUI selectPlayerText;      // "플레이어를 선택해주세요" 텍스트
        [SerializeField] private PolygonCollider2D resetCollider;       // 리셋 콜라이더
        [SerializeField] private PolygonCollider2D backCollider;        // 뒤로가기 콜라이더
        
        [Header("플레이어 카드 시스템")]
        [SerializeField] private Transform playerCardsContainer;        // 플레이어 카드들이 배치될 컨테이너
        [SerializeField] private GameObject playerCardPrefab;           // 플레이어 카드 프리팹
        
        [Header("UI 매니저")]
        [SerializeField] private InGameUIManager uiManager;             // UI 매니저 참조
        
        [Header("테스트 플레이어 데이터")]
        [SerializeField] private List<PlayerInfo> testPlayers = new List<PlayerInfo>();
        
        [System.Serializable]
        public class PlayerInfo
        {
            public string playerId;
            public string playerName;
            public Sprite playerAvatar;
        }
        
        // 현재 상태
        private PlayerCard selectedPlayer = null;
        private List<PlayerCard> playerCards = new List<PlayerCard>();
        private Dictionary<string, Color> playerColors = new Dictionary<string, Color>();
        
        private void Start()
        {
            InitializeSystem();
        }
        
        private void Update()
        {
            HandleColliderInput();
        }
        
        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void InitializeSystem()
        {
            Debug.Log("=== 추리 시스템 초기화 시작 ===");
            
            // 버튼 이벤트 설정
            SetupButtons();
            
            // Hue 피커 이벤트 설정
            SetupHuePicker();
            
            // 테스트 플레이어가 없으면 기본 플레이어 추가
            if (testPlayers.Count == 0)
            {
                AddDefaultTestPlayers();
            }
            
            // 플레이어 카드 생성
            CreatePlayerCards();
            
            // 초기 UI 상태 설정
            SetInitialUIState();
            
            Debug.Log("=== 추리 시스템 초기화 완료 ===");
        }
        
        /// <summary>
        /// 콜라이더 입력 처리
        /// </summary>
        private void HandleColliderInput()
        {
            // 클릭/터치 감지
            bool isClicked = Input.GetMouseButtonDown(0);
            
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                isClicked = true;
            }
            
            if (!isClicked) return;
            
            // 입력 위치 가져오기
            Vector2 inputPosition;
            if (Input.touchCount > 0)
            {
                inputPosition = Input.GetTouch(0).position;
            }
            else
            {
                inputPosition = Input.mousePosition;
            }
            
            // 스크린 좌표를 월드 좌표로 변환
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(inputPosition);
            
            // 리셋 콜라이더 확인
            if (resetCollider != null && resetCollider.OverlapPoint(worldPosition))
            {
                Debug.Log("리셋 콜라이더 클릭됨");
                ResetAllDeductions();
                return;
            }
            
            // 뒤로가기 콜라이더 확인
            if (backCollider != null && backCollider.OverlapPoint(worldPosition))
            {
                Debug.Log("뒤로가기 콜라이더 클릭됨");
                OnBackButtonClicked();
                return;
            }
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            // PolygonCollider2D 기반 버튼들
            if (resetCollider != null)
            {
                Debug.Log("리셋 콜라이더 설정됨");
            }
            else
            {
                Debug.LogWarning("리셋 콜라이더가 할당되지 않았습니다!");
            }
            
            if (backCollider != null)
            {
                Debug.Log("뒤로가기 콜라이더 설정됨");
            }
            else
            {
                Debug.LogWarning("뒤로가기 콜라이더가 할당되지 않았습니다!");
            }
        }
        
        /// <summary>
        /// Hue 피커 이벤트 설정
        /// </summary>
        private void SetupHuePicker()
        {
            if (huePicker != null)
            {
                huePicker.OnColorChanged += OnColorSelected;
                Debug.Log("Hue 피커 이벤트 설정됨");
            }
            else
            {
                Debug.LogWarning("Hue 피커가 할당되지 않았습니다!");
            }
        }
        
        /// <summary>
        /// 기본 테스트 플레이어 추가
        /// </summary>
        private void AddDefaultTestPlayers()
        {
            Debug.Log("기본 테스트 플레이어 추가 중...");
            
            testPlayers.Add(new PlayerInfo { playerId = "player1", playerName = "플레이어 1", playerAvatar = null });
            testPlayers.Add(new PlayerInfo { playerId = "player2", playerName = "플레이어 2", playerAvatar = null });
            testPlayers.Add(new PlayerInfo { playerId = "player3", playerName = "플레이어 3", playerAvatar = null });
            testPlayers.Add(new PlayerInfo { playerId = "player4", playerName = "플레이어 4", playerAvatar = null });
            Debug.Log($"기본 테스트 플레이어 {testPlayers.Count}명 추가됨");
        }
        
        /// <summary>
        /// 플레이어 카드 생성
        /// </summary>
        private void CreatePlayerCards()
        {
            Debug.Log($"플레이어 카드 생성 시작 - {testPlayers.Count}명");
            
            // 기존 카드 정리
            ClearPlayerCards();
            
            // 새로운 카드 생성
            foreach (var playerInfo in testPlayers)
            {
                CreatePlayerCard(playerInfo);
            }
            
            Debug.Log($"플레이어 카드 {playerCards.Count}개 생성 완료");
            
            // 저장된 색상 로드
            LoadPlayerColors();
        }
        
        /// <summary>
        /// 개별 플레이어 카드 생성
        /// </summary>
        private void CreatePlayerCard(PlayerInfo playerInfo)
        {
            if (playerCardPrefab == null || playerCardsContainer == null)
            {
                Debug.LogError("플레이어 카드 프리팹 또는 컨테이너가 할당되지 않았습니다!");
                return;
            }
            
            GameObject cardObject = Instantiate(playerCardPrefab, playerCardsContainer);
            PlayerCard playerCard = cardObject.GetComponent<PlayerCard>();
            
            if (playerCard != null)
            {
                playerCard.Initialize(playerInfo.playerId, playerInfo.playerName, playerInfo.playerAvatar);
                playerCard.OnPlayerCardClicked += OnPlayerCardClicked;
                playerCards.Add(playerCard);
                
                Debug.Log($"플레이어 카드 생성됨: {playerInfo.playerName}");
            }
            else
            {
                Debug.LogError($"PlayerCard 컴포넌트를 찾을 수 없습니다: {playerInfo.playerName}");
                Destroy(cardObject);
            }
        }
        
        /// <summary>
        /// 기존 플레이어 카드들 정리
        /// </summary>
        private void ClearPlayerCards()
        {
            foreach (var card in playerCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            playerCards.Clear();
        }
        
        /// <summary>
        /// 초기 UI 상태 설정
        /// </summary>
        private void SetInitialUIState()
        {
            // 색상 표시 비활성화
            SetColorDisplayActive(false);
            
            // 플레이어 선택 텍스트 활성화
            SetSelectPlayerTextActive(true);
            
            Debug.Log("초기 UI 상태 설정 완료");
        }
        
        /// <summary>
        /// 플레이어 카드 클릭 이벤트
        /// </summary>
        private void OnPlayerCardClicked(PlayerCard clickedCard)
        {
            Debug.Log($"플레이어 카드 클릭됨: {clickedCard.PlayerName}");
            
            // 이전 선택 해제
            if (selectedPlayer != null)
            {
                selectedPlayer.SetSelected(false);
            }
            
            // 새 플레이어 선택
            selectedPlayer = clickedCard;
            selectedPlayer.SetSelected(true);
            
            // UI 상태 변경 - 색상 표시 활성화, 텍스트 숨김
            SetColorDisplayActive(true);
            SetSelectPlayerTextActive(false);
            
            // 이미 선택된 색상이 있다면 표시
            if (playerColors.ContainsKey(clickedCard.PlayerId))
            {
                Color playerColor = playerColors[clickedCard.PlayerId];
                SetCurrentColorDisplay(playerColor);
                
                // Hue 피커에도 현재 색상 설정
                if (huePicker != null)
                {
                    huePicker.SetCurrentColor(playerColor);
                }
                
                Debug.Log($"기존 색상 표시: {clickedCard.PlayerName} → {ColorUtility.ToHtmlStringRGB(playerColor)}");
            }
            else
            {
                SetCurrentColorDisplay(Color.white);
            }
            
            Debug.Log($"플레이어 선택됨: {clickedCard.PlayerName}");
        }
        
        /// <summary>
        /// 색상 선택 이벤트 (Hue 피커에서)
        /// </summary>
        private void OnColorSelected(Color selectedColor)
        {
            if (selectedPlayer == null)
            {
                Debug.LogWarning("선택된 플레이어가 없습니다!");
                return;
            }
            
            Debug.Log($"색상 선택됨: {ColorUtility.ToHtmlStringRGB(selectedColor)} for {selectedPlayer.PlayerName}");
            
            // 플레이어에게 색상 할당
            playerColors[selectedPlayer.PlayerId] = selectedColor;
            selectedPlayer.SetPlayerColor(selectedColor);
            
            // 현재 색상 표시 업데이트
            SetCurrentColorDisplay(selectedColor);
            
            // 색상 저장
            SavePlayerColor(selectedPlayer.PlayerId, selectedColor);
            
            // 색상 선택 완료 후 - 색상 피커는 비활성화하지만 플레이어 색상은 유지
            // SetColorDisplayActive(false); // 이 줄을 제거하여 색상이 계속 보이도록 함
            SetSelectPlayerTextActive(true);
            
            Debug.Log($"색상 할당 완료 - 플레이어 색상 표시 유지");
        }
        
        /// <summary>
        /// 플레이어 색상 저장
        /// </summary>
        private void SavePlayerColor(string playerId, Color color)
        {
            string colorKey = $"PlayerColor_{playerId}";
            string colorString = $"{color.r},{color.g},{color.b},{color.a}";
            PlayerPrefs.SetString(colorKey, colorString);
            PlayerPrefs.Save();
            Debug.Log($"플레이어 색상 저장됨: {playerId} → {ColorUtility.ToHtmlStringRGB(color)}");
        }
        
        /// <summary>
        /// 모든 플레이어 색상 로드
        /// </summary>
        private void LoadPlayerColors()
        {
            foreach (var playerInfo in testPlayers)
            {
                string colorKey = $"PlayerColor_{playerInfo.playerId}";
                if (PlayerPrefs.HasKey(colorKey))
                {
                    string colorString = PlayerPrefs.GetString(colorKey);
                    if (TryParseColor(colorString, out Color savedColor))
                    {
                        playerColors[playerInfo.playerId] = savedColor;
                        
                        // 해당 플레이어 카드에 색상 적용
                        PlayerCard playerCard = playerCards.Find(card => card.PlayerId == playerInfo.playerId);
                        if (playerCard != null)
                        {
                            playerCard.SetPlayerColor(savedColor);
                            Debug.Log($"저장된 색상 복원: {playerInfo.playerName} → {ColorUtility.ToHtmlStringRGB(savedColor)}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 색상 문자열 파싱
        /// </summary>
        private bool TryParseColor(string colorString, out Color color)
        {
            color = Color.white;
            string[] values = colorString.Split(',');
            
            if (values.Length == 4 &&
                float.TryParse(values[0], out float r) &&
                float.TryParse(values[1], out float g) &&
                float.TryParse(values[2], out float b) &&
                float.TryParse(values[3], out float a))
            {
                color = new Color(r, g, b, a);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 현재 색상 표시 설정
        /// </summary>
        private void SetCurrentColorDisplay(Color color)
        {
            if (currentColorDisplay != null)
            {
                currentColorDisplay.color = color;
            }
        }
        
        /// <summary>
        /// 색상 표시 활성화/비활성화
        /// </summary>
        private void SetColorDisplayActive(bool active)
        {
            // CircularHuePicker의 colorOutput 제어
            if (huePicker != null)
            {
                huePicker.SetColorOutputActive(active);
                Debug.Log($"Hue 피커 색상 출력: {(active ? "활성화" : "비활성화")}");
            }
            
            // 추가 현재 색상 표시가 있다면 같이 제어
            if (currentColorDisplay != null)
            {
                currentColorDisplay.gameObject.SetActive(active);
                Debug.Log($"현재 색상 표시: {(active ? "활성화" : "비활성화")}");
            }
        }
        
        /// <summary>
        /// 플레이어 선택 텍스트 활성화/비활성화
        /// </summary>
        private void SetSelectPlayerTextActive(bool active)
        {
            if (selectPlayerText != null)
            {
                selectPlayerText.gameObject.SetActive(active);
                Debug.Log($"선택 텍스트: {(active ? "활성화" : "비활성화")}");
            }
        }
        
        /// <summary>
        /// 모든 추리 리셋
        /// </summary>
        private void ResetAllDeductions()
        {
            Debug.Log("=== 모든 추리 리셋 시작 ===");
            
            // 모든 플레이어 색상 제거
            playerColors.Clear();
            
            // 저장된 색상 데이터 삭제
            ClearAllSavedColors();
            
            // 모든 플레이어 카드 리셋
            foreach (var card in playerCards)
            {
                if (card != null)
                {
                    card.ResetColor();
                    card.SetSelected(false);
                }
            }
            
            // 선택 해제
            selectedPlayer = null;
            
            // UI 초기 상태로
            SetInitialUIState();
            
            Debug.Log("=== 모든 추리 리셋 완료 ===");
        }
        
        /// <summary>
        /// 저장된 모든 색상 데이터 삭제
        /// </summary>
        private void ClearAllSavedColors()
        {
            foreach (var playerInfo in testPlayers)
            {
                string colorKey = $"PlayerColor_{playerInfo.playerId}";
                PlayerPrefs.DeleteKey(colorKey);
            }
            PlayerPrefs.Save();
            Debug.Log("저장된 모든 색상 데이터 삭제됨");
        }
        
        /// <summary>
        /// 뒤로가기 버튼 클릭
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("뒤로가기 버튼 클릭됨");
            
            // 플레이어가 선택되어 있으면 선택 해제
            if (selectedPlayer != null)
            {
                selectedPlayer.SetSelected(false);
                selectedPlayer = null;
                SetInitialUIState();
                Debug.Log("플레이어 선택 해제됨");
            }
            else
            {
                // 패널 닫기
                if (uiManager != null)
                {
                    uiManager.CloseDeductionPanel();
                    Debug.Log("추리 패널 닫기 요청됨");
                }
                else
                {
                    Debug.LogError("uiManager가 할당되지 않았습니다!");
                }
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 해제
            if (huePicker != null)
            {
                huePicker.OnColorChanged -= OnColorSelected;
            }
        }
    }
}
