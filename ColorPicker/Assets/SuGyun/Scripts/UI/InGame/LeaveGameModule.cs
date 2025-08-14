using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 게임 나가기 확인 모듈
    /// </summary>
    public class LeaveGameModule : MonoBehaviour
    {
        [Header("게임 나가기 UI")]
        [SerializeField] private GameObject confirmationPanel; // 확인 패널
        [SerializeField] private Button confirmButton;     // 나가기 확인 버튼
        [SerializeField] private Button cancelButton;      // 취소 버튼
        [SerializeField] private TextMeshProUGUI confirmationText; // 확인 메시지 텍스트
        
        [Header("확인 메시지")]
        [SerializeField] private string defaultMessage = ""; // Unity에서 텍스트 직접 설정
        
        // UI 매니저 참조
        private InGameUIManager uiManager;
        
        public void Initialize()
        {
            // UI 매니저 참조 가져오기
            uiManager = GetComponentInParent<InGameUIManager>();
            if (uiManager == null)
                uiManager = FindObjectOfType<InGameUIManager>();
            
            SetupButtonEvents();
            
            // 초기에는 확인 패널 숨기기
            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);
        }
        
        #region 버튼 이벤트 설정
        
        private void SetupButtonEvents()
        {
            if (confirmButton) confirmButton.onClick.AddListener(OnConfirmLeaveGame);
            if (cancelButton) cancelButton.onClick.AddListener(OnCancelLeaveGame);
        }
        
        #endregion
        
        #region 확인 화면 표시
        
        /// <summary>
        /// 게임 나가기 확인 화면 표시
        /// </summary>
        public void ShowConfirmation()
        {
            // 확인 패널 활성화
            if (confirmationPanel != null)
                confirmationPanel.SetActive(true);
        }
        
        /// <summary>
        /// 커스텀 메시지로 확인 화면 표시
        /// </summary>
        public void ShowConfirmation(string customMessage)
        {
            // 특별한 경우에만 메시지 변경
            if (!string.IsNullOrEmpty(customMessage) && confirmationText)
                confirmationText.text = customMessage;
        }
        
        private void SetDefaultMessage()
        {
            // Unity Inspector에서 설정된 텍스트 사용, 코드에서 변경하지 않음
        }
        
        #endregion
        
        #region 버튼 이벤트 처리
        
        /// <summary>
        /// 게임 나가기 확인 버튼 클릭
        /// </summary>
        public void OnConfirmLeaveGame()
        {
            Debug.Log("게임을 나갑니다...");
            
            // 설정 저장
            SaveAllSettings();
            
            // 게임 종료 또는 로비로 이동
            LeaveGame();
        }
        
        /// <summary>
        /// 취소 버튼 클릭
        /// </summary>
        public void OnCancelLeaveGame()
        {
            Debug.Log("게임 나가기를 취소했습니다.");
            
            // 확인 패널 숨기기
            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);
        }
        
        #endregion
        
        #region 게임 나가기 처리
        
        private void LeaveGame()
        {
            // 개발 환경에서는 플레이 모드 종료
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                // 빌드된 게임에서는 실제 종료 또는 로비로 이동
                // 로비 씬이 있다면 로비로 이동
                if (HasLobbyScene())
                {
                    LoadLobbyScene();
                }
                else
                {
                    // 로비 씬이 없다면 게임 종료
                    Application.Quit();
                }
            #endif
        }
        
        private bool HasLobbyScene()
        {
            // 로비 씬 존재 여부 확인
            // 예: "Lobby", "MainMenu", "InLobby" 등의 씬이 있는지 확인
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName.Contains("Lobby") || sceneName.Contains("MainMenu") || sceneName.Contains("InLobby"))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void LoadLobbyScene()
        {
            // 로비 씬 로드
            // 씬 이름을 정확히 맞춰서 로드
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("InLobby");
            }
            catch
            {
                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
                }
                catch
                {
                    try
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                    }
                    catch
                    {
                        // 로비 씬 로드 실패 시 게임 종료
                        Debug.LogWarning("로비 씬을 찾을 수 없습니다. 게임을 종료합니다.");
                        Application.Quit();
                    }
                }
            }
        }
        
        #endregion
        
        #region 설정 저장
        
        private void SaveAllSettings()
        {
            // UI 매니저를 통해 모든 설정 저장
            if (uiManager != null)
            {
                // InGameUIManager의 SaveAllSettings 메서드 호출
                // uiManager.SaveAllSettings(); // 필요시 추가
            }
            
            // 직접 PlayerPrefs 저장
            PlayerPrefs.Save();
            
            Debug.Log("모든 설정이 저장되었습니다.");
        }
        
        #endregion
        
        #region Inspector 연결용 공개 메서드
        
        /// <summary>
        /// 게임 나가기 확인 (Inspector에서 연결 가능)
        /// </summary>
        public void ConfirmLeaveGame() => OnConfirmLeaveGame();
        
        /// <summary>
        /// 게임 나가기 취소 (Inspector에서 연결 가능)
        /// </summary>
        public void CancelLeaveGame() => OnCancelLeaveGame();
        
        #endregion
        
        #region 공개 메서드
        
        /// <summary>
        /// 확인 메시지 설정
        /// </summary>
        public void SetConfirmationMessage(string message)
        {
            if (confirmationText)
                confirmationText.text = message;
        }
        
        /// <summary>
        /// 기본 메시지로 리셋
        /// </summary>
        public void ResetToDefaultMessage()
        {
            SetDefaultMessage();
        }
        
        #endregion
        
        #region 프로퍼티
        
        /// <summary>
        /// 현재 확인 메시지
        /// </summary>
        public string CurrentMessage => confirmationText != null ? confirmationText.text : defaultMessage;
        
        /// <summary>
        /// 기본 확인 메시지
        /// </summary>
        public string DefaultMessage => defaultMessage;
        
        #endregion
    }
}
