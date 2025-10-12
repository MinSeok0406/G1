using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IDetectiveService
    {
        (bool ok, int colorId) InspectNearest(int requesterRootViewId, int targetViewId, float radius, float tolerance);
    }

    public sealed class DetectiveService : IDetectiveService
    {
        public (bool ok, int colorId) InspectNearest(int requesterRootViewId, int targetViewId, float radius, float tolerance)
        {
            var requester = PhotonView.Find(requesterRootViewId);
            var target = PhotonView.Find(targetViewId);
            if (!requester || !target) return (false, -1);

            if (radius > 0f)
            {
                float max = radius * Mathf.Max(1f, tolerance);
                if ((requester.transform.position - target.transform.position).sqrMagnitude > max * max)
                    return (false, -1);
            }

            if (!GameDataManager.Instance.TryGetUIDByViewID(target.ViewID, out string uid) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(uid, out var priv))
                return (true, -1); // 대상은 있으나 Unknown

            return (true, priv.identityColorId);
        }
    }
}
