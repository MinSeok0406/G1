using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class MafiaAbility : CitizenAbility
    {
        private KillEvent killEvent;
        private Player currentPlayer;

        private void Awake()
        {
            killEvent = gameObject.AddComponent<KillEvent>();

 
        }

        protected override void Start()
        {
            base.Start();

        }



        private void OnEnable()
        {
            killEvent.OnKill += KillEvent_OnKill;
        }

        private void OnDisable()
        {
            killEvent.OnKill -= KillEvent_OnKill;
        }

        private void KillEvent_OnKill(KillEvent killEvent, KillEventArgs killEventArgs)
        {
            AbilityManager.Instance.TryRequestKill(killEventArgs.playerId, this.photonView.ViewID);
        }


        private void OnKillButtonPressed()
        {
            if (currentPlayer == null)
            {
                Debug.LogWarning("현재 타겟 플레이어가 설정되지 않았습니다.");
                return;
            }

            int targetPlayerId = currentPlayer.photonView.Owner.ActorNumber;

            killEvent.CallKillEvent(targetPlayerId);

        }

        public Player GetCurrentPlayer()
        {
            return currentPlayer;
        }

    }
}
