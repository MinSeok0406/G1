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
    public class Player : MonoBehaviourPunCallbacks
    {
        [HideInInspector] public IdleEvent idleEvent;
        [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public PlayerControl playerControl;
        [HideInInspector] public PlayerDataChangedEvent playerDataChangedEvent;
        [HideInInspector] public DeathEvent deathEvent;


        [HideInInspector] public PlayerClassType playerClassType = PlayerClassType.citizen;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();            
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerControl = GetComponent<PlayerControl>();
            playerDataChangedEvent = GetComponent<PlayerDataChangedEvent>();
            deathEvent = GetComponent<DeathEvent>();

        }

        private void Start()
        {
            int playerId = photonView.Owner.ActorNumber;
            NetworkManager.Instance.RegisterPlayer(playerId, this);

            DontDestroyOnLoad(gameObject);

            if (!PhotonNetwork.IsMasterClient) return;
            PlayerData playerData = new PlayerData(playerId, "", (int)ColorType.White, (int)PlayerClassType.citizen);
            NetworkManager.Instance.AddOrUpdatePlayerData(playerData);

        }

        private void OnDestroy()
        {
            int playerId = photonView.Owner.ActorNumber;
            NetworkManager.Instance.UnregisterPlayerObject(playerId);
        }

    }
}
