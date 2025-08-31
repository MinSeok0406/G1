using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public sealed class AbilityManager : SingletonNetworkBehaviour<AbilityManager>
    {
        // ★ 서버시간 기반(동기화됨). viewID -> cooldownEnd(초, PhotonNetwork.Time)
        private readonly Dictionary<int, double> cooldownEndTimes = new();

        // ====== Config ======
        [Header("Kill Config")]
        [SerializeField, Min(0f)] private float defaultCooldownSeconds = 20f;   // ★ Settings 참조 유지 가능
        [SerializeField] private float minKillDistance = 1f;                  // 필요 시 0으로
        [SerializeField] private bool validateDistance = false;

        [HideInInspector] public AbilityBinder abilityBinder;

        // 외부 Settings와의 호환(기존 코드가 Settings.defaultCooldown 참조 시)
        private float DefaultCooldownOrSettings =>
            (Settings.defaultCooldown > 0f) ? Settings.defaultCooldown : defaultCooldownSeconds;

        private DetectiveAbility detectiveAbility => GetComponentInChildren<DetectiveAbility>();

        protected override void Awake()
        {
            base.Awake();
            abilityBinder = FindObjectOfType<AbilityBinder>();
        }

        // ====== Cooldown ======

        /// <summary> 현재 플레이어(viewID)가 쿨타임 중인가? (클라/호스트 공용 조회) </summary>
        public bool IsOnCooldown(int viewID)
        {
            return cooldownEndTimes.TryGetValue(viewID, out double end) && PhotonNetwork.Time < end;
        }

        /// <summary>
        /// [호스트 전용] 스킬 사용 시 쿨다운 시작
        /// </summary>
        public void StartCooldown(int viewID, float skillCooldownDuration)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            double duration = Mathf.Max(0f, skillCooldownDuration);
            cooldownEndTimes[viewID] = PhotonNetwork.Time + duration;

            // ★ 클라 동기화(해당 소유자에게만 전송)
            var pv = PhotonView.Find(viewID);
            if (pv != null && pv.Owner != null)
            {
                photonView.RPC(nameof(RPC_SyncCooldown), pv.Owner, viewID, cooldownEndTimes[viewID]);
            }
        }

        /// <summary>
        /// [호스트 전용] 남은 쿨타임(초). 재참여/재연결 동기화용
        /// </summary>
        public float GetRemainingCooldown(int viewID)
        {
            return cooldownEndTimes.TryGetValue(viewID, out double end)
                ? Mathf.Max(0f, (float)(end - PhotonNetwork.Time))
                : 0f;
        }

        [PunRPC]
        private void RPC_SyncCooldown(int viewID, double endTime)
        {
            cooldownEndTimes[viewID] = endTime;
            UIManager.Instance?.UpdateAbilityCooldownUI(viewID, GetRemainingCooldown(viewID));
        }

        // ====== Kill Flow ======

        /// <summary> 클라 → 호스트: 킬 요청 </summary>
        public void TryRequestKill(int targetViewID, int killerViewID)
        {
            photonView.RPC(nameof(RPC_RequestKill), RpcTarget.MasterClient, targetViewID, killerViewID);
        }

        [PunRPC]
        private void RPC_RequestKill(int targetViewID, int killerViewID, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (IsOnCooldown(killerViewID))
            {
                photonView.RPC(nameof(RPC_KillRejectedCooldown), info.Sender, GetRemainingCooldown(killerViewID));
                return;
            }

            // UID 매핑
            if (!GameDataManager.Instance.TryGetUIDByViewID(targetViewID, out string targetUID) ||
                !GameDataManager.Instance.TryGetUIDByViewID(killerViewID, out string killerUID))
            {
                Debug.LogWarning("[Ability] UID mapping failed.");

                return;
            }

            // 유효성 검증
            if (!IsKillValid(targetUID, killerUID, targetViewID, killerViewID))
                return;

            // 적용
            ApplyKill(killerUID, targetUID, targetViewID, killerViewID);
        }

        [PunRPC]
        private void RPC_KillRejectedCooldown(float remaining)
        {
            UIManager.Instance?.UpdateAbilityCooldownUI(PhotonNetwork.LocalPlayer.ActorNumber, remaining);
        }

        private bool IsKillValid(string targetUID, string killerUID, int targetViewID, int killerViewID)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(killerUID, out var killerData) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(targetUID, out var targetData))
                return false;

            // 직업 검증
            if (killerData.classType != (int)PlayerClassType.mafia) return false;
            if (targetData.classType == (int)PlayerClassType.ghost) return false;

            // 거리 검증 옵션
            if (validateDistance && minKillDistance > 0f)
            {
                var kv = PhotonView.Find(killerViewID);
                var tv = PhotonView.Find(targetViewID);
                if (kv == null || tv == null) return false;

                var kp = kv.transform.position;
                var tp = tv.transform.position;
                if (Vector3.SqrMagnitude(kp - tp) > (minKillDistance * minKillDistance))
                    return false;
            }

            return true;
        }

        private void ApplyKill(string killerUID, string targetUID, int targetViewID, int killerViewID)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(targetUID, out var targetData)) return;

            // 상태 전환
            targetData.classType = (int)PlayerClassType.ghost;
            GameDataManager.Instance.UpdatePrivatePlayerData(targetData);

            photonView.RPC(nameof(RPC_ConfirmKillResult), RpcTarget.All, targetViewID);

            StartCooldown(killerViewID, DefaultCooldownOrSettings);
        }

        [PunRPC]
        private void RPC_ConfirmKillResult(int targetViewID)
        {
            var view = PhotonView.Find(targetViewID);
            if (!view) return;

            var player = view.GetComponent<Player>();
            if (player)
            {
                player.deathEvent?.CallDeathEvent();
                Debug.Log($"[Ability] {player.name} has been killed.");
            }

        }

        // ====== 재접속/유지보수 ======

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            var toRemove = ListPool<int>.Get(); // using 제거
            try
            {
                foreach (var key in cooldownEndTimes.Keys)   // 키만 순회해서 수집
                {
                    var pv = PhotonView.Find(key);
                    if (pv == null || pv.Owner == null || pv.OwnerActorNr == otherPlayer.ActorNumber)
                        toRemove.Add(key);
                }

                for (int i = 0; i < toRemove.Count; i++)
                    cooldownEndTimes.Remove(toRemove[i]);
            }
            finally
            {
                ListPool<int>.Release(toRemove); // 리스트 비움(재사용)
            }
        }


        /// <summary> 클라가 자신의 남은 쿨타임 동기화를 요청할 때(재접속/씬 재오픈) </summary>
        public void RequestMyCooldownSync(int myViewID)
        {
            photonView.RPC(nameof(RPC_RequestCooldownSync), RpcTarget.MasterClient, myViewID);
        }

        [PunRPC]
        private void RPC_RequestCooldownSync(int viewID, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 요청자가 해당 view 소유자인지 보수적으로 검증
            var pv = PhotonView.Find(viewID);
            if (pv == null || pv.OwnerActorNr != info.Sender?.ActorNumber) return;

            if (cooldownEndTimes.TryGetValue(viewID, out double end))
                photonView.RPC(nameof(RPC_SyncCooldown), info.Sender, viewID, end);
        }

        // ====== 유틸 풀 ======

        private static class ListPool<T>
        {
            [System.ThreadStatic] private static List<T> _list;
            public static List<T> Get() => _list ??= new List<T>(8);
            public static void Release(List<T> list) => list.Clear();
        }

        // ====== Mafia Ability ======
        public void RequestMafiaLayerSync(int requesterActorId)
        {
            photonView.RPC(nameof(RPC_RequestMafiaLayerSync), RpcTarget.MasterClient, requesterActorId);
        }

        [PunRPC]
        private void RPC_RequestMafiaLayerSync(int requesterActorId, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 마피아 플레이어 목록 조회
            var mafiaList = GameDataManager.Instance.GetAllPrivatePlayerData()
                .FindAll(p => p.classType == (int)PlayerClassType.mafia);

            foreach (var pdata in mafiaList)
            {
                if (GameDataManager.Instance.TryGetViewIDByUID(pdata.googleUID, out int viewId) && viewId > 0)
                {
                    // 해당 클라이언트에게만 레이어 변경 요청
                    photonView.RPC(nameof(ApplyMafiaLayer), PhotonNetwork.CurrentRoom.GetPlayer(requesterActorId), viewId);
                }
            }
        }

        [PunRPC]
        private void ApplyMafiaLayer(int targetViewId)
        {
            PhotonView targetView = PhotonView.Find(targetViewId);
            if (targetView.IsMine) return;

            if (targetView && targetView.gameObject)
            {
                int mafiaLayer = LayerMask.NameToLayer("Mafia");
                if (mafiaLayer < 0)
                {
                    Debug.LogError("[MafiaAbility] 'Mafia' 레이어가 프로젝트에 정의되어 있지 않습니다.");
                    return;
                }

                targetView.gameObject.layer = mafiaLayer;
                foreach (Transform child in targetView.transform)
                {
                    child.gameObject.layer = mafiaLayer;
                }

                Debug.Log($"[MafiaAbility] Mafia Layer 적용 완료: {targetView.gameObject.name}");
            }
        }

        //==== Detective Ability ======

        public void RequestDetectiveInspectNearest(int requesterRootViewID, float radius)
        {
            photonView.RPC(nameof(RPC_RequestDetectiveInspectNearest), RpcTarget.MasterClient, requesterRootViewID, radius);
        }

        [PunRPC]
        private void RPC_RequestDetectiveInspectNearest(int requesterRootViewID, float radius, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var requester = PhotonView.Find(requesterRootViewID);

            // ===== 최근접 대상 탐색 (서버/호스트 판단) =====
            PhotonView nearest = null;
            float nearestSqr = float.MaxValue;
            Vector3 origin = requester.transform.position;
            var myOwner = requester.Owner;

            var allViews = GameObject.FindObjectsOfType<PhotonView>();
            for (int i = 0; i < allViews.Length; i++)
            {
                var v = allViews[i];
                if (!v || v.ViewID <= 0) continue;
                if (v.Owner == null) continue;
                if (v.ViewID == requesterRootViewID) continue; // 자기 자신 제외

                float sqr = (v.transform.position - origin).sqrMagnitude;
                if (sqr < nearestSqr && sqr <= radius * radius)
                {
                    nearestSqr = sqr;
                    nearest = v;
                }
            }

            if (!nearest)
            {
                photonView.RPC(nameof(RPC_DetectiveInspectResult), info.Sender, -1, -1);
                return;
            }

            // 색상/데이터 조회
            if (!GameDataManager.Instance.TryGetUIDByViewID(nearest.ViewID, out string targetUID) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(targetUID, out var priv))
            {
                photonView.RPC(nameof(RPC_DetectiveInspectResult), info.Sender, nearest.ViewID, -1);
                return;
            }

            int colorId = priv.identityColorId;
            photonView.RPC(nameof(RPC_DetectiveInspectResult), info.Sender, nearest.ViewID, colorId);
        }

        /// <summary>
        /// 호스트 → 요청 클라 : 탐문 결과 회신 (viewID, colorId)
        /// </summary>
        [PunRPC]
        private void RPC_DetectiveInspectResult(int targetViewID, int colorId)
        {
            // 클라 측에서 이벤트로 브릿지 (AbilityManager는 항상 활성)
            detectiveAbility.HandleInspectResult(targetViewID, colorId);
        }
    }
}

