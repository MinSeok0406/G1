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

            // ���� �ʱ�ȭ
            StateMachine = new GameStateMachine();
            playerClassAssigner = new PlayerClassAssigner();

            GameStartedState = new GameStartedState(StateMachine);
            PlayingGameState = new PlayingGameState(StateMachine);
            MeetingState = new MeetingState(StateMachine);
            VotingState = new VotingState(StateMachine);

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

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable{{ "GamePhase", (int)phase }});
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
    }
}
