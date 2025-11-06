using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 게임 결과 상태
    /// 게임 종료 후 승리/패배 결과를 표시하는 스테이트
    /// </summary>
    public class ResultState : GameState
    {
        public ResultState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            Debug.Log("[ResultState] Entered Result state - Game Over");

            // UI 결과 화면 표시
            if (UIManager.Instance != null)
            {
                // TODO: UIManager에 ShowGameResultUI 메서드 추가 필요
                UIManager.Instance.ShowToastToScreen("게임 종료!");
            }

            // 플레이어 입력 비활성화
            DisablePlayerInput();
        }

        public override void Exit()
        {
            Debug.Log("[ResultState] Exiting Result state");

            // 플레이어 입력 재활성화
            EnablePlayerInput();
        }

        public override void Update()
        {
            // 결과 화면에서는 특별한 업데이트 로직 없음
        }

        /// <summary>
        /// 플레이어 입력 비활성화
        /// </summary>
        private void DisablePlayerInput()
        {
            var myPlayer = PlayerManager.Instance?.GetMyPlayer();
            if (myPlayer != null)
            {
                var playerControl = myPlayer.GetComponent<PlayerControl>();
                if (playerControl != null)
                {
                    playerControl.enabled = false;
                }
            }
        }

        /// <summary>
        /// 플레이어 입력 재활성화
        /// </summary>
        private void EnablePlayerInput()
        {
            var myPlayer = PlayerManager.Instance?.GetMyPlayer();
            if (myPlayer != null)
            {
                var playerControl = myPlayer.GetComponent<PlayerControl>();
                if (playerControl != null)
                {
                    playerControl.enabled = true;
                }
            }
        }
    }
}
