using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 긴급 소집 인터랙티브 오브젝트 (간단 버전)
    /// 상호작용 시 패널을 열고 진행율 표시, 조건 충족 시 버튼 활성화
    /// </summary>
    public class InteractiveEmergencyMeeting : InteractiveTriggerBase
    {
        [Header("UI References")]
        [SerializeField] private GameObject emergencyPopupUI; // 긴급 소집 팝업 UI

        [Header("UI Components")]
        [SerializeField] private Button confirmButton; // 긴급 소집 확인 버튼
        [SerializeField] private Button closeButton; // 닫기 버튼
        [SerializeField] private TMPro.TMP_Text progressText; // 진행율 텍스트 (X / Y)
        [SerializeField] private TMPro.TMP_Text statusText; // 상태 텍스트

        private bool _isUIOpen = false;

        protected override void Awake()
        {
            base.Awake();

            // UI 초기화
            if (emergencyPopupUI != null)
            {
                emergencyPopupUI.SetActive(false);
            }

            // 버튼 이벤트 연결
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseUI);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseUI);
            }
        }

        private void Update()
        {
            // UI가 열려있을 때
            if (_isUIOpen)
            {
                UpdateUI();

                // ESC 키로 닫기
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseUI();
                }
            }
        }

        protected override bool CanInteract()
        {
            // 내 플레이어가 살아있는지 확인
            int myActorId = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorId < 0)
                return false;

            if (!GameDataManager.Instance.TryGetInGameDataByActorId(myActorId, out var myInGameData))
                return false;

            // 죽은 플레이어는 긴급 소집 불가
            if (!myInGameData.isAlive)
                return false;

            // 미팅이 진행 중이면 사용 불가
            if (MeetingManager.Instance != null && MeetingManager.Instance.IsMeetingActive())
                return false;

            return true;
        }

        protected override void HandleInteractLocal()
        {
            OpenUI();
        }

        /// <summary>
        /// UI 열기
        /// </summary>
        private void OpenUI()
        {
            if (emergencyPopupUI == null)
            {
                Debug.LogWarning("[InteractiveEmergencyMeeting] Emergency popup UI is not assigned!");
                return;
            }

            if (_isUIOpen)
                return;

            _isUIOpen = true;
            emergencyPopupUI.SetActive(true);

            // 플레이어 입력 차단
            if (PlayerManager.Instance?.GetMyPlayer()?.GetComponent<PlayerControl>() is PlayerControl control)
            {
                control.SetControlEnabled(false);
            }

            UpdateUI();
            Debug.Log("[InteractiveEmergencyMeeting] UI opened");
        }

        /// <summary>
        /// UI 닫기
        /// </summary>
        private void CloseUI()
        {
            if (!_isUIOpen)
                return;

            _isUIOpen = false;

            if (emergencyPopupUI != null)
            {
                emergencyPopupUI.SetActive(false);
            }

            // 플레이어 입력 복구
            if (PlayerManager.Instance?.GetMyPlayer()?.GetComponent<PlayerControl>() is PlayerControl control)
            {
                control.SetControlEnabled(true);
            }

            Debug.Log("[InteractiveEmergencyMeeting] UI closed");
        }

        /// <summary>
        /// UI 업데이트 (진행율 및 버튼 상태)
        /// </summary>
        private void UpdateUI()
        {
            if (MissionManager.Instance == null || GameDataManager.Instance == null)
                return;

            // 미션 진행도 계산
            float currentProgress = MissionManager.Instance.CalculateMissionProgress();
            var rules = GameDataManager.Instance.GetGameRules();
            float requiredProgress = rules?.minMissionProgressForEmergency ?? 0.3f;

            // 진행율 텍스트 업데이트 (X / Y 형태)
            if (progressText != null)
            {
                int currentPercent = Mathf.RoundToInt(currentProgress * 100f);
                int requiredPercent = Mathf.RoundToInt(requiredProgress * 100f);
                progressText.text = $"{currentPercent}% / {requiredPercent}%";
            }

            // 조건 확인
            bool canCall = CheckCanCall(currentProgress, requiredProgress, out string statusMessage);

            // 상태 텍스트 업데이트
            if (statusText != null)
            {
                statusText.text = statusMessage;
            }

            // 버튼 활성화/비활성화
            if (confirmButton != null)
            {
                confirmButton.interactable = canCall;
            }
        }

        /// <summary>
        /// 긴급 소집 가능 여부 확인
        /// </summary>
        private bool CheckCanCall(float currentProgress, float requiredProgress, out string statusMessage)
        {
            // 1. 쿨다운 체크
            if (MeetingManager.Instance != null && !MeetingManager.Instance.CanCallEmergencyMeeting())
            {
                float remaining = MeetingManager.Instance.GetEmergencyMeetingCooldownRemaining();
                statusMessage = $"쿨다운: {Mathf.CeilToInt(remaining)}초 남음";
                return false;
            }

            // 2. 미션 진행도 체크
            if (currentProgress < requiredProgress)
            {
                statusMessage = "미션 진행도 부족";
                return false;
            }

            // 조건 충족
            statusMessage = "긴급 소집 가능";
            return true;
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            int myActorId = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorId < 0)
            {
                Debug.LogWarning("[InteractiveEmergencyMeeting] Cannot get local player actor ID.");
                return;
            }

            Debug.Log($"[InteractiveEmergencyMeeting] Player {myActorId} called emergency meeting");

            // MeetingManager에 긴급 소집 요청
            if (MeetingManager.Instance != null)
            {
                MeetingManager.Instance.RequestEmergencyMeeting(myActorId);
            }

            CloseUI();
        }
    }
}
