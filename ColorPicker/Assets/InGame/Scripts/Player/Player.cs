using Cainos.PixelArtTopDown_Basic;
using FunkyCode;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    #region RequireComponent
    [RequireComponent(typeof(Idle))]
    [RequireComponent(typeof(IdleEvent))]
    [RequireComponent(typeof(MovementByVelocity))]
    [RequireComponent(typeof(MovementByVelocityEvent))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    #endregion
    [DisallowMultipleComponent]
    public class Player : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [HideInInspector] public IdleEvent idleEvent;
        [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public PlayerControl playerControl;
        [HideInInspector] public PlayerDataChangedEvent playerDataChangedEvent;
        [HideInInspector] public DeathEvent deathEvent;

        #region about player class
        private CitizenAbility citizenAbility;
        private MafiaAbility mafiaAbility;
        #endregion 

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerControl = GetComponent<PlayerControl>();
            playerDataChangedEvent = GetComponent<PlayerDataChangedEvent>();
            deathEvent = GetComponent<DeathEvent>();

            citizenAbility = GetComponentInChildren<CitizenAbility>();
            mafiaAbility = GetComponentInChildren<MafiaAbility>();

            DontDestroyOnLoad(gameObject);
        }

        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
        {
        }

        public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            if (targetView != photonView) return;
            if (!photonView.IsMine) return;

            PlayerManager.Instance.SetMyPlayer(this);

            InitializedPlayer();
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player senderOfFailedRequest)
        {
        }

        public void InitializedPlayer()
        {
            Camera.main.transform.SetParent(transform);
        }

        // 수정 예정
        public void AttachClassComponent(PlayerClassType classType)
        {
            var ability = GetComponent<IClassAbility>() as Component;
            if (ability != null) { Destroy(ability); }

            string abilityName = $"{classType}Ability";
            Type abilityType = Type.GetType($"ColorPicker.InGame.{abilityName}");

            if (abilityType != null)
            {
                gameObject.AddComponent(abilityType);
                Debug.Log($"[Player] {abilityName} 부착 완료");
            }
            else
            {
                Debug.LogWarning($"[Player] {abilityName} 타입을 찾지 못했습니다.");
            }
        }

    }
}
