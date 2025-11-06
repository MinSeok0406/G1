using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class ExchangeKioskTrigger : InteractiveTriggerBase
    {
        protected override bool CanInteract()
        {
            // 향후 비활성화 상태/쿨다운 등 조건 가능
            return true;
        }

        protected override void HandleInteractLocal()
        {
            // 클라 -> 호스트 요청
            photonView.RPC(nameof(RPC_RequestExchange), RpcTarget.MasterClient);
        }

        [PunRPC]
        private void RPC_RequestExchange(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int actorId = info.Sender.ActorNumber;

            if (!GameDataManager.Instance.TryGetInGameDataByActorId(actorId, out var pdata) || pdata == null)
            {
                // 입력 검증 실패: 호출자에게만 실패 피드백
                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, false, -1, -1);
                return;
            }

            var rules = GameDataManager.Instance.GetGameRules();
            int cost = Mathf.Max(0, rules.paintCost);

            if (pdata.coinCount >= cost)
            {
                pdata.coinCount -= cost;
                pdata.paintCount = Mathf.Clamp(pdata.paintCount + 1, 0, 999999);

                // 성공 정보 회신(호출자 개인 피드백)
                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, true, pdata.coinCount, pdata.paintCount);
            }
            else
            {
                photonView.RPC(nameof(RPC_ReceiveExchangeResult), info.Sender, false, pdata.coinCount, pdata.paintCount);
            }
        }

        [PunRPC]
        private void RPC_ReceiveExchangeResult(bool success, int coinCount, int paintCount)
        {
            if (UIManager.Instance == null) return;

            if (coinCount >= 0) UIManager.Instance.UpdateCoinInfo(coinCount);
            if (paintCount >= 0) UIManager.Instance.UpdatePaintInfo(paintCount);

            if (success)
                UIManager.Instance.ShowToast("교환 완료!", transform.position + toastOffset);
            else
                UIManager.Instance.ShowToast(coinCount >= 0 ? "코인이 부족합니다." : "교환 처리 실패", transform.position + toastOffset);
        }
    }
}
