using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IPaintService
    {
        /// <summary>
        /// 서버에서만 호출. 도색 판정 및 상태 갱신을 수행한다.
        /// </summary>
        PaintApplyResult TryApplyPaint(int viewId, int requesterActorNum,
            out int appliedColorType, // 성공 시 적용된 colorType, 실패 시 -1
            out int remainingPaint, // 요청자의 남은 페인트(미알 수면 -1)
            out Vector3 toastWorldPos); // 토스트 기준 위치(없으면 Vector3.zero)
    }
}