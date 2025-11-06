using Photon.Pun;
using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 미팅 타입
    /// </summary>
    public enum MeetingType
    {
        BodyReport = 0,     // 시체 발견
        Emergency = 1       // 긴급 소집
    }

    /// <summary>
    /// 미팅 시작 및 관리
    /// - 시체 발견 미팅
    /// - 긴급 소집 미팅
    /// </summary>
    public sealed class MeetingManager : SingletonNetworkBehaviour<MeetingManager>
    {
        #region Events
        public event Action<MeetingType> OnMeetingStarted;
        public event Action OnMeetingEnded;
        #endregion

        #region Private Fields
        private bool _isMeetingActive = false;
        private MeetingType _currentMeetingType;
        private float _lastEmergencyMeetingTime = -9999f; // 마지막 긴급 소집 시간
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region Public API
        /// <summary>
        /// [Client] 시체 발견 미팅 요청
        /// </summary>
        public void RequestBodyReportMeeting(int reporterActorId, Vector3 bodyPosition)
        {
            if (_isMeetingActive)
            {
                Debug.LogWarning("[MeetingManager] Meeting is already active.");
                return;
            }

            photonView.RPC(nameof(RPC_RequestBodyReportMeeting), RpcTarget.MasterClient, reporterActorId, bodyPosition);
        }

        /// <summary>
        /// [Client] 긴급 소집 미팅 요청
        /// </summary>
        public void RequestEmergencyMeeting(int callerActorId)
        {
            if (_isMeetingActive)
            {
                Debug.LogWarning("[MeetingManager] Meeting is already active.");
                return;
            }

            // 쿨다운 체크 (클라이언트 측)
            if (!CanCallEmergencyMeeting())
            {
                if (UIManager.Instance != null)
                {
                    float remaining = GetEmergencyMeetingCooldownRemaining();
                    UIManager.Instance.ShowToastToScreen($"긴급 소집은 {remaining:F0}초 후에 사용할 수 있습니다.");
                }
                return;
            }

            // 미션 진행도 체크 (클라이언트 측)
            float currentProgress = MissionManager.Instance?.CalculateMissionProgress() ?? 0f;
            var rules = GameDataManager.Instance?.GetGameRules();
            float requiredProgress = rules?.minMissionProgressForEmergency ?? 0.3f;

            if (currentProgress < requiredProgress)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowToastToScreen($"긴급 소집은 미션 진행도 {(requiredProgress * 100):F0}% 이상 필요합니다.");
                }
                return;
            }

            photonView.RPC(nameof(RPC_RequestEmergencyMeeting), RpcTarget.MasterClient, callerActorId);
        }

        /// <summary>
        /// 긴급 소집 사용 가능 여부 확인
        /// </summary>
        public bool CanCallEmergencyMeeting()
        {
            var rules = GameDataManager.Instance?.GetGameRules();
            float cooldown = rules?.emergencyMeetingCooldown ?? 30f;

            return (Time.time - _lastEmergencyMeetingTime) >= cooldown;
        }

        /// <summary>
        /// 긴급 소집 쿨다운 남은 시간 반환
        /// </summary>
        public float GetEmergencyMeetingCooldownRemaining()
        {
            var rules = GameDataManager.Instance?.GetGameRules();
            float cooldown = rules?.emergencyMeetingCooldown ?? 30f;

            float elapsed = Time.time - _lastEmergencyMeetingTime;
            return Mathf.Max(0f, cooldown - elapsed);
        }

        /// <summary>
        /// [Host Only] 미팅 종료
        /// </summary>
        public void EndMeeting()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[MeetingManager] EndMeeting is host-only.");
                return;
            }

            if (!_isMeetingActive)
            {
                Debug.LogWarning("[MeetingManager] No active meeting to end.");
                return;
            }

            _isMeetingActive = false;

            Debug.Log("[MeetingManager] Meeting ended.");

            OnMeetingEnded?.Invoke();

            // 브로드캐스트
            photonView.RPC(nameof(RPC_OnMeetingEnded), RpcTarget.All);

            // 라운드 재시작
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.StartNewRound();
            }
        }

        /// <summary>
        /// 미팅 활성 상태 반환
        /// </summary>
        public bool IsMeetingActive() => _isMeetingActive;

        /// <summary>
        /// 현재 미팅 타입 반환
        /// </summary>
        public MeetingType GetCurrentMeetingType() => _currentMeetingType;
        #endregion

        #region RPC - Body Report Meeting
        [PunRPC]
        private void RPC_RequestBodyReportMeeting(int reporterActorId, Vector3 bodyPosition, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (_isMeetingActive)
            {
                Debug.LogWarning("[MeetingManager] Meeting is already active.");
                return;
            }

            // 발신자 검증
            if (info.Sender == null || info.Sender.ActorNumber != reporterActorId)
            {
                Debug.LogWarning($"[MeetingManager] Spoofed body report request. Sender={info.Sender?.ActorNumber}, Arg={reporterActorId}");
                return;
            }

            // 신고자가 생존 상태인지 확인
            if (!GameDataManager.Instance.TryGetPublicPlayerDataByActorId(reporterActorId, out var reporterData))
            {
                Debug.LogWarning($"[MeetingManager] Reporter data not found for actor {reporterActorId}");
                return;
            }

            var reporterInGameData = GameDataManager.Instance.GetInGameData(reporterData.googleUID);
            if (reporterInGameData == null || !reporterInGameData.isAlive)
            {
                Debug.LogWarning($"[MeetingManager] Reporter {reporterActorId} is not alive.");
                return;
            }

            // 시체 발견 미팅 시작
            StartMeeting(MeetingType.BodyReport, reporterActorId, bodyPosition);
        }
        #endregion

        #region RPC - Emergency Meeting
        [PunRPC]
        private void RPC_RequestEmergencyMeeting(int callerActorId, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (_isMeetingActive)
            {
                Debug.LogWarning("[MeetingManager] Meeting is already active.");
                return;
            }

            // 발신자 검증
            if (info.Sender == null || info.Sender.ActorNumber != callerActorId)
            {
                Debug.LogWarning($"[MeetingManager] Spoofed emergency meeting request. Sender={info.Sender?.ActorNumber}, Arg={callerActorId}");
                return;
            }

            // 호출자가 생존 상태인지 확인
            if (!GameDataManager.Instance.TryGetPublicPlayerDataByActorId(callerActorId, out var callerData))
            {
                Debug.LogWarning($"[MeetingManager] Caller data not found for actor {callerActorId}");
                return;
            }

            var callerInGameData = GameDataManager.Instance.GetInGameData(callerData.googleUID);
            if (callerInGameData == null || !callerInGameData.isAlive)
            {
                Debug.LogWarning($"[MeetingManager] Caller {callerActorId} is not alive.");
                return;
            }

            // 쿨다운 체크 (호스트 측 추가 검증)
            var rules = GameDataManager.Instance?.GetGameRules();
            float cooldown = rules?.emergencyMeetingCooldown ?? 30f;
            if ((Time.time - _lastEmergencyMeetingTime) < cooldown)
            {
                Debug.LogWarning($"[MeetingManager] Emergency meeting on cooldown.");
                return;
            }

            // 미션 진행도 체크 (호스트 측 추가 검증)
            float currentProgress = MissionManager.Instance?.CalculateMissionProgress() ?? 0f;
            float requiredProgress = rules?.minMissionProgressForEmergency ?? 0.3f;
            if (currentProgress < requiredProgress)
            {
                Debug.LogWarning($"[MeetingManager] Insufficient mission progress for emergency meeting: {currentProgress:F2}/{requiredProgress:F2}");
                return;
            }

            // 긴급 소집 미팅 시작
            StartMeeting(MeetingType.Emergency, callerActorId, Vector3.zero);
        }
        #endregion

        #region Meeting Start Logic
        /// <summary>
        /// [Host Only] 미팅 시작
        /// </summary>
        private void StartMeeting(MeetingType meetingType, int initiatorActorId, Vector3 position)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            _isMeetingActive = true;
            _currentMeetingType = meetingType;

            // 긴급 소집인 경우 쿨다운 기록
            if (meetingType == MeetingType.Emergency)
            {
                _lastEmergencyMeetingTime = Time.time;
            }

            Debug.Log($"[MeetingManager] Starting {meetingType} meeting by actor {initiatorActorId}");

            OnMeetingStarted?.Invoke(meetingType);

            // 브로드캐스트
            photonView.RPC(nameof(RPC_OnMeetingStarted), RpcTarget.All, (int)meetingType, initiatorActorId, position);

            // 게임 스테이트를 Meeting으로 변경
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RequestPhaseChange(GameStateType.Meeting);
            }
        }

        [PunRPC]
        private void RPC_OnMeetingStarted(int meetingTypeInt, int initiatorActorId, Vector3 position)
        {
            _isMeetingActive = true;
            _currentMeetingType = (MeetingType)meetingTypeInt;

            if (_currentMeetingType == MeetingType.Emergency)
            {
                _lastEmergencyMeetingTime = Time.time;
            }

            string meetingTypeName = _currentMeetingType == MeetingType.BodyReport ? "시체 발견" : "긴급 소집";
            Debug.Log($"[MeetingManager] {meetingTypeName} 미팅 시작 (호출자: {initiatorActorId})");

            OnMeetingStarted?.Invoke(_currentMeetingType);

            // UI 표시
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMeetingUI(true);
                UIManager.Instance.ShowToastToScreen(meetingTypeName);
            }
        }

        [PunRPC]
        private void RPC_OnMeetingEnded()
        {
            _isMeetingActive = false;

            Debug.Log("[MeetingManager] Meeting ended.");

            OnMeetingEnded?.Invoke();

            // UI 숨기기
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMeetingUI(false);
            }
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugStartBodyReportMeeting()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[MeetingManager] Debug command is host-only.");
                return;
            }

            int myActorId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            StartMeeting(MeetingType.BodyReport, myActorId, Vector3.zero);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugStartEmergencyMeeting()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[MeetingManager] Debug command is host-only.");
                return;
            }

            int myActorId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            StartMeeting(MeetingType.Emergency, myActorId, Vector3.zero);
        }
        #endregion
    }
}
