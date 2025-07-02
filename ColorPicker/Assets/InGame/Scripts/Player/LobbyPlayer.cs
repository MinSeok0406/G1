using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class LobbyPlayer : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [HideInInspector] public IdleEvent idleEvent;
        [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public PlayerControl playerControl;
        [HideInInspector] public PlayerDataChangedEvent playerDataChangedEvent;

        #region about player class
        private CitizenAbility citizenAbility;
        private MafiaAbility mafiaAbility;
        #endregion 

        private void Awake()
        {
            animator = GetComponent<Animator>();
            idleEvent = GetComponent<IdleEvent>();
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerControl = GetComponent<PlayerControl>();
            playerDataChangedEvent = GetComponent<PlayerDataChangedEvent>();

            citizenAbility = GetComponentInChildren<CitizenAbility>();
            mafiaAbility = GetComponentInChildren<MafiaAbility>();
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer) { }

        public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player senderOfFailedRequest) { }

        public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            if(targetView != photonView) return;

            bool isRealOwner = (photonView.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber);

            if (!isRealOwner) return;

            if (PhotonNetwork.IsMasterClient) return;
            InitializedPlayer();
        }

        public void InitializedPlayer()
        {
            GameObject cameraObj;

            if (Camera.main == null)
            {
                cameraObj = Instantiate(GameResources.Instance.mainCameraPrefab);
            }
            else
            {
                cameraObj = Camera.main.gameObject;
            }

            cameraObj.transform.SetParent(transform, false);

            PlayerManager.Instance.SetMyLobbyPlayer(this);
        }
    }
}
