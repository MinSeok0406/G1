using System;
using System.Linq;
using ColorPicker.InGame;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Video;

namespace ColorPicker.InGame
{
    [DisallowMultipleComponent]
    public sealed class CutsceneManager : SingletonNetworkBehaviour<CutsceneManager>, IOnEventCallback 
    {
        // --- Inspector ---
        [Header("Refs")]
        [SerializeField] private CutsceneCatalog catalog;
        [SerializeField] private VideoPlayer videoPlayer;   // RawImage/RenderTexture 연결
        [SerializeField] private GameObject overlayRoot;    // 블랙바/스킵 버튼 그룹(선택)

        [Header("Playback")]
        [Tooltip("클라 준비 여유(초). 짧은 컷신은 너무 크게 잡지 말 것.")]
        [SerializeField, Range(0, 0.5f)] private double hostLeadDelaySec = 0.08d;

        [Tooltip("Prepare 대기 타임아웃(초)")]
        [SerializeField, Min(0.1f)] private double prepareTimeoutSec = 2.5d;

        [Tooltip("호스트 연속 시작 쿨다운(초) - 실수로 연타 방지")]
        [SerializeField, Min(0f)] private float startCooldownSec = 0.2f;

        [Tooltip("로그 상세")]
        [SerializeField] private bool verbose = false;

        private bool IsHost => PhotonNetwork.IsMasterClient;
        private int LocalActor => PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;

        // --- Event codes ---
        private const byte EVT_START = 111;
        private const byte EVT_STOP  = 112;

        // --- Targeting ---
        private enum TargetMode : byte { All = 0, List = 1, Single = 2 }

        // --- Local state cache ---
        public bool   IsPlaying        { get; private set; }
        public string CurrentKey       { get; private set; }
        public double StartNetworkTime { get; private set; }
        public double EstimatedDurSec  { get; private set; }
        public bool   AllowSkip        { get; private set; }

