using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        #region Properties
        public GameStateMachine StateMachine { get; private set; }
        public PlayerClassAssigner playerClassAssigner { get; private set; }
        public GameStartedState GameStartedState { get; private set; }
        public PlayingGameState PlayingGameState { get; private set; }
        public MeetingState MeetingState { get; private set; }
        
        [HideInInspector] public float meetingTimer;
        #endregion

        #region Constants
        private const double ADD_COOLDOWN_SEC = 1.0;
        private const float ALL_VOTED_CAP_SECONDS = 3f;
        private const float MIN_TIME_FOR_REDUCTION = 10f; // 감소 가능한 최소 시간
        private const float MIN_TIME_AFTER_REDUCTION = 10f; // 감소 후 최소 시간
        private const string GAME_PHASE_KEY = "GamePhase";
        #endregion

        #region Private Fields
        private readonly Dictionary<int, double> _addTimerCooldownUntil = new Dictionary<int, double>();
        private readonly HashSet<int> _meetingAddRequesters = new HashSet<int>();
        private readonly HashSet<int> _meetingVoters = new HashSet<int>();
        private readonly Dictionary<int, int> _voteCounts = new Dictionary<int, int>();
        
        private bool _allVotedCapped;
        private bool _hasUsedTimeModification = false; // 시간 증가/감소 사용 여부
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            InitializeGameStates();
        }

        private void Start()
        {
            StateMachine?.Initialize(GameStartedState);
        }

        private void Update()
        {
            StateMachine?.CurrentState?.Update();

            #if UNITY_EDITOR
            DebugPrintPlayerData();
            #endif
        }
        #endregion

        #region Initialization
        private void InitializeGameStates()
        {
            StateMachine = new GameStateMachine();
            playerClassAssigner = new PlayerClassAssigner();

            GameStartedState = new GameStartedState(StateMachine);
            PlayingGameState = new PlayingGameState(StateMachine);
            MeetingState = new MeetingState(StateMachine);
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DebugPrintPlayerData()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (var data in GameDataManager.Instance.GetAllPrivatePlayerData())
                {
                    Debug.Log($"Color: {(ColorType)data.identityColorId}, Class: {(PlayerClassType)data.classType}");
                }
            }
        }
        #endregion

        #region Photon Callbacks
        /// <summary>
        /// Room Custom Properties가 변경되었을 때 호출됨 (모든 클라이언트에서 동기화)
        /// </summary>
        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (!changedProps.TryGetValue(GAME_PHASE_KEY, out object value)) return;

            var newPhase = (GameStateType)(int)value;
            var currentPhase = HelperUtilities.ToPhase(StateMachine.CurrentState);

            if (newPhase != currentPhase)
            {
                ChangeStateFromPhase(newPhase);
            }
        }
        #endregion

        #region Phase Management
        /// <summary>
        /// [호스트 전용] Room Custom Properties를 통해 게임 상태를 설정하여 모든 클라이언트에게 동기화한다.
        /// </summary>
        public void SetPhase(GameStateType phase)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[GameManager] SetPhase는 마스터 클라이언트만 호출할 수 있습니다.");
                return;
            }

            var properties = new Hashtable { { GAME_PHASE_KEY, (int)phase } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        /// <summary>
        /// [클라이언트 공통] Room Custom Properties로 전달받은 페이즈 정보로부터 로컬 게임 상태를 전환한다.
        /// </summary>
        public void ChangeStateFromPhase(GameStateType phase)
        {
            var newState = HelperUtilities.ToState(phase);

            if (newState != null)
            {
                StateMachine.ChangeState(newState);
            }
            else
            {
                Debug.LogError($"[GameManager] 알 수 없는 게임 페이즈: {phase}");
            }
        }

        /// <summary>
        /// 클라이언트가 마스터에게 페이즈 전환을 요청
        /// </summary>
        public void RequestPhaseChange(GameStateType requestedPhase)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetPhase(requestedPhase);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestPhaseChange), RpcTarget.MasterClient, (int)requestedPhase);
            }
        }

        /// <summary>
        /// [RPC][호스트 전용] 일반 클라이언트가 마스터에게 페이즈 전환을 요청할 때 호출
        /// </summary>
        [PunRPC]
        private void RPC_RequestPhaseChange(int phaseInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var requestedPhase = (GameStateType)phaseInt;
            Debug.Log($"[GameManager] {info.Sender.NickName}이(가) 페이즈 전환 요청: {requestedPhase}");

            SetPhase(requestedPhase);
        }
        #endregion

        #region Meeting Timer Management
        /// <summary>
        /// [마스터 전용] 미팅 타이머 초기화
        /// </summary>
        public void SetMeetingTimer(float time = Settings.defaultMeetingTime)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[GameManager] SetMeetingTimer는 마스터 클라이언트만 호출할 수 있습니다.");
                return;
            }

            if (time <= 0)
            {
                Debug.LogWarning($"[GameManager] 유효하지 않은 미팅 시간: {time}");
                return;
            }

            meetingTimer = time;
        }

        /// <summary>
        /// 미팅 시간 추가 요청 (클라이언트 → 마스터)
        /// </summary>
        public void RequestAddMeetingTime(float time = Settings.defaultAddTime)
        {
            if (_hasUsedTimeModification)
            {
                Debug.LogWarning("[GameManager] 시간 증가/감소는 한 번만 사용할 수 있습니다.");
                UIManager.Instance?.ShowToastToScreen("시간 조정은 한 번만 가능합니다.");
                return;
            }

            photonView.RPC(nameof(RPC_AddMeetingTime), RpcTarget.MasterClient, time);
        }

        /// <summary>
        /// [RPC][마스터 전용] 미팅 시간 추가 처리 (중복 요청 방지)
        /// </summary>
        [PunRPC]
        private void RPC_AddMeetingTime(float time, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            if (!ValidateTimeValue(time)) return;

            // 이미 시간 조정을 사용했는지 확인
            if (_hasUsedTimeModification)
            {
                Debug.LogWarning($"[MeetingTimer] 시간 조정은 이미 사용되었습니다.");
                return;
            }

            int senderActorNum = info.Sender.ActorNumber;

            if (_meetingAddRequesters.Contains(senderActorNum))
            {
                Debug.LogWarning($"[MeetingTimer] Actor {senderActorNum}은(는) 이미 시간 추가를 요청했습니다.");
                return;
            }

            _meetingAddRequesters.Add(senderActorNum);
            _hasUsedTimeModification = true;
            meetingTimer += time;
            
            Debug.Log($"[MeetingTimer] Actor {senderActorNum}이(가) {time}초 추가. 현재 시간: {meetingTimer}초");
            
            BroadcastMeetingTimer();
            
            // 시간 조정 사용 상태 동기화
            photonView.RPC(nameof(RPC_SyncTimeModificationState), RpcTarget.All, true);
        }

        /// <summary>
        /// 미팅 시간 감소 요청 (한 번만 가능, 10초 미만 시 불가)
        /// </summary>
        public void RequestReduceMeetingTime(float time = Settings.defaultAddTime)
        {
            if (_hasUsedTimeModification)
            {
                Debug.LogWarning("[GameManager] 시간 증가/감소는 한 번만 사용할 수 있습니다.");
                UIManager.Instance?.ShowToastToScreen("시간 조정은 한 번만 가능합니다.");
                return;
            }

            if (meetingTimer <= MIN_TIME_FOR_REDUCTION)
            {
                Debug.LogWarning($"[GameManager] 남은 시간이 {MIN_TIME_FOR_REDUCTION}초 이하일 때는 시간 감소가 불가능합니다.");
                UIManager.Instance?.ShowToastToScreen($"남은 시간이 {MIN_TIME_FOR_REDUCTION}초 이하일 때는 시간을 줄일 수 없습니다.");
                return;
            }

            photonView.RPC(nameof(RPC_ReduceMeetingTime), RpcTarget.MasterClient, time);
        }

        /// <summary>
        /// [RPC][마스터 전용] 미팅 시간 감소 처리
        /// </summary>
        [PunRPC]
        private void RPC_ReduceMeetingTime(float time, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            if (!ValidateTimeValue(time)) return;

            // 이미 시간 조정을 사용했는지 확인
            if (_hasUsedTimeModification)
            {
                Debug.LogWarning($"[MeetingTimer] 시간 조정은 이미 사용되었습니다.");
                return;
            }

            // 남은 시간이 최소 시간 이하인 경우 감소 불가
            if (meetingTimer <= MIN_TIME_FOR_REDUCTION)
            {
                Debug.LogWarning($"[MeetingTimer] 남은 시간이 {MIN_TIME_FOR_REDUCTION}초 이하이므로 감소할 수 없습니다.");
                return;
            }

            _hasUsedTimeModification = true;
            
            float newTime = meetingTimer - time;
            
            // 감소 후 시간이 10초 미만이면 10초로 고정
            if (newTime < MIN_TIME_AFTER_REDUCTION)
            {
                meetingTimer = MIN_TIME_AFTER_REDUCTION;
                Debug.Log($"[MeetingTimer] Actor {info.Sender.ActorNumber}이(가) 시간 감소 시도. 최소 시간으로 설정: {MIN_TIME_AFTER_REDUCTION}초");
            }
            else
            {
                meetingTimer = newTime;
                Debug.Log($"[MeetingTimer] Actor {info.Sender.ActorNumber}이(가) {time}초 감소. 현재 시간: {meetingTimer}초");
            }
            
            BroadcastMeetingTimer();
            
            // 시간 조정 사용 상태 동기화
            photonView.RPC(nameof(RPC_SyncTimeModificationState), RpcTarget.All, true);
        }

        /// <summary>
        /// [RPC] 시간 조정 사용 상태 동기화
        /// </summary>
        [PunRPC]
        private void RPC_SyncTimeModificationState(bool hasUsed)
        {
            _hasUsedTimeModification = hasUsed;
        }

        /// <summary>
        /// 새로운 미팅 상태 진입 시 호출 → 요청 기록 초기화
        /// </summary>
        public void ResetMeetingRequests()
        {
            _meetingAddRequesters.Clear();
            _meetingVoters.Clear();
            _voteCounts.Clear();
            _allVotedCapped = false;
            _hasUsedTimeModification = false;
        }

        /// <summary>
        /// [마스터 전용] 미팅 타이머를 모든 클라이언트에 동기화
        /// </summary>
        public void BroadcastMeetingTimer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            photonView.RPC(nameof(RPC_SyncMeetingTimer), RpcTarget.All, meetingTimer, PhotonNetwork.Time);
        }

        /// <summary>
        /// [RPC] 미팅 타이머 동기화 (네트워크 지연 보정)
        /// </summary>
        [PunRPC]
        private void RPC_SyncMeetingTimer(float serverRemaining, double sentServerTime)
        {
            double now = PhotonNetwork.Time;
            float networkDelay = (float)(now - sentServerTime);
            float adjustedTime = Mathf.Max(0f, serverRemaining - networkDelay);

            UIManager.Instance?.StartMeetingTimerUI(adjustedTime);
        }
        #endregion

        #region Voting System
        /// <summary>
        /// 투표 요청 (클라이언트 → 마스터)
        /// </summary>
        public void RequestVote(int targetActorNumber)
        {
            photonView.RPC(nameof(RPC_SubmitVote), RpcTarget.MasterClient, targetActorNumber);
        }

        /// <summary>
        /// [RPC][마스터 전용] 투표 제출 처리 (중복 투표 방지)
        /// </summary>
        [PunRPC]
        private void RPC_SubmitVote(int targetActorNumber, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int voterActorNumber = info.Sender.ActorNumber;

            // 중복 투표 방지
            if (!_meetingVoters.Add(voterActorNumber))
            {
                Debug.LogWarning($"[Vote] Actor {voterActorNumber}은(는) 이미 투표했습니다.");
                return;
            }

            // 투표 집계
            if (_voteCounts.ContainsKey(targetActorNumber))
            {
                _voteCounts[targetActorNumber]++;
            }
            else
            {
                _voteCounts[targetActorNumber] = 1;
            }

            Debug.Log($"[Vote] Actor {voterActorNumber}이(가) Actor {targetActorNumber}에게 투표. 현재 득표수: {_voteCounts[targetActorNumber]}");

            CheckAllVotedAndCap();
        }

        /// <summary>
        /// 전원 투표 완료 시 시간 3초로 제한 및 결과 집계
        /// </summary>
        private void CheckAllVotedAndCap()
        {
            if (_allVotedCapped) return;

            int totalPlayers = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
            if (totalPlayers <= 0) return;

            // 전원 투표 완료 확인
            if (_meetingVoters.Count >= totalPlayers)
            {
                _allVotedCapped = true;

                // 남은 시간이 3초 이상이면 3초로 제한
                if (meetingTimer > ALL_VOTED_CAP_SECONDS)
                {
                    meetingTimer = ALL_VOTED_CAP_SECONDS;
                    BroadcastMeetingTimer();
                }

                LogVoteResults();
            }
        }

        /// <summary>
        /// 투표 결과 로그 출력
        /// </summary>
        private void LogVoteResults()
        {
            if (_voteCounts.Count == 0)
            {
                Debug.Log("[Vote Result] 투표 결과가 없습니다.");
                return;
            }

            Debug.Log("===== 투표 결과 =====");
            foreach (var kvp in _voteCounts)
            {
                Debug.Log($"Actor {kvp.Key}: {kvp.Value}표");
            }

            var mostVotedActor = GetMostVotedPlayer();
            
            if (mostVotedActor == -1)
            {
                Debug.Log("[Vote Result] 동표로 인해 추방 없음");
            }
            else
            {
                Debug.Log($"[Vote Result] 최다 득표자: Actor {mostVotedActor}");
            }
        }

        /// <summary>
        /// 최다 득표자 반환 (동점자가 있을 경우 -1 반환)
        /// </summary>
        /// <returns>(ActorNumber, VoteCount) 또는 (-1, maxVotes) if 동표</returns>
        public int GetMostVotedPlayer()
        {
            if (_voteCounts.Count == 0) return -1;

            int maxVotes = -1;
            int mostVotedActor = -1;
            int tieCount = 0; // 최다 득표 동점자 수

            // 1단계: 최대 득표수 찾기
            foreach (var kvp in _voteCounts)
            {
                if (kvp.Value > maxVotes)
                {
                    maxVotes = kvp.Value;
                }
            }

            // 2단계: 최다 득표자 확인 및 동점 체크
            foreach (var kvp in _voteCounts)
            {
                if (kvp.Value == maxVotes)
                {
                    mostVotedActor = kvp.Key;
                    tieCount++;
                }
            }

            // 동점자가 2명 이상이면 -1 반환
            if (tieCount > 1)
            {
                Debug.Log($"[Vote Result] 동표 발생: {tieCount}명이 {maxVotes}표로 동점");
                return -1;
            }

            return mostVotedActor;
        }

        /// <summary>
        /// 투표 결과 딕셔너리 반환 (읽기 전용 복사본)
        /// </summary>
        public Dictionary<int, int> GetVoteDictionary()
        {
            return new Dictionary<int, int>(_voteCounts);
        }

        /// <summary>
        /// 투표한 플레이어 수 반환
        /// </summary>
        public int GetVotedPlayerCount()
        {
            return _meetingVoters.Count;
        }

        /// <summary>
        /// 전체 플레이어 수 반환
        /// </summary>
        public int GetTotalPlayerCount()
        {
            return PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        }

        /// <summary>
        /// 시간 조정(증가/감소) 사용 가능 여부 반환
        /// </summary>
        public bool CanModifyTime()
        {
            return !_hasUsedTimeModification;
        }

        /// <summary>
        /// 시간 감소 가능 여부 반환 (사용 가능 + 10초 초과)
        /// </summary>
        public bool CanReduceTime()
        {
            return !_hasUsedTimeModification && meetingTimer > MIN_TIME_FOR_REDUCTION;
        }
        #endregion

        #region Validation Helpers
        /// <summary>
        /// 시간 값 유효성 검사
        /// </summary>
        private bool ValidateTimeValue(float time)
        {
            if (float.IsNaN(time) || float.IsInfinity(time) || time <= 0f)
            {
                Debug.LogWarning($"[GameManager] 유효하지 않은 시간 값: {time}");
                return false;
            }
            return true;
        }
        #endregion
    }
}