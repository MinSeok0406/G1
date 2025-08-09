using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 인게임 UI 메인 매니저
    /// 모든 UI 모듈들을 관리하는 중앙 컨트롤러
    /// </summary>
    public class InGameUIManager : MonoBehaviour
    {
        [Header("UI 패널들")]
        [SerializeField] private GameObject settingsPanel;     // 전체 설정 패널 (기본 비활성화)
        [SerializeField] private GameObject graphicsPanel;     // 그래픽 설정 패널
        [SerializeField] private GameObject soundPanel;        // 사운드 설정 패널  
        [SerializeField] private GameObject gameplayPanel;     // 게임플레이 설정 패널
        
        [Header("설정 열기/닫기 버튼")]
        [SerializeField] private Button openSettingsButton;    // 설정 열기 버튼
        [SerializeField] private Button closeSettingsButton;   // 설정 닫기 버튼 (뒤로가기)
        
        [Header("탭 버튼들")]
        [SerializeField] private Button graphicsTabButton;     // 그래픽 탭 버튼
        [SerializeField] private Button soundTabButton;        // 사운드 탭 버튼
        [SerializeField] private Button gameplayTabButton;     // 게임플레이 탭 버튼
        
        [Header("게임 나가기 버튼")]
        [SerializeField] private Button exitGameButton;        // 게임 나가기 버튼
        
        [Header("추리 시스템")]
        [SerializeField] private Button deductionButton;       // 추리 시스템 열기 버튼
        [SerializeField] private GameObject deductionPanel;    // 추리 시스템 패널
        
        [Header("UI 모듈들")]
        [SerializeField] private GraphicsSettingsModule graphicsModule;   // 그래픽 설정 모듈
        [SerializeField] private SoundSettingsModule soundModule;         // 사운드 설정 모듈
        [SerializeField] private GameplaySettingsModule gameplayModule;   // 게임플레이 설정 모듈
        [SerializeField] private LeaveGameModule leaveGameModule;         // 나가기 모듈
        
        [Header("UI 테마 설정")]
        [SerializeField] private Color selectedTabColor = Color.white;      // 선택된 탭 색상
        [SerializeField] private Color normalTabColor = Color.gray;         // 일반 탭 색상
        
        // 현재 활성화된 탭
        private int currentTabIndex = 0;
        
        // 설정 패널 상태
        private bool isSettingsPanelOpen = false;
        
        // 추리 패널 상태
        private bool isDeductionPanelOpen = false;
        
        void Start()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 모듈들 초기화
            InitializeModules();
            
            // 버튼 이벤트 설정
            SetupTabButtons();
            
            // 설정 패널 초기 상태 (닫힘)
            CloseSettingsPanel();
            
            // 추리 패널 초기 상태 (닫힘)
            CloseDeductionPanel();
        }
        
        /// <summary>
        /// 모든 모듈 초기화
        /// </summary>
        private void InitializeModules()
        {
            if (graphicsModule != null)
                graphicsModule.Initialize();
                
            if (soundModule != null)
                soundModule.Initialize();
                
            if (gameplayModule != null)
                gameplayModule.Initialize();
                
            if (leaveGameModule != null)
                leaveGameModule.Initialize();
        }
        
        /// <summary>
        /// 탭 버튼 이벤트 설정
        /// </summary>
        private void SetupTabButtons()
        {
            // 설정 열기/닫기 버튼
            if (openSettingsButton != null)
                openSettingsButton.onClick.AddListener(() => OpenSettingsPanel());
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(() => CloseSettingsPanel());
            
            // 탭 버튼들
            if (graphicsTabButton != null)
                graphicsTabButton.onClick.AddListener(() => SwitchTab(0));
                
            if (soundTabButton != null)
                soundTabButton.onClick.AddListener(() => SwitchTab(1));
                
            if (gameplayTabButton != null)
                gameplayTabButton.onClick.AddListener(() => SwitchTab(2));
                
            // 게임 나가기 버튼 이벤트 설정
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(() => OnExitGameButtonClicked());
            
            // 추리 시스템 버튼 이벤트 설정
            if (deductionButton != null)
                deductionButton.onClick.AddListener(() => ToggleDeductionPanel());
        }
        
        /// <summary>
        /// 탭 전환
        /// </summary>
        /// <param name="tabIndex">전환할 탭 인덱스 (0: 그래픽, 1: 사운드, 2: 게임플레이)</param>
        public void SwitchTab(int tabIndex)
        {
            currentTabIndex = tabIndex;
            
            // 모든 패널 비활성화
            DeactivateAllPanels();
            
            // 모든 탭 버튼 색상 초기화
            ResetAllTabColors();
            
            // 선택된 탭에 따라 패널 활성화 및 버튼 색상 변경
            switch (tabIndex)
            {
                case 0: // 그래픽 설정
                    if (graphicsPanel != null) graphicsPanel.SetActive(true);
                    SetTabButtonColor(graphicsTabButton, selectedTabColor);
                    break;
                    
                case 1: // 사운드 설정
                    if (soundPanel != null) soundPanel.SetActive(true);
                    SetTabButtonColor(soundTabButton, selectedTabColor);
                    break;
                    
                case 2: // 게임플레이 설정
                    if (gameplayPanel != null) gameplayPanel.SetActive(true);
                    SetTabButtonColor(gameplayTabButton, selectedTabColor);
                    break;
            }
        }
        
        /// <summary>
        /// 모든 패널 비활성화
        /// </summary>
        private void DeactivateAllPanels()
        {
            if (graphicsPanel != null) graphicsPanel.SetActive(false);
            if (soundPanel != null) soundPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
        }
        
        /// <summary>
        /// 모든 탭 버튼 색상 초기화
        /// </summary>
        private void ResetAllTabColors()
        {
            SetTabButtonColor(graphicsTabButton, normalTabColor);
            SetTabButtonColor(soundTabButton, normalTabColor);
            SetTabButtonColor(gameplayTabButton, normalTabColor);
        }
        
        /// <summary>
        /// 게임 나가기 버튼 클릭 처리
        /// </summary>
        private void OnExitGameButtonClicked()
        {
            if (leaveGameModule != null)
            {
                leaveGameModule.ShowConfirmation();
            }
        }
        
        #region 설정 패널 관리
        
        /// <summary>
        /// 설정 패널 열기
        /// </summary>
        public void OpenSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                isSettingsPanelOpen = true;
                
                // 첫 번째 탭 활성화 (그래픽 설정)
                SwitchTab(0);
            }
        }
        
        /// <summary>
        /// 설정 패널 닫기
        /// </summary>
        public void CloseSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                isSettingsPanelOpen = false;
                
                // 모든 패널 비활성화
                DeactivateAllPanels();
            }
        }
        
        /// <summary>
        /// 뒤로가기 키 처리 (ESC 키)
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 패널 우선순위: 추리 패널 -> 설정 패널
                if (isDeductionPanelOpen)
                {
                    CloseDeductionPanel();
                }
                else if (isSettingsPanelOpen)
                {
                    CloseSettingsPanel();
                }
                else
                {
                    OpenSettingsPanel();
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// 탭 버튼 색상 설정
        /// </summary>
        /// <param name="button">색상을 변경할 버튼</param>
        /// <param name="color">적용할 색상</param>
        private void SetTabButtonColor(Button button, Color color)
        {
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = color;
                button.colors = colors;
            }
        }
        
        /// <summary>
        /// 외부에서 특정 모듈에 접근할 수 있는 메서드들
        /// </summary>
        public GraphicsSettingsModule GetGraphicsModule() => graphicsModule;
        public SoundSettingsModule GetSoundModule() => soundModule;
        public GameplaySettingsModule GetGameplayModule() => gameplayModule;
        public LeaveGameModule GetLeaveGameModule() => leaveGameModule;
        
        /// <summary>
        /// 현재 활성화된 탭 인덱스 반환
        /// </summary>
        public int GetCurrentTab() => currentTabIndex;
        
        /// <summary>
        /// 설정 패널 상태 확인
        /// </summary>
        public bool IsSettingsPanelOpen() => isSettingsPanelOpen;
        
        /// <summary>
        /// 추리 패널 상태 확인
        /// </summary>
        public bool IsDeductionPanelOpen() => isDeductionPanelOpen;
        
        /// <summary>
        /// 추리 패널 토글
        /// </summary>
        public void ToggleDeductionPanel()
        {
            if (isDeductionPanelOpen)
            {
                CloseDeductionPanel();
            }
            else
            {
                OpenDeductionPanel();
            }
        }
        
        /// <summary>
        /// 추리 패널 열기
        /// </summary>
        public void OpenDeductionPanel()
        {
            if (deductionPanel != null)
            {
                // 다른 패널들 닫기
                CloseSettingsPanel();
                
                // 추리 패널 활성화
                deductionPanel.SetActive(true);
                isDeductionPanelOpen = true;
            }
        }
        
        /// <summary>
        /// 추리 패널 닫기
        /// </summary>
        public void CloseDeductionPanel()
        {
            if (deductionPanel != null)
            {
                deductionPanel.SetActive(false);
                isDeductionPanelOpen = false;
            }
        }
        
        /// <summary>
        /// UI 매니저 활성화/비활성화
        /// </summary>
        /// <param name="active">활성화 여부</param>
        public void SetUIActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        #region Inspector 연결용 공개 메서드
        
        /// <summary>
        /// 설정 열기 (Inspector에서 연결 가능)
        /// </summary>
        public void OpenSettings() => OpenSettingsPanel();
        
        /// <summary>
        /// 설정 닫기 (Inspector에서 연결 가능)
        /// </summary>
        public void CloseSettings() => CloseSettingsPanel();
        
        /// <summary>
        /// 추리 패널 열기 (Inspector에서 연결 가능)
        /// </summary>
        public void OpenDeduction() => OpenDeductionPanel();
        
        /// <summary>
        /// 추리 패널 닫기 (Inspector에서 연결 가능)
        /// </summary>
        public void CloseDeduction() => CloseDeductionPanel();
        
        /// <summary>
        /// 추리 패널 토글 (Inspector에서 연결 가능)
        /// </summary>
        public void ToggleDeduction() => ToggleDeductionPanel();
        
        #endregion
    }
}
    