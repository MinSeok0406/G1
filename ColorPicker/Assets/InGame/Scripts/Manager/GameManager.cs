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

        #region Property
        public GameStartedState GameStartedState { get; private set; }
        public PlayingGameState PlayingGameState { get; private set; }
        public MeetingState MeetingState { get; private set; }
        public VotingState VotingState { get; private set; }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            // 상태 초기화
            StateMachine = new GameStateMachine();
            playerClassAssigner = new PlayerClassAssigner();

            GameStartedState = new GameStartedState(StateMachine);
            PlayingGameState = new PlayingGameState(StateMachine);
            MeetingState = new MeetingState(StateMachine);
            VotingState = new VotingState(StateMachine);
        }

        private void Update()
        {
            StateMachine.CurrentState?.Update();
        }

        /// <summary>
        /// Room Custom Properties가 변경되었을 때 호출됨 (모든 클라이언트 상태 동기화)
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
                    Debug.Log($"[GameManager] 이미 현재 상태와 동일한 Phase({newPhase})입니다. 상태 전이 생략.");
                }
            }
        }

        /// <summary>
        /// [호스트 전용] Room Custom Properties에 현재 게임 상태를 기록하여 모든 클라이언트에게 전파한다.
        /// </summary>
        public void SetPhase(GameStateType phase)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable{{ "GamePhase", (int)phase }});
        }

        /// <summary>
        /// [클라이언트 전용] Room Custom Properties로 전달받은 상태 값을 기반으로 현재 로컬 상태를 전환한다.
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
                Debug.LogError($"[GameManager] 상태 전이 실패: {phase}");
            }
        }

        /// <summary>
        /// [RPC][호스트 전용] 일반 클라이언트가 마스터에게 상태 변경을 요청할 때 호출 (ex: 회의 호출)
        /// </summary>
        [PunRPC]
        private void RPC_RequestPhaseChange(int phaseInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameStateType requestedPhase = (GameStateType)phaseInt;

            Debug.Log($"[GameManager] {info.Sender.NickName} 요청으로 상태 변경 시도: {requestedPhase}");

            SetPhase(requestedPhase);
        }

        /// <summary>
        /// 클라이언트가 마스터에게 상태 변경을 요청
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
    }
}
