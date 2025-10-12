
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class ColorObjectManager : SingletonNetworkBehaviour<ColorObjectManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public void RequestPaintApply(int viewID)
        {
            photonView.RPC(nameof(RPC_RequestPaintApply), RpcTarget.MasterClient, viewID);
        }

        [PunRPC]
        public void RPC_RequestPaintApply(int viewID, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(info.Sender.ActorNumber, out var playerData);
            GameDataManager.Instance.TryGetInGameDataByActorId(info.Sender.ActorNumber, out var data);

            if (GameDataManager.Instance.GetPaintObjectColor(viewID) != -1) return;

            if (data.paintCount > 0)
            {
                Debug.Log(data.paintCount);

                data.paintCount--;
                GameDataManager.Instance.SetPaintObjectColor(viewID, playerData.identityColorId);
                photonView.RPC(nameof(RPC_PaintUIUpdate), info.Sender, data.paintCount);
            }
            else
            {
                Debug.Log("not eoungh paint");
                return;
            }

            var colorMap = GameDataManager.Instance.GetAllPaintObjectDictionary();

            foreach (var id in colorMap.Keys)
            {
                photonView.RPC(nameof(RPC_PaintApply), RpcTarget.All, id, colorMap[id]);
            }

        }

        [PunRPC]
        public void RPC_PaintApply(int viewID, int colorType)
        {
            PhotonView view = PhotonView.Find(viewID);

            if (!view)
            {
                Debug.Log("[ColorOjbectManager] 유효하지 않은 접근");
                return;
            }

            view.GetComponent<PaintApplyTrigger>().SetPaintColor(colorType);
        }

        [PunRPC]
        public void RPC_PaintUIUpdate(int paintCount)
        {
            UIManager.Instance.UpdatePaintInfo(paintCount);
        }
    }
}