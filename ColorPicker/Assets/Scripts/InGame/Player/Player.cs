using ColorPicker.Server;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class Player : NetworkRoomPlayer
    {
        [HideInInspector] public IdleEvent idleEvent;
        [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public PlayerChangeColor playerColor;
        [HideInInspector] public ColorChangedEvent colorChangedEvent;
        [HideInInspector] public Animator animator;

        [SyncVar(hook = nameof(SetPlayerColor_Hook))]
        [HideInInspector] public ColorType playerColorType;
        [HideInInspector] public PlayerClassType playerClassType;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerColor = GetComponent<PlayerChangeColor>();
            colorChangedEvent = GetComponent<ColorChangedEvent>();
            animator = GetComponent<Animator>();
        }

        public override void Start()
        {
            base.Start();

            if (!isServer) return;

            GameSystem.Instance.AddPlayer(netId ,this);
        }

        public void SetPlayerColor_Hook(ColorType oldColor, ColorType newColor)
        {
            playerColor.SetPlayerColor(oldColor, newColor);
        }

    }
}

