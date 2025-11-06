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

            // 마피아끼리는 서로 죽일 수 없음
            if (target.classType == (int)PlayerClassType.mafia)
            {
                Debug.LogWarning("[Kill] Mafia cannot kill other mafia.");
                return false;
            }

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
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[Kill] ApplyKill called on non-host. Ignored.");
                return;
            }

            if (!GameDataManager.Instance.TryGetPrivatePlayerData(targetUid, out var target))
            {
                Debug.LogWarning($"[Kill] Private data not found for target uid={targetUid}");
                return;
            }

            var targetPV = PhotonView.Find(targetViewId);

            if (!targetPV)
            {
                Debug.LogError($"[Kill] Target PhotonView not found. viewId={targetViewId}");
                return;
            }

            int targetActor = targetPV.OwnerActorNr;
            if (targetActor <= 0)
            {
                Debug.LogWarning($"[Kill] Invalid actor numbers. target={targetActor}");
                return;
            }

            CutsceneManager.Instance.Host_PlayForActors("Kill_Common", new[] { targetActor });

            target.classType = (int)PlayerClassType.ghost;
            GameDataManager.Instance.UpdatePrivatePlayerData(target);

            // 플레이어 사망 상태 설정
            GameDataManager.Instance.RequestSetPlayerAliveState(targetUid, false);

            sender.RPC("RPC_ConfirmKillResult", RpcTarget.All, targetViewId);

            _cooldowns.StartCooldownHostOnly(killerViewId, cooldownSec, sender);

            // 마피아 킬러에게 즉시 UI 업데이트 동기화
            SyncUIToKiller(killerViewId);
        }

        public void ApplyKillByColorPicker(int targetActorId, int killerActorId, PhotonView sender, float cooldownSec)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorId, out var target)) return;
            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(killerActorId, out var killer)) return;

            target.classType = (int)PlayerClassType.ghost;
            GameDataManager.Instance.UpdatePrivatePlayerData(target);

            // 플레이어 사망 상태 설정
            GameDataManager.Instance.RequestSetPlayerAliveState(target.googleUID, false);

            killer.identityColorId = target.identityColorId;
            GameDataManager.Instance.UpdatePrivatePlayerData(killer);

            GameDataManager.Instance.TryGetViewIDByUID(target.googleUID, out int targetViewId);
            GameDataManager.Instance.TryGetViewIDByUID(killer.googleUID, out int killerViewId);

            CutsceneManager.Instance.Host_PlayForActors("Kill_ColorPick", new[] { targetActorId });

            sender.RPC("RPC_ConfirmKillResult", RpcTarget.All, targetViewId);
            _cooldowns.StartCooldownHostOnly(killerViewId, cooldownSec, sender);

            // 마피아 킬러에게 즉시 UI 업데이트 동기화
            SyncUIToKiller(killerViewId);
        }

        /// <summary>
        /// 마피아 킬러에게 즉시 UI 업데이트 동기화
        /// </summary>
        private void SyncUIToKiller(int killerViewId)
        {
            var killerPV = PhotonView.Find(killerViewId);
            if (killerPV == null) return;

            var killerPlayer = PhotonNetwork.CurrentRoom.GetPlayer(killerPV.OwnerActorNr);
            if (killerPlayer == null) return;

            // 킬러에게만 UI 프로필 업데이트 RPC 전송
            UIManager.Instance?.photonView.RPC("RPC_SyncProfileToClient", killerPlayer);
        }
    }
}
