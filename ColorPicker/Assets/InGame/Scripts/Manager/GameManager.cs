using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        public GameStateMachine StateMachine { get; private set; }
        public PlayerClassAssigner playerClassAssigner { get; private set; }

        private readonly Dictionary<int, double> _addTimerCooldownUntil = new(); // ActorNumber -> PhotonTime
        private const double AddCooldownSec = 1.0;

        [HideInInspector] public float meetingTimer;

        private readonly HashSet<int> meetingAddRequesters = new HashSet<int>();
        private readonly HashSet<int> meetingVoters = new HashSet<int>();
        private readonly Dictionary<int, int> voteCounts = new Dictionary<int, int>();
        private bool allVotedCapped = false;

        private const float AllVotedCapSeconds = 3f;

        #region Property

        public GameStartedState GameStartedState { get; private set; }
        public PlayingGameState PlayingGameState { get; private set; }
        public MeetingState MeetingState { get; private set; }

        #endregion

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            // ���� �ʱ�ȭ
            StateMachine = new GameStateMachine();
            playerClassAssigner = new PlayerClassAssigner();

            GameStartedState = new GameStartedState(StateMachine);
            PlayingGameState = new PlayingGameState(StateMachine);
            MeetingState = new MeetingState(StateMachine);

        }

        private void Start()
        {
            StateMachine.Initialize(GameStartedState);
        }

        private void Update()
        {
            StateMachine.CurrentState?.Update();

            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (var data in GameDataManager.Instance.GetAllPrivatePlayerData())
                {
                    Debug.Log($"{(ColorType)data.identityColorId} and {(PlayerClassType)data.classType}");
                }
            }
        }

        /// <summary>
        /// Room Custom Properties�� ����Ǿ��� �� ȣ��� (��� Ŭ���̾�Ʈ ���� ����ȭ)
        /// </summary>
        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (changedProps.TryGetValue("GamePhase", out object value))
            {
                var newPhase = (GameStateType)(int)value;
                var currentPhase = HelperUtilities.ToPhase(StateMachine.CurrentState);

                if (newPhase != currentPhase)
                {
                    ChangeStateFromPhase(newPhase);
                }
                else
                {
                    Debug.Log($"[GameManager] �̹� ���� ���¿� ������ Phase({newPhase})�Դϴ�. ���� ���� ����.");
                }
            }
        }

        /// <summary>
        /// [ȣ��Ʈ ����] Room Custom Properties�� ���� ���� ���¸� ����Ͽ� ��� Ŭ���̾�Ʈ���� �����Ѵ�.
        /// </summary>
        public void SetPhase(GameStateType phase)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "GamePhase", (int)phase } });
        }

        /// <summary>
        /// [Ŭ���̾�Ʈ ����] Room Custom Properties�� ���޹��� ���� ���� ������� ���� ���� ���¸� ��ȯ�Ѵ�.
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
                Debug.LogError($"[GameManager] ���� ���� ����: {phase}");
            }
        }

        /// <summary>
        /// [RPC][ȣ��Ʈ ����] �Ϲ� Ŭ���̾�Ʈ�� �����Ϳ��� ���� ������ ��û�� �� ȣ�� (ex: ȸ�� ȣ��)
        /// </summary>
        [PunRPC]
        private void RPC_RequestPhaseChange(int phaseInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameStateType requestedPhase = (GameStateType)phaseInt;

            Debug.Log($"[GameManager] {info.Sender.NickName} ��û���� ���� ���� �õ�: {requestedPhase}");

            SetPhase(requestedPhase);
        }

        /// <summary>
        /// Ŭ���̾�Ʈ�� �����Ϳ��� ���� ������ ��û
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

        #region meeting

        public void SetMeetingTimer(float time = Settings.defaultMeetingTime)
        {
            if (!PhotonNetwork.IsMasterClient || time <= 0) return;

            meetingTimer = time;
        }

        public void RequestAddMeetingTime(float time = Settings.defaultAddTime)
        {
            photonView.RPC(nameof(RPC_AddMeetingTime), RpcTarget.MasterClient, time);
        }

        [PunRPC]
        private void RPC_AddMeetingTime(float time, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (float.IsNaN(time) || float.IsInfinity(time) || time <= 0f) return;

            int senderActorNum = info.Sender.ActorNumber;

            // 이미 요청한 사람은 무시
            if (meetingAddRequesters.Contains(senderActorNum))
            {
                Debug.LogWarning($"[MeetingTimer] Actor {senderActorNum} already requested time once.");
                return;
            }

            meetingAddRequesters.Add(senderActorNum);

            meetingTimer += time;
            BroadcastMeetingTimer();
        }

        /// <summary>
        /// 새로운 미팅 상태 진입 시 호출 → 중복 요청 기록 초기화
        /// </summary>
        public void ResetMeetingRequests()
        {
            meetingAddRequesters.Clear();
        }

        public void BroadcastMeetingTimer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            // 서버 남은 시간 + 보낸 시점의 서버타임(지연 보정용)
            photonView.RPC(nameof(RPC_SyncMeetingTimer), RpcTarget.All, meetingTimer, PhotonNetwork.Time);
        }

        [PunRPC]
        private void RPC_SyncMeetingTimer(float serverRemaining, double sentServerTime)
        {
            // 네트워크 지연만큼 보정
            double now = PhotonNetwork.Time;
            float adjusted = Mathf.Max(0f, serverRemaining - (float)(now - sentServerTime));

            // 각 클라 로컬 UI 시작/갱신
            UIManager.Instance.StartMeetingTimerUI(adjusted);
        }

        public void RequestVote(int targetNum)
        {
            photonView.RPC(nameof(RPC_SubmitVote), RpcTarget.MasterClient, targetNum);
        }

        [PunRPC]
        private void RPC_SubmitVote(int targetNum, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int sender = info.Sender.ActorNumber;

            // 이미 투표한 사람은 무시
            if (!meetingVoters.Add(sender))
                return;

            // 표 집계
            if (voteCounts.ContainsKey(targetNum))
                voteCounts[targetNum]++;
            else
                voteCounts[targetNum] = 1;

            Debug.Log($"[Vote] Actor {sender} voted for {targetNum}. Current count = {voteCounts[targetNum]}");

            CheckAllVotedAndCap();
        }

        /// <summary>
        /// 전원 투표 시 3초 캡 + 집계 결과 확인
        /// </summary>
        private void CheckAllVotedAndCap()
        {
            if (allVotedCapped) return;

            int required = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

            if (required > 0 && meetingVoters.Count >= required)
            {
                allVotedCapped = true;

                if (meetingTimer > AllVotedCapSeconds)
                    meetingTimer = AllVotedCapSeconds;

                BroadcastMeetingTimer();

                // ===== 여기서 결과 집계 출력 예시 =====
                foreach (var kvp in voteCounts)
                    Debug.Log($"Target {kvp.Key} received {kvp.Value} votes");

                // 최다 득표자 구하기
                int maxTarget = -1;
                int maxVotes = -1;
                foreach (var kvp in voteCounts)
                {
                    if (kvp.Value > maxVotes)
                    {
                        maxVotes = kvp.Value;
                        maxTarget = kvp.Key;
                    }
                }

                Debug.Log($"[Vote Result] Player {maxTarget} has the most votes ({maxVotes})");

            }
        }

        public Dictionary<int,int> GetVoteDictionary()
        {
            return voteCounts;
        }

        #endregion
    }
}
