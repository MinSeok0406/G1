using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    
    public sealed class PaintService : IPaintService
    {
        public PaintApplyResult TryApplyPaint(int viewId, int requesterActorNum,
            out int appliedColorType, out int remainingPaint, out Vector3 toastWorldPos)
        {
            appliedColorType = -1;
            remainingPaint   = -1;
            toastWorldPos    = Vector3.zero;

            // 대상 뷰/트리거 조회
            PhotonView targetView = PhotonView.Find(viewId);
            var trigger = targetView ? targetView.GetComponent<PaintApplyTrigger>() : null;
            if (trigger) toastWorldPos = trigger.transform.position + Vector3.up * 0.5f;

            // 기본 검증
            bool hasPriv = GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(requesterActorNum, out var playerPriv);
            bool hasIngm = GameDataManager.Instance.TryGetInGameDataByActorId(requesterActorNum, out var playerIngm);
            if (!hasPriv || !hasIngm || !targetView || !trigger)
                return PaintApplyResult.Error;

            // 이미 도색된 오브젝트인지
            int currentColor = GameDataManager.Instance.GetPaintObjectColor(viewId);
            if (currentColor != -1)
                return PaintApplyResult.AlreadyPainted;

            // (선택) 거리 검증 등을 여기 추가 가능
            // if (!ServerDistanceValidator.IsWithinRange(requesterActorNum, targetView.transform.position, maxDist)) return PaintApplyResult.Error;

            // 보유 페인트 확인
            if (playerIngm.paintCount <= 0)
                return PaintApplyResult.NotEnoughPaint;

            // 적용
            playerIngm.paintCount--;
            remainingPaint   = playerIngm.paintCount;
            appliedColorType = playerPriv.identityColorId;

            GameDataManager.Instance.SetPaintObjectColor(viewId, appliedColorType);

            return PaintApplyResult.Success;
        }
    }
}