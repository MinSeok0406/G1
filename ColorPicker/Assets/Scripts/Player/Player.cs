using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
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

        //[HideInInspector] 
        public PlayerClassType playerClassType = PlayerClassType.citizen;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();            
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            S_RegisterPlayer();

            DontDestroyOnLoad(gameObject);
        }

        public override void OnLeftRoom()
        {
            S_UnRegisterPlayer();

            base.OnLeftRoom();
        }

        private void S_RegisterPlayer()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int playerId = photonView.Owner.ActorNumber;
                NetworkManager.Instance.S_RegisterPlayer(playerId, this);
            }
        }

        private void S_UnRegisterPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int playerId = photonView.Owner.ActorNumber;

            NetworkManager.Instance.S_UnRegisterPlayer(playerId);
        }
    }
}
