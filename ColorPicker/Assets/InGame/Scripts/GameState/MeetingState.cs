using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MeetingState : GameState
    {
        GameManager gameManager;

        public MeetingState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            gameManager ??= GameManager.Instance;

            gameManager.SetMeetingTimer();
            gameManager.BroadcastMeetingTimer();

            UIManager.Instance.ShowMeetingUI(true);
        }
        public override void Exit()
        {
            base.Exit();

            UIManager.Instance.ShowMeetingUI(false);
        }

        public override void Update()
        {
            base.Update();

            if (!PhotonNetwork.IsMasterClient) return;  

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // 언더런 방지
            gameManager.meetingTimer = Mathf.Max(0f, gameManager.meetingTimer - dt);

            if (gameManager.meetingTimer <= 0f)
            {
                gameManager.RequestPhaseChange(GameStateType.Playing);
            }
        }
    }
}
