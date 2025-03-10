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
    public class Player : MonoBehaviourPun
    {
        [HideInInspector] public IdleEvent idleEvent;
        [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer spriteRenderer;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();            
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        private void Start()
        {
            if (photonView.IsMine)
            {
                NetworkManager.Instance.RegisterClient(PhotonNetwork.LocalPlayer.ActorNumber);
                NetworkManager.Instance.SetMyPlayer(this);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.Instance.RegisterPlayer(this);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}