        private float _lastHostStartAt;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            if (!catalog)     Debug.LogError("[Cutscene] Catalog not set.");
            if (!videoPlayer) Debug.LogError("[Cutscene] VideoPlayer not set.");
        }

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            if (videoPlayer) videoPlayer.errorReceived += OnVideoError;
        }

        public override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            if (videoPlayer) videoPlayer.errorReceived -= OnVideoError;
        }

        // ========================= Host API =========================

        /// <summary>전체 플레이어에게 컷신 시작.</summary>
        public void Host_PlayForAll(string key)
        {
            GuardHost(nameof(Host_PlayForAll));
            if (!TryResolve(key, out var entry)) return;
            if (IsOverCooldown()) return;

            double t0  = PhotonNetwork.Time + hostLeadDelaySec;
            double dur = CutsceneCatalog.EstimateDuration(entry);

            BroadcastStart(key, t0, dur, entry.allowSkip, TargetMode.All, null, 0);
            ApplyLocalStart(key, t0, dur, entry.allowSkip);
            HandleStartPlayback(entry, t0, dur);

            _lastHostStartAt = Time.unscaledTime;
        }

        /// <summary>특정 액터 1명에게만 컷신 시작(예: 킬 피해자만).</summary>
        public void Host_PlayForActor(string key, int actorNumber)
        {
            GuardHost(nameof(Host_PlayForActor));
            if (!TryResolve(key, out var entry)) return;
            if (IsOverCooldown()) return;

            double t0  = PhotonNetwork.Time + hostLeadDelaySec;
            double dur = CutsceneCatalog.EstimateDuration(entry);

            BroadcastStart(key, t0, dur, entry.allowSkip, TargetMode.Single, null, actorNumber);
            // 호스트가 대상이면 로컬도 재생해야 하므로 아래 처리
            if (LocalActor == actorNumber) HandleStartPlayback(entry, t0, dur);

            _lastHostStartAt = Time.unscaledTime;
        }

        /// <summary>여러 액터에게만 컷신 시작(예: 킬 연루자 2명만).</summary>
        public void Host_PlayForActors(string key, int[] actorNumbers)
        {
            GuardHost(nameof(Host_PlayForActors));
            if (actorNumbers == null || actorNumbers.Length == 0) return;
            if (!TryResolve(key, out var entry)) return;
            if (IsOverCooldown()) return;

            double t0  = PhotonNetwork.Time + hostLeadDelaySec;
            double dur = CutsceneCatalog.EstimateDuration(entry);

            BroadcastStart(key, t0, dur, entry.allowSkip, TargetMode.List, actorNumbers, 0);
            if (actorNumbers.Contains(LocalActor)) HandleStartPlayback(entry, t0, dur);

            _lastHostStartAt = Time.unscaledTime;
        }

        /// <summary>현재 재생 중단/스킵.</summary>
        public void Host_Stop()
        {
            GuardHost(nameof(Host_Stop));
            if (!IsPlaying) { BroadcastStop(); return; }

            BroadcastStop();
            ApplyLocalStop();
            HandleStopPlayback();
        }

        private bool IsOverCooldown() => (Time.unscaledTime - _lastHostStartAt) < startCooldownSec;

        // ======================= Client Requests =====================

        [PunRPC] public void RPC_RequestPlayForAll(string key, PhotonMessageInfo _)       { if (IsHost) Host_PlayForAll(key); }
        [PunRPC] public void RPC_RequestPlayForActor(string key, int actor, PhotonMessageInfo _) { if (IsHost) Host_PlayForActor(key, actor); }
        [PunRPC] public void RPC_RequestPlayForActors(string key, int[] actors, PhotonMessageInfo _) { if (IsHost) Host_PlayForActors(key, actors); }

        [PunRPC] public void RPC_RequestStop(PhotonMessageInfo _) { if (IsHost && AllowSkip) Host_Stop(); }

        // ====================== Photon Events ========================

        private void BroadcastStart(string key, double t0, double dur, bool allowSkip,
                                    TargetMode mode, int[] list, int single)
        {
            // 데이터: key, t0, dur, allowSkip, mode, list(optional), single(optional)
            var data = new object[] { key, t0, dur, allowSkip, (byte)mode, list, single };

            PhotonNetwork.RaiseEvent(
                EVT_START, data,
                new RaiseEventOptions {
                    Receivers = ReceiverGroup.All,
                    CachingOption = EventCaching.AddToRoomCache // 늦참 수신
                },
                SendOptions.SendReliable
            );
        }

        private void BroadcastStop()
        {
            PhotonNetwork.RaiseEvent(
                EVT_STOP, null,
                new RaiseEventOptions {
                    Receivers = ReceiverGroup.All,
                    CachingOption = EventCaching.RemoveFromRoomCache
                },
                SendOptions.SendReliable
            );
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == EVT_START)
            {
                var d = (object[])photonEvent.CustomData;
                string key      = (string)d[0];
                double t0       = (double)d[1];
                double dur      = (double)d[2];
                bool allowSkip  = (bool)d[3];
                var mode        = (TargetMode)(byte)d[4];
                var list        = d[5] as int[];
                int single      = (int)d[6];

                // 짧은 컷신: 늦참이 캐시된 START를 수신해도, 이미 지난 컷신은 무시
                if (PhotonNetwork.Time - t0 >= dur) return;

                // 타깃 필터
                if (!ShouldPlayOnThisClient(mode, list, single)) return;

                ApplyLocalStart(key, t0, dur, allowSkip);

                if (!TryResolve(key, out var entry)) return;
                HandleStartPlayback(entry, t0, dur);
            }
            else if (photonEvent.Code == EVT_STOP)
            {
                ApplyLocalStop();
                HandleStopPlayback();
            }
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            // 아주 짧은 컷신이라도, 진행 중이면 새 호스트가 상태 재방송
            if (IsHost && IsPlaying)
            {
                // 재방송 도중 이미 끝났다면 수신 측에서 dur 검사로 무시됨
                BroadcastStart(CurrentKey, StartNetworkTime, EstimatedDurSec, AllowSkip, TargetMode.All, null, 0);
            }
        }

        private bool ShouldPlayOnThisClient(TargetMode mode, int[] list, int single)
        {
            switch (mode)
            {
                case TargetMode.All:   return true;
                case TargetMode.Single:return LocalActor == single;
                case TargetMode.List:  return list != null && list.Contains(LocalActor);
                default:               return false;
            }
        }

        // ===================== Playback ==============================

        private void HandleStartPlayback(CutsceneCatalog.Entry entry, double startNetTime, double durationSec)
        {
            if (!videoPlayer) return;

            // UI On (선택)
            if (overlayRoot) overlayRoot.SetActive(true);

            // 세팅
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping   = false;
            videoPlayer.skipOnDrop  = true;
            videoPlayer.source      = VideoSource.VideoClip;
            videoPlayer.clip        = entry.clip;

            videoPlayer.loopPointReached -= OnVideoEnded_HostOnly;
            videoPlayer.loopPointReached += OnVideoEnded_HostOnly;

            StartCoroutine(Co_PrepareSeekPlay(startNetTime, durationSec));
        }

        private System.Collections.IEnumerator Co_PrepareSeekPlay(double startNetTime, double durationSec)
        {
            if (!videoPlayer) yield break;

            var go = videoPlayer.gameObject;
            if (go && !go.activeSelf) go.SetActive(true);

            bool prepared = false;
            double tStart = Time.unscaledTimeAsDouble;

            try
            {
                try { videoPlayer.Prepare(); }
                catch (Exception e) { Debug.LogWarning($"[Cutscene] Prepare failed: {e.Message}"); yield break; }

                while (!prepared && (Time.unscaledTimeAsDouble - tStart) < prepareTimeoutSec)
                {
                    if (!IsPlaying || !videoPlayer || !go || !go.activeInHierarchy) yield break;
                    prepared = videoPlayer.isPrepared;
                    if (prepared) break;
                    yield return null;
                }

                if (!prepared)
                {
                    Debug.LogWarning("[Cutscene] Prepare timeout.");
                    if (IsHost && IsPlaying) Host_Stop();
                    else { ApplyLocalStop(); HandleStopPlayback(); }
                    yield break;
                }

                double elapsed = Math.Max(0, PhotonNetwork.Time - startNetTime);
                double seekSec = Mathf.Clamp((float)elapsed, 0f, (float)durationSec);
                TrySeek(seekSec);

                try { videoPlayer.Play(); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Cutscene] Play failed: {e.Message}");
                    if (IsHost && IsPlaying) Host_Stop();
                    else { ApplyLocalStop(); HandleStopPlayback(); }
                    yield break;
                }

                if (!IsHost)
                {
                    yield return new WaitUntil(() =>
                        !videoPlayer || !go || !go.activeInHierarchy || !videoPlayer.isPlaying);

                    if (IsPlaying) 
                    {
                        ApplyLocalStop();
                        HandleStopPlayback(); 
                    }
                    yield break;
                }

                double remain = Math.Max(0, durationSec - seekSec);
                yield return new WaitForSecondsRealtime((float)remain + 0.05f);
                if (IsPlaying) Host_Stop();
            }
            finally
            {
                if (videoPlayer)
                {
                    if (!videoPlayer.isPlaying && go && go.activeSelf)
                        go.SetActive(false);
                }
                else if (go && go.activeSelf)
                {
                    go.SetActive(false);
                }
            }
        }


        private void HandleStopPlayback()
        {
            if (videoPlayer)
            {
                videoPlayer.loopPointReached -= OnVideoEnded_HostOnly;
                if (videoPlayer.isPlaying) videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
            if (overlayRoot) overlayRoot.SetActive(false);
        }

        private void OnVideoEnded_HostOnly(VideoPlayer vp)
        {
            if (!IsHost) return;
            if (IsPlaying) Host_Stop();
        }

        private void TrySeek(double sec)
        {
            try { if (videoPlayer.canSetTime) videoPlayer.time = Math.Max(0, sec); }
            catch (Exception e) { Debug.LogWarning($"[Cutscene] Seek failed: {e.Message}"); }
        }

        private void OnVideoError(VideoPlayer src, string msg)
        {
            Debug.LogWarning($"[Cutscene] Video error: {msg}");
            if (IsHost && IsPlaying) Host_Stop();
        }

        // ===================== Helpers ===============================

        private bool TryResolve(string key, out CutsceneCatalog.Entry entry)
        {
            entry = null;
            if (!catalog) { Debug.LogError("[Cutscene] Catalog missing."); return false; }
            if (!catalog.TryGet(key, out entry) || entry.clip == null)
            {
                Debug.LogError($"[Cutscene] Unknown/invalid key: {key}");
                return false;
            }
            return true;
        }

        private void ApplyLocalStart(string key, double t0, double dur, bool skip)
        {
            IsPlaying        = true;
            CurrentKey       = key;
            StartNetworkTime = t0;
            EstimatedDurSec  = dur;
            AllowSkip        = skip;
        }

        private void ApplyLocalStop()
        {
            IsPlaying        = false;
            CurrentKey       = null;
            StartNetworkTime = 0;
            EstimatedDurSec  = 0;
            AllowSkip        = true;
        }

        private void GuardHost(string where)
        {
            if (!IsHost) throw new InvalidOperationException($"[{nameof(CutsceneManager)}] {where} must be called by Host.");
        }
    }
}
