using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public sealed class ChatManager : SingletonNetworkBehaviour<ChatManager>, IInRoomCallbacks
    {
        [Header("UI Refs")]
        [SerializeField] private Transform chatContentRoot;
        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Prefabs (assign in GameResources or here)")]
        [SerializeField] private ChatBubbleView myChatPrefab;
        [SerializeField] private ChatBubbleView otherChatPrefab;
        [SerializeField] private ChatBubbleView ghostMyChatPrefab;
        [SerializeField] private ChatBubbleView ghostOtherChatPrefab;

        [Header("Limits & Rules")]
        [SerializeField, Min(10)] private int maxMessages = 150;
        [SerializeField, Min(1)] private int maxCharsPerMessage = 200;
        [SerializeField, Range(0.1f, 5f)] private float minSendIntervalSec = 0.5f;

        // --- Runtime state ---
        private readonly List<ChatMessage> history = new List<ChatMessage>(); // host에서만 기록
        private readonly Dictionary<int, double> lastSendAt = new Dictionary<int, double>();
        private int seq; // host: monotonically increasing sequence for ordering

        // cache
        private bool uiReady;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            if (!scrollRect && chatContentRoot) scrollRect = chatContentRoot.GetComponentInParent<ScrollRect>();
        }

        private void Start()
        {
            S_InitHistory();
            photonView.RPC(nameof(C_ClearChatUI), RpcTarget.AllViaServer);
            uiReady = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region Public UI Entry

        public void C_TrySendMessage()
        {
            if (messageInputField == null) return;

            var raw = messageInputField.text;
            var msg = SanitizeMessage(raw, maxCharsPerMessage);
            if (string.IsNullOrEmpty(msg))
            {
                messageInputField.ActivateInputField();
                return;
            }

            photonView.RPC(nameof(S_ReceiveMessage), RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber, msg, PhotonNetwork.ServerTimestamp);

            messageInputField.text = string.Empty;
            messageInputField.ActivateInputField();
            messageInputField.Select();
        }

        // 게임 중 플레이어 조작 잠금(채팅창 활성화 시 등)
        public void C_DisablePlayerControl(bool disableControl)
        {
            var me = PlayerManager.Instance?.GetMyPlayer();
            if (!me) return;
            var pc = me.GetComponent<PlayerControl>();
            //if (pc) pc.DisablePlayerControl(disableControl);
        }

        #endregion

        #region Server (Host-only)

        private void S_InitHistory()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            history.Clear();
            seq = 0;
            lastSendAt.Clear();
        }

        [PunRPC]
        private void S_ReceiveMessage(int playerId, string message, int serverTs, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 보낸 사람 검증 (스푸핑 방지)
            if (info.Sender == null || info.Sender.ActorNumber != playerId) return;

            // 스팸 방지
            var now = Time.realtimeSinceStartupAsDouble;
            if (lastSendAt.TryGetValue(playerId, out var lastAt))
            {
                if (now - lastAt < minSendIntervalSec) return;
            }
            lastSendAt[playerId] = now;

            // 길이 제한
            if (string.IsNullOrEmpty(message)) return;
            if (message.Length > maxCharsPerMessage)
                message = message.Substring(0, maxCharsPerMessage);

            // ==== 인게임 생존 여부 판별 ====
            // 로비에서는 InGameData 자체가 없으므로 null이면 Global 채널로 처리
            var channel = ChatChannel.Global;
            if (TryIsAliveByActor(playerId, out var isAlive, out var hasInGameData))
            {
                // 인게임 데이터가 있을 때만 유령 채널 판정
                if (hasInGameData && !isAlive)
                    channel = ChatChannel.Ghost;
            }

            // 기록 추가(채널 포함)
            var msg = new ChatMessage(++seq, playerId, message, serverTs, channel);
            history.Add(msg);

            // 히스토리 용량 제한
            if (history.Count > maxMessages)
            {
                var removeCount = history.Count - maxMessages;
                history.RemoveRange(0, removeCount);
            }

            // ==== 타겟 전송 ====
            if (channel == ChatChannel.Global)
            {
                // 살아있는 플레이어가 보낸 메시지: 모두에게(Dead도 볼 수 있음)
                photonView.RPC(nameof(C_ReceiveMessage), RpcTarget.AllViaServer, msg.seq, msg.playerId, msg.message, msg.serverTs, false);
            }
            else
            {
                // 유령 채팅: Dead 플레이어에게만 전송
                // (Alive 쪽은 수신하지 않으므로 자연스럽게 '살아있는 사람은 살아있는 채팅만' 규칙 충족)
                foreach (var p in PhotonNetwork.PlayerList)
                {
                    if (TryIsAliveByActor(p.ActorNumber, out var targetAlive, out var targetHasData) && targetHasData && !targetAlive)
                    {
                        photonView.RPC(nameof(C_ReceiveMessage), p, msg.seq, msg.playerId, msg.message, msg.serverTs, true);
                    }
                }
            }
        }

        // 신규 참여자/마스터 교체 시 배치로 싱크
        private void S_SyncHistoryTo(Photon.Realtime.Player target)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (target == null) return;

            bool targetAlive = true;
            bool hasInGame = TryIsAliveByActor(target.ActorNumber, out targetAlive, out var targetHasData) && targetHasData;

            // 최근순으로 자르기
            const int kBatch = 50;
            var slice = history.Count > kBatch ? history.Skip(history.Count - kBatch) : history.AsEnumerable();

            // 타겟 가시성에 맞게 필터:
            // - Global: 항상 포함
            // - Ghost: Dead인 경우에만 포함
            var filtered = hasInGame && !targetAlive
                ? slice // Dead는 둘 다 받음
                : slice.Where(m => m.channel == ChatChannel.Global); // Alive/로비는 Global만

            var arr = filtered.ToArray();
            var seqs = arr.Select(m => m.seq).ToArray();
            var pids = arr.Select(m => m.playerId).ToArray();
            var texts = arr.Select(m => m.message).ToArray();
            var tss = arr.Select(m => m.serverTs).ToArray();
            var ghosts = arr.Select(m => m.channel == ChatChannel.Ghost).ToArray();

            photonView.RPC(nameof(C_BulkReceive), target, seqs, pids, texts, tss, ghosts);
        }

        /// <summary>
        /// sender/target의 생존 여부를 GameDataManager에서 조회.
        /// hasInGameData=false면 로비 등으로 간주.
        /// </summary>
        private static bool TryIsAliveByActor(int actorNum, out bool isAlive, out bool hasInGameData)
        {
            isAlive = true;
            hasInGameData = false;

            var gdm = GameDataManager.Instance;
            if (!gdm) return true; // 매니저 없으면 로비 취급(Global)

            if (gdm.TryGetInGameDataByActorId(actorNum, out var data) && data != null)
            {
                hasInGameData = true;
                // InGameData가 정의한 생존 플래그 사용(필드명은 프로젝트 기준으로 맞춰줘)
                isAlive = data.isAlive; // <-- 프로젝트의 실제 필드명으로 교체 필요
            }
            return true;
        }

        #endregion

        #region Client UI

        [PunRPC]
        private void C_ClearChatUI()
        {
            if (!chatContentRoot) return;
            for (int i = chatContentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(chatContentRoot.GetChild(i).gameObject);
            }
            if (scrollRect) scrollRect.verticalNormalizedPosition = 0;
        }

        [PunRPC]
        private void C_ReceiveMessage(int seq, int playerId, string message, int serverTs, bool isGhost)
        {
            if (!uiReady) return;
            AddBubble(playerId, message, isGhost);
        }

        [PunRPC]
        private void C_BulkReceive(int[] seqs, int[] playerIds, string[] messages, int[] serverTss, bool[] isGhostFlags)
        {
            if (!uiReady) return;
            if (seqs == null || messages == null) return;

            var items = new List<(int seq, int pid, string msg, int ts, bool ghost)>(seqs.Length);
            for (int i = 0; i < seqs.Length; i++)
                items.Add((seqs[i], playerIds[i], messages[i], serverTss[i], isGhostFlags != null && i < isGhostFlags.Length && isGhostFlags[i]));

            items.Sort((a, b) => a.seq.CompareTo(b.seq));
            foreach (var it in items)
                AddBubble(it.pid, it.msg, it.ghost);
        }

        private void AddBubble(int playerId, string message, bool isGhost)
        {
            if (!chatContentRoot) return;

            var isMine = PhotonNetwork.LocalPlayer != null && playerId == PhotonNetwork.LocalPlayer.ActorNumber;

            // -------- 프리팹 선택 분기 (4-way) --------
            ChatBubbleView prefab = ResolveBubblePrefab(isMine, isGhost);
            if (!prefab)
            {
                Debug.LogWarning("[ChatManager] ChatBubbleView prefab is missing for the requested state.");
                return;
            }

            var view = Instantiate(prefab, chatContentRoot);
            view.Bind(playerId.ToString(), message);

            view.gameObject.SetActive(chatContentRoot.gameObject.activeSelf);

            if (scrollRect)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        private ChatBubbleView ResolveBubblePrefab(bool isMine, bool isGhost)
        {
            // 1) 인스펙터에 직접 배치된 프리팹 우선
            if (isGhost)
            {
                if (isMine && ghostMyChatPrefab) return ghostMyChatPrefab;
                if (!isMine && ghostOtherChatPrefab) return ghostOtherChatPrefab;
            }
            else
            {
                if (isMine && myChatPrefab) return myChatPrefab;
                if (!isMine && otherChatPrefab) return otherChatPrefab;
            }

            // 2) 폴백: 유령 프리팹 비었으면 일반 프리팹 사용
            if (isGhost)
            {
                if (isMine && myChatPrefab) return myChatPrefab;
                if (!isMine && otherChatPrefab) return otherChatPrefab;
            }

            // 3) 최종 폴백: GameResources (프로젝트 자원 정책에 맞춰 유지)
            if (isMine)
            {
                var res = GameResources.Instance?.myChatContentPrefab?.GetComponent<ChatBubbleView>();
                if (res) return res;
            }
            else
            {
                var res = GameResources.Instance?.chatContentPrefab?.GetComponent<ChatBubbleView>();
                if (res) return res;
            }

            return null;
        }

        #endregion

        #region Helpers & Callbacks

        private static string SanitizeMessage(string src, int maxLen)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            var s = src.Replace("\r", "").Trim();
            if (s.Length > maxLen) s = s.Substring(0, maxLen);
            return s;
        }

        // 신규 입장자에게 히스토리 배치 전송
        void IInRoomCallbacks.OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient) S_SyncHistoryTo(newPlayer);
        }

        // 마스터 교체 시 새 마스터가 히스토리를 비게 되는 것을 방지
        void IInRoomCallbacks.OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer != null && newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                lastSendAt.Clear();
            }
        }

        // 미사용 콜백
        void IInRoomCallbacks.OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { }
        void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }

        #endregion

        public void OnClick_DisableControl(bool isActive)
        {
            if (PlayerManager.Instance.GetMyPlayer()) {
                //PlayerManager.Instance.GetMyPlayer()?.playerControl?.DisablePlayerControl(isActive);
            }
            else {
                //PlayerManager.Instance.GetMyLobbyPlayer()?.playerControl.DisablePlayerControl(isActive);
            }
        }
    }

    public enum ChatChannel : byte
    {
        Global = 0, // Alive가 보낸 메시지
        Ghost  = 1  // Dead만 가시
    }

    [Serializable]
    public struct ChatMessage
    {
        public int seq;        // 서버(호스트) 증가 시퀀스
        public int playerId;
        public string message;
        public int serverTs;   // Photon 서버 타임스탬프
        public ChatChannel channel;

        public ChatMessage(int seq, int playerId, string message, int serverTs, ChatChannel channel)
        {
            this.seq = seq;
            this.playerId = playerId;
            this.message = message;
            this.serverTs = serverTs;
            this.channel = channel;
        }
    }
}
