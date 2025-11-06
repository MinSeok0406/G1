using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 게임 전체 상태 및 페이즈 관리 + 미팅/투표 오케스트레이션
    /// </summary>
    public sealed class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        #region Constants
        private const string GAME_PHASE_KEY = "GamePhase";
        #endregion

        #region Properties
        public GameStateMachine StateMachine { get; private set; }
        public PlayerClassAssigner PlayerClassAssigner { get; private set; }
        public GameStartedState GameStartedState { get; private set; }
        public PlayingGameState PlayingGameState { get; private set; }
        public MeetingState MeetingState { get; private set; }
        public ResultState ResultState { get; private set; }

        [HideInInspector] public float meetingTimer;
        #endregion

        #region Managers
        [HideInInspector] public MeetingTimerManager _meetingTimer;
        [HideInInspector] public VotingSystemManager _voting;
        #endregion

        #region Unity
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            _meetingTimer = new MeetingTimerManager(this);
            _voting = new VotingSystemManager();

            InitializeGameStates();
        }

        private void Start()  => StateMachine?.Initialize(GameStartedState);
        private void Update() => StateMachine?.CurrentState?.Update();
        #endregion

        #region Init
        private void InitializeGameStates()
        {
            StateMachine       = new GameStateMachine();
            PlayerClassAssigner = new PlayerClassAssigner();

            GameStartedState = new GameStartedState(StateMachine);
            PlayingGameState = new PlayingGameState(StateMachine);
            MeetingState     = new MeetingState(StateMachine);
            ResultState      = new ResultState(StateMachine);
        }
        #endregion

        #region Photon Prop Changes
        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (!changedProps.TryGetValue(GAME_PHASE_KEY, out object v)) return;

            var newPhase     = (GameStateType)(int)v;
            var currentPhase = HelperUtilities.ToPhase(StateMachine.CurrentState);

            if (newPhase != currentPhase) ChangeStateFromPhase(newPhase);
        }
        #endregion

        #region Phase
        public void SetPhase(GameStateType phase)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[GameManager] SetPhase is master-only.");
                return;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable { { GAME_PHASE_KEY, (int)phase } });
        }

        public void ChangeStateFromPhase(GameStateType phase)
        {
            var newState = HelperUtilities.ToState(phase);
            if (newState == null)
            {
                Debug.LogError($"[GameManager] Unknown phase: {phase}");
                return;
            }
            StateMachine.ChangeState(newState);
        }

        public void RequestPhaseChange(GameStateType requestedPhase)
        {
            if (PhotonNetwork.IsMasterClient) SetPhase(requestedPhase);
            else photonView.RPC(nameof(RPC_RequestPhaseChange), RpcTarget.MasterClient, (int)requestedPhase);
        }

        [PunRPC]
        private void RPC_RequestPhaseChange(int phaseInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var requested = (GameStateType)phaseInt;
            // TODO: 권한/상태 검증 로직 추가 가능
            SetPhase(requested);
        }
        #endregion

        #region Meeting Timer (Public Facade)
        public void SetMeetingTimer(float time = Settings.defaultMeetingTime) =>
            _meetingTimer.SetMeetingTimer(time);

        public void RequestAddMeetingTime(float time = Settings.defaultAddTime) =>
            _meetingTimer.RequestAddTime(time);

        public void RequestReduceMeetingTime(float time = Settings.defaultAddTime) =>
            _meetingTimer.RequestReduceTime(time);

        public void BroadcastMeetingTimer() => _meetingTimer.BroadcastTimer();

        public bool CanModifyTime() => _meetingTimer.CanModifyTime();
        public bool CanReduceTime() => _meetingTimer.CanReduceTime();
        public void ResetMeetingRequests()
        {
            _meetingTimer.ResetRequests();
            _voting.ResetVotes();
        }
        #endregion

        #region Meeting Timer — RPC Facade (외부에서 이 메서드만 쓰세요)
        /// <summary>클라이언트→호스트: 시간 추가 요청</summary>
        public void RpcRequestAddMeetingTime(float time) =>
            photonView.RPC(nameof(RPC_AddMeetingTime), RpcTarget.MasterClient, time);

        /// <summary>클라이언트→호스트: 시간 감소 요청</summary>
        public void RpcRequestReduceMeetingTime(float time) =>
            photonView.RPC(nameof(RPC_ReduceMeetingTime), RpcTarget.MasterClient, time);

        /// <summary>호스트→전체: 타이머 동기화</summary>
        public void RpcBroadcastMeetingTimer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            photonView.RPC(nameof(RPC_SyncMeetingTimer), RpcTarget.All, meetingTimer, PhotonNetwork.Time);
        }

        /// <summary>호스트→전체: 시간조정 사용 상태 동기화</summary>
        public void RpcSyncTimeModificationState(bool used) =>
            photonView.RPC(nameof(RPC_SyncTimeModificationState), RpcTarget.All, used);
        #endregion

        #region Meeting Timer — RPC Impl (내부 위임)
        [PunRPC]
        private void RPC_AddMeetingTime(float time, PhotonMessageInfo info) =>
            _meetingTimer.HandleAddTimeRequest(time, info);

        [PunRPC]
        private void RPC_ReduceMeetingTime(float time, PhotonMessageInfo info) =>
            _meetingTimer.HandleReduceTimeRequest(time, info);

        [PunRPC]
        private void RPC_SyncTimeModificationState(bool hasUsed) =>
            _meetingTimer.SyncTimeModificationState(hasUsed);

        [PunRPC]
        private void RPC_SyncMeetingTimer(float serverRemaining, double sentServerTime) =>
            _meetingTimer.SyncTimer(serverRemaining, sentServerTime);
        #endregion

        #region Voting
        public void RequestVote(int targetActorNumber)
        {
            photonView.RPC(nameof(RPC_SubmitVote), RpcTarget.MasterClient, targetActorNumber);
        }

        [PunRPC]
        private void RPC_SubmitVote(int targetActorNumber, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            bool allVoted = _voting.ProcessVote(
                info.Sender.ActorNumber,
                targetActorNumber,
                PhotonNetwork.CurrentRoom?.PlayerCount ?? 0);

            // 투표 결과를 모든 클라이언트에 브로드캐스트
            BroadcastVoteUpdate(targetActorNumber);

            if (allVoted) _meetingTimer.CapTimerOnAllVoted();
        }

        /// <summary>
        /// [Host Only] 투표 결과를 모든 클라이언트에 동기화
        /// </summary>
        private void BroadcastVoteUpdate(int targetActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 해당 플레이어의 현재 득표수 가져오기
            var voteDict = _voting.GetVoteDictionary();
            int voteCount = voteDict.ContainsKey(targetActorNumber) ? voteDict[targetActorNumber] : 0;

            // 모든 클라이언트에 투표 결과 업데이트 RPC 전송
            photonView.RPC(nameof(RPC_UpdateVoteDisplay), RpcTarget.All, targetActorNumber, voteCount);
        }

        [PunRPC]
        private void RPC_UpdateVoteDisplay(int actorNumber, int voteCount)
        {
            UIManager.Instance?.UpdateVoteDisplay(actorNumber, voteCount);
        }

        public int GetMostVotedPlayer() => _voting.GetMostVotedPlayer();
        public Dictionary<int, int> GetVoteDictionary() => _voting.GetVoteDictionary();
        public int GetVotedPlayerCount() => _voting.GetVotedPlayerCount();
        public int GetTotalPlayerCount() => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        #endregion

        #region Death Body Management
        /// <summary>
        /// [Host Only] 시체 정리를 지연 후 실행 (투표 처형 후 생성된 시체 포함)
        /// </summary>
        public void ClearDeathBodiesDelayed(float delay = 0.5f)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[GameManager] ClearDeathBodiesDelayed is host-only.");
                return;
            }

            StartCoroutine(ClearDeathBodiesCoroutine(delay));
        }

        private System.Collections.IEnumerator ClearDeathBodiesCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            DeathBodyManager.Instance?.ClearAllDeathBodies();
        }
        #endregion
    }
}
