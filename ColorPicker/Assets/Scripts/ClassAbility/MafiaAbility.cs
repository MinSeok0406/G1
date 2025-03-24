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

            UIManager.Instance.SetMafiaUI();
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
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            currentPlayer = collision.GetComponent<Player>();
            UIManager.Instance.GetKillButton().interactable = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            currentPlayer = null;
            UIManager.Instance.GetKillButton().interactable = false;
        }

    }
}
