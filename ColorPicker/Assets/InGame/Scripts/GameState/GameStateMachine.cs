using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColorPicker.InGame
{
    public class GameStateMachine : MonoBehaviourPun
    {
        public GameState CurrentState { get; private set; }

        /// <summary>
        /// 게임의 상태를 초기 설정하는 함수  
        /// </summary>
        public void Initialize(GameState startState)
        {
            CurrentState = startState;
            CurrentState.Enter();
        }

        /// <summary>
        /// 현재 게임 상태를 다른 상태로 전환하는 함수  
        /// 이전 상태를 종료하고 새 상태로 진입시키는 역할을 함
        /// </summary>
        public void ChangeState(GameState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
