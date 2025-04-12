using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MafiaAbility : CitizenAbility
    {
        private KillEvent killEvent;
        private Player currentPlayer;

        private void Awake()
        {
            killEvent = gameObject.AddComponent<KillEvent>();

            IniatializedKillAblity();
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
            int playerId = killEventArgs.player.photonView.Owner.ActorNumber;

            GameManager.Instance.C_SendPlayerState(playerId);
        }

        protected override void Start()
        {
            base.Start();

            UIManager.Instance.SetMafiaUI(true);
        }

        private void IniatializedKillAblity()
        {
            UIManager.Instance.GetKillButton().onClick.AddListener(PlayerKill);
        }


        private void PlayerKill()
        {
            if(currentPlayer != null)
            {
                killEvent.CallKillEvent(currentPlayer, 100f);
                transform.position = currentPlayer.transform.position;
            }
            // 플레이어 시체 생성;
            // 해당플레이어 포스트 프로세싱 설정
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {

            currentPlayer = collision.GetComponent<Player>();
            if (currentPlayer != null)
            {
                UIManager.Instance.GetKillButton().interactable = true;
            }

        }

        private void OnTriggerExit2D(Collider2D collision)
        {
                currentPlayer = null;
                UIManager.Instance.GetKillButton().interactable = false;
        }

    }
}
