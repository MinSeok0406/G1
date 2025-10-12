using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IKillService
    {
        bool ValidateKill(string targetUid, string killerUid, int targetViewId, int killerViewId, float radius);
        void ApplyKill(string killerUid, string targetUid, int targetViewId, int killerViewId, float cooldownSec, PhotonView sender);
        void ApplyKillByColorPicker(int targetActorId, int killerActorId, PhotonView sender, float cooldownSec);
    }

    public sealed class KillService : IKillService
    {
        private readonly ICooldownService _cooldowns;
        public KillService(ICooldownService cooldowns) => _cooldowns = cooldowns;

        public bool ValidateKill(string targetUid, string killerUid, int targetViewId, int killerViewId, float radius)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(killerUid, out var killer) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(targetUid, out var target))
                return false;

            if (killer.classType != (int)PlayerClassType.mafia) return false;
            if (target.classType == (int)PlayerClassType.ghost) return false;

            if (radius > 0f)
            {
                var kv = PhotonView.Find(killerViewId);
                var tv = PhotonView.Find(targetViewId);
                if (!kv || !tv) return false;
                if ((kv.transform.position - tv.transform.position).sqrMagnitude > radius * radius)
                    return false;
            }
            return true;
        }

        public void ApplyKill(string killerUid, string targetUid, int targetViewId, int killerViewId, float cooldownSec, PhotonView sender)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(targetUid, out var target)) return;

            target.classType = (int)PlayerClassType.ghost;
            GameDataManager.Instance.UpdatePrivatePlayerData(target);

            sender.RPC("RPC_ConfirmKillResult", RpcTarget.All, targetViewId);
            _cooldowns.StartCooldownHostOnly(killerViewId, cooldownSec, sender);
        }

        public void ApplyKillByColorPicker(int targetActorId, int killerActorId, PhotonView sender, float cooldownSec)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorId, out var target)) return;
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(killerActorId, out var killer)) return;

            target.classType = (int)PlayerClassType.ghost;
            GameDataManager.Instance.UpdatePrivatePlayerData(target);

            killer.identityColorId = target.identityColorId;
            GameDataManager.Instance.UpdatePrivatePlayerData(killer);

            GameDataManager.Instance.TryGetViewIDByUID(target.googleUID, out int targetViewId);
            GameDataManager.Instance.TryGetViewIDByUID(killer.googleUID, out int killerViewId);

            sender.RPC("RPC_ConfirmKillResult", RpcTarget.All, targetViewId);
            _cooldowns.StartCooldownHostOnly(killerViewId, cooldownSec, sender);
        }
    }
}
