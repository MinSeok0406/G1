using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class ColorObjectManager : SingletonNetworkBehaviour<ColorObjectManager>
    {
        private IPaintService _service;

        private static class Msg
        {
            public const string ErrInvalidRequest = "요청 처리 실패";
            public const string ErrInvalidTarget = "유효하지 않은 대상입니다.";
            public const string ErrAlreadyPainted = "이미 도색된 오브젝트입니다.";
            public const string ErrNotEnoughPaint = "페인트가 부족합니다.";
            public const string OkPainted = "도색 완료!";
        }

        protected override void Awake()
        {
            base.Awake();
            _service = new PaintService(); // 필요하면 DI로 주입 가능
        }

        /// <summary>클라이언트 → 서버: 도색 요청</summary>
        public void RequestPaintApply(int viewID)
        {
            photonView.RPC(nameof(RPC_RequestPaintApply), RpcTarget.MasterClient, viewID);
        }

        /// <summary>서버: 도색 요청 처리</summary>
        [PunRPC]
        public void RPC_RequestPaintApply(int viewID, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var result = _service.TryApplyPaint(
                viewID,
                info.Sender.ActorNumber,
                out int appliedColorType,
                out int remainingPaint,
                out Vector3 toastPos
            );
            
            toastPos += Vector3.up * 1.5f;

            // 요청자 UI/토스트 (케이스별)
            switch (result)
            {
                case PaintApplyResult.Success:
                    if (remainingPaint >= 0)
                        photonView.RPC(nameof(RPC_PaintUIUpdate), info.Sender, remainingPaint);
                    photonView.RPC(nameof(RPC_ClientToast), info.Sender, Msg.OkPainted, toastPos);

                    // 전체에 해당 오브젝트만 브로드캐스트 (네트워크 효율성 ↑)
                    photonView.RPC(nameof(RPC_PaintApply), RpcTarget.All, viewID, appliedColorType);
                    break;

                case PaintApplyResult.AlreadyPainted:
                    photonView.RPC(nameof(RPC_ClientToast), info.Sender, Msg.ErrAlreadyPainted, toastPos);
                    break;

                case PaintApplyResult.NotEnoughPaint:
                    photonView.RPC(nameof(RPC_ClientToast), info.Sender, Msg.ErrNotEnoughPaint, toastPos);
                    break;

                default:
                    // Error: 유효하지 않은 접근/데이터 누락 등
                    photonView.RPC(nameof(RPC_ClientToast), info.Sender, Msg.ErrInvalidRequest, toastPos);
                    break;
            }
        }

        /// <summary>클라: 특정 오브젝트에 색 적용(시각 반영)</summary>
        [PunRPC]
        public void RPC_PaintApply(int viewID, int colorType)
        {
            PhotonView view = PhotonView.Find(viewID);
            if (!view)
            {
                Debug.LogWarning("[ColorObjectManager] invalid view id on PaintApply");
                return;
            }

            var trigger = view.GetComponent<PaintApplyTrigger>();
            if (!trigger)
            {
                Debug.LogWarning("[ColorObjectManager] PaintApplyTrigger missing on target view");
                return;
            }

            trigger.SetPaintColor(colorType);
        }

        /// <summary>클라: 남은 페인트 UI 업데이트</summary>
        [PunRPC]
        public void RPC_PaintUIUpdate(int paintCount)
        {
            UIManager.Instance?.UpdatePaintInfo(paintCount);
        }

        /// <summary>클라: 요청자 전용 토스트 출력</summary>
        [PunRPC]
        private void RPC_ClientToast(string message, Vector3 worldPos)
        {
            UIManager.Instance?.ShowToast(message, worldPos);
        }
    }
}
