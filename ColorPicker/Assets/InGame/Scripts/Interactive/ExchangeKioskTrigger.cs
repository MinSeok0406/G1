using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class ExchangeKioskTrigger : MonoBehaviourPun, IInteractive
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Vector3 offset;
        private OutlineMarker outlineMarker;

        private void Awake()
        {
            outlineMarker = GetComponentInChildren<OutlineMarker>();
        }

        public Vector3 GetPosition() => transform.position;

        public void OnInteract()
        {
            photonView.RPC(nameof(RPC_RequestExchange), RpcTarget.MasterClient);
        }

        [PunRPC]
        public void RPC_RequestExchange(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int actorId = info.Sender.ActorNumber;

            if (!GameDataManager.Instance.TryGetInGameDataByActorId(actorId, out var playerData) || playerData == null)
            {
                Debug.LogWarning($"[ExchangeKioskTrigger] Player not found for Actor {actorId}");
                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, false, -1, -1);
                return;
            }

            var rules = GameDataManager.Instance.GetGameRules();
            int cost = rules.paintCost;

            if (playerData.coinCount >= cost)
            {
                playerData.coinCount -= cost;
                playerData.paintCount++;

                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, true, playerData.coinCount, playerData.paintCount);
            }
            else
            {
                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, false, playerData.coinCount, playerData.paintCount);
            }
        }

        [PunRPC]
        public void RPC_ReceiveExchangeResult(bool success, int coinCount, int paintCount)
        {
            if (success)
            {
                UIManager.Instance.UpdateCoinInfo(coinCount);
                UIManager.Instance.UpdatePaintInfo(paintCount);
                UIManager.Instance.ShowToast("교환 완료!", transform.position + offset);
            }
            else
            {
                if (coinCount >= 0 && paintCount >= 0)
                {
                    UIManager.Instance.UpdateCoinInfo(coinCount);
                    UIManager.Instance.UpdatePaintInfo(paintCount);
                    UIManager.Instance.ShowToast("코인이 부족합니다.", transform.position + offset);
                }
                else
                {
                    UIManager.Instance.ShowToast("교환 처리 실패", transform.position + offset);
                }
            }
        }

        public void ToggleHighlight(bool active)
        {
            if (outlineMarker == null) return;

            if (active)
            {
                if (outlineMaterial != null)
                    outlineMarker.Apply(outlineMaterial);
            }
            else
            {
                outlineMarker.Clear();
            }
        }

         private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return;

            playerControl.interactionDetector.AddInteractable(this);
            ToggleHighlight(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return;

            playerControl.interactionDetector.RemoveInteractable(this);
            ToggleHighlight(false);
        }
    }
}
