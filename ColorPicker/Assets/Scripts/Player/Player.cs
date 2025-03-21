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
        [HideInInspector] public PlayerData playerData;

        //[HideInInspector] 
        public PlayerClassType playerClassType = PlayerClassType.citizen;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();            
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            InitializedPlayerData();
        }

        private void Start()
        {
            S_RegisterPlayer();

            DontDestroyOnLoad(gameObject);
        }

        private void InitializedPlayerData()
        {
            playerData = new PlayerData(photonView.Owner.ActorNumber, null, this); 
        }

        private void OnDestroy()
        {
            S_UnRegisterPlayer();
        }

        private void S_RegisterPlayer()
        {
            int playerId = photonView.Owner.ActorNumber;

            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.Instance.S_RegisterPlayer(playerId, playerData);
            }
            else
            {
                NetworkManager.Instance.C_RegisterPlayer(playerId, playerData);
            }
        }

        private void S_UnRegisterPlayer()
        {
            int playerId = photonView.Owner.ActorNumber;

            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.Instance.S_UnRegisterPlayer(playerId);
            }
            else
            {
                NetworkManager.Instance.C_UnRegisterPlayer(playerId);
            }
        }

    }
}
