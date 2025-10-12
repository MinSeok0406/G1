using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public sealed class AbilityManager : SingletonNetworkBehaviour<AbilityManager>
    {
        [Header("Config")]
        [SerializeField, Min(0f)] private float killCooldownSeconds = Settings.killCooldownSeconds;
        [SerializeField, Min(0f)] private float colorPickWrongCooldownSeconds = Settings.killCooldownSeconds;
        [SerializeField, Range(1f, 2f)] private float detectiveInspectTolerance = 1.2f; // ★ 외부화

        // Services
        private ICooldownService cooldowns;
        private IKillService kills;
        private IDetectiveService detective;
        private IColorPickService colorPick;

        [HideInInspector] public AbilityBinder abilityBinder => GetComponent<AbilityBinder>();
        private DetectiveAbility detectiveAbility => GetComponentInChildren<DetectiveAbility>();

        protected override void Awake()
        {
            base.Awake();
            cooldowns = new CooldownService();
            kills     = new KillService(cooldowns);
            detective = new DetectiveService();
            colorPick = new ColorPickService();
        }

        // ===== Cooldown Facade =====
        public bool IsOnCooldown(int viewID) => cooldowns.IsOnCooldown(viewID);
        public float GetRemainingCooldown(int viewID) => cooldowns.GetRemaining(viewID);

        public void StartCooldown(int viewID, float duration)
        {
            cooldowns.StartCooldownHostOnly(viewID, duration, photonView);
        }

        [PunRPC]
        private void RPC_SyncCooldown(int viewID, double endTime)
        {
            // ★ 클라 로컬 캐시 먼저
            cooldowns.SetEndTimeFromServer(viewID, endTime);
            UIManager.Instance?.UpdateAbilityCooldownUI(viewID, cooldowns.GetRemaining(viewID));
        }

        // ===== Kill Flow (Mafia) =====
        public void TryRequestKill(int targetViewID, int killerViewID, float killRadius)
        {
            photonView.RPC(nameof(RPC_RequestKill), RpcTarget.MasterClient, targetViewID, killerViewID, killRadius);
        }

        [PunRPC]
        private void RPC_RequestKill(int targetViewID, int killerViewID, float killRadius, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // ★ 소유권 검증
            var killerView = PhotonView.Find(killerViewID);
            if (!killerView || killerView.OwnerActorNr != info.Sender?.ActorNumber)
            {
                Debug.LogWarning("[Ability] KillerView ownership mismatch.");
                return;
            }

            if (IsOnCooldown(killerViewID))
            {
                photonView.RPC(nameof(RPC_KillRejectedCooldown), info.Sender, GetRemainingCooldown(killerViewID));
                return;
            }

            if (!GameDataManager.Instance.TryGetUIDByViewID(targetViewID, out string targetUID) ||
                !GameDataManager.Instance.TryGetUIDByViewID(killerViewID, out string killerUID))
            {
                Debug.LogWarning("[Ability] UID mapping failed.");
                return;
            }

            if (!kills.ValidateKill(targetUID, killerUID, targetViewID, killerViewID, killRadius))
                return;

            kills.ApplyKill(killerUID, targetUID, targetViewID, killerViewID, killCooldownSeconds, photonView);
        }

        [PunRPC]
        private void RPC_KillRejectedCooldown(float remaining)
        {
            UIManager.Instance?.UpdateAbilityCooldownUI(PhotonNetwork.LocalPlayer.ActorNumber, remaining);
        }

        [PunRPC]
        private void RPC_ConfirmKillResult(int targetViewID)
        {
            var view = PhotonView.Find(targetViewID);
            if (!view) return;

            var player = view.GetComponent<Player>();
            if (player)
            {
                player.DeathEvent?.CallDeathEvent();
                Debug.Log($"[Ability] {player.name} has been killed.");
            }
        }

        // ===== Detective =====
        public void RequestDetectiveInspectNearest(int requesterRootViewID, int targetViewID, float radius)
        {
            photonView.RPC(nameof(RPC_RequestDetectiveInspectNearest), RpcTarget.MasterClient, requesterRootViewID, targetViewID, radius);
        }

        [PunRPC]
        private void RPC_RequestDetectiveInspectNearest(int requesterRootViewID, int targetViewID, float radius, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var (ok, colorId) = detective.InspectNearest(requesterRootViewID, targetViewID, radius, detectiveInspectTolerance);
            if (!ok) { photonView.RPC(nameof(RPC_DetectiveInspectResult), info.Sender, -1, -1); return; }

            photonView.RPC(nameof(RPC_DetectiveInspectResult), info.Sender, targetViewID, colorId);
        }

        [PunRPC]
        private void RPC_DetectiveInspectResult(int targetViewID, int colorId)
        {
            detectiveAbility?.HandleInspectResult(targetViewID, colorId);
        }

        // ===== ColorPicker =====
        public void TryRequestColorPick()
        {
            var card = UIManager.Instance?.GetPlayerCard();
            if (!card) { Debug.Log("Not found PlayerCard Data"); return; }

            int targetActorNum = card.GetActorNum();
            int deductionColor = card.GetColor();
            photonView.RPC(nameof(RPC_TryRequestColorPick), RpcTarget.MasterClient, targetActorNum, deductionColor);
        }

        [PunRPC]
        private void RPC_TryRequestColorPick(int targetActorNum, int deductionColor, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorNum, out var _))
            {
                Debug.Log($"not found {targetActorNum} data");
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, (int)ColorPickResult.Error);
                return;
            }

            var judge = colorPick.ValidateAndJudge(targetActorNum, info.Sender.ActorNumber, deductionColor, cooldowns, colorPickWrongCooldownSeconds, photonView);

            if (judge == ColorPickResult.Success)
            {
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, (int)ColorPickResult.Success);
                kills.ApplyKillByColorPicker(targetActorNum, info.Sender.ActorNumber, photonView, 3f);
            }
            else if (judge == ColorPickResult.Fail)
            {
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, (int)ColorPickResult.Fail);
            }
            else
            {
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, (int)ColorPickResult.Error);
            }
        }

        [PunRPC]
        private void RPC_ClientOnColorPickResult(int resultCode)
        {
            var result = (ColorPickResult)resultCode;
            if (result == ColorPickResult.Error) { UIManager.Instance?.ShowToastToScreen(AbilityMessages.ColorPickError); return; }
            if (result == ColorPickResult.Fail)  { UIManager.Instance?.ShowToastToScreen(AbilityMessages.ColorPickFail);  return; }
            if (result == ColorPickResult.Success){ UIManager.Instance?.ShowToastToScreen(AbilityMessages.ColorPickOk);    return; }
        }

        // ===== 재접속/정리 =====
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            cooldowns.RemoveForPlayerLeftRoom(otherPlayer.ActorNumber);
        }

        public void RequestMyCooldownSync(int myViewID)
        {
            photonView.RPC(nameof(RPC_RequestCooldownSync), RpcTarget.MasterClient, myViewID);
        }

        [PunRPC]
        private void RPC_RequestCooldownSync(int viewID, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var pv = PhotonView.Find(viewID);
            if (pv == null || pv.OwnerActorNr != info.Sender?.ActorNumber) return;

            if (!cooldowns.TryGetEndTime(viewID, out double end))
            {
                // 쿨다운 없음 → 0으로 동기화
                photonView.RPC(nameof(RPC_SyncCooldown), info.Sender, viewID, PhotonNetwork.Time);
                return;
            }

            photonView.RPC(nameof(RPC_SyncCooldown), info.Sender, viewID, end);
        }

        // ===== Mafia Layer Sync =====
        public void RequestMafiaLayerSync(int requesterActorId)
        {
            photonView.RPC(nameof(RPC_RequestMafiaLayerSync), RpcTarget.MasterClient, requesterActorId);
        }

        [PunRPC]
        private void RPC_RequestMafiaLayerSync(int requesterActorId, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var mafiaList = GameDataManager.Instance.GetAllPrivatePlayerData()
                .FindAll(p => p.classType == (int)PlayerClassType.mafia);

            foreach (var pdata in mafiaList)
            {
                if (GameDataManager.Instance.TryGetViewIDByUID(pdata.googleUID, out int viewId) && viewId > 0)
                    photonView.RPC(nameof(ApplyMafiaLayer), PhotonNetwork.CurrentRoom.GetPlayer(requesterActorId), viewId);
            }
        }

        [PunRPC]
        private void ApplyMafiaLayer(int targetViewId)
        {
            var targetView = PhotonView.Find(targetViewId);
            if (!targetView || targetView.IsMine) return;

            int mafiaLayer = LayerMask.NameToLayer("Mafia");
            if (mafiaLayer < 0)
            {
                Debug.LogError("[MafiaAbility] 'Mafia' 레이어가 프로젝트에 정의되어 있지 않습니다.");
                return;
            }

            SetLayerRecursively(targetView.gameObject, mafiaLayer);
            Debug.Log($"[MafiaAbility] Mafia Layer 적용 완료: {targetView.gameObject.name}");
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (!root) return;
            if (root.layer != layer) root.layer = layer;
            foreach (Transform child in root.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}
