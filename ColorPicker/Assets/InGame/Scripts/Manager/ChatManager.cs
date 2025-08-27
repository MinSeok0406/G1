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
    /// <summary>
    /// Host-authoritative Chat Manager
    /// - S_ 접두사는 반드시 MasterClient에서만 실행
    /// - C_ 접두사는 클라이언트 UI 갱신 전용
    /// </summary>
    public sealed class ChatManager : SingletonNetworkBehaviour<ChatManager>, IInRoomCallbacks
    {
        [Header("UI Refs")]
        [SerializeField] private Transform chatContentRoot;
        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Prefabs (assign in GameResources or here)")]
        [SerializeField] private ChatBubbleView myChatPrefab;
        [SerializeField] private ChatBubbleView otherChatPrefab;

        [Header("Limits & Rules")]
        [SerializeField, Min(10)] private int maxMessages = 150;
        [SerializeField, Min(1)] private int maxCharsPerMessage = 200;
        [SerializeField, Range(0.1f, 5f)] private float minSendIntervalSec = 0.5f;

        // --- Runtime state ---
        private readonly List<ChatMessage> _history = new();         // host에서만 쓰기
        private readonly Dictionary<int, double> _lastSendAt = new(); // host: anti-spam
        private int _seq; // host: monotonically increasing sequence for ordering

        // cache
        private bool _uiReady;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            // Null/assignment guards
            if (!scrollRect && chatContentRoot) scrollRect = chatContentRoot.GetComponentInParent<ScrollRect>();
        }

        private void Start()
        {
            // 호스트는 히스토리 초기화
            S_InitHistory();

            // 모든 클라 UI 초기화
            photonView.RPC(nameof(C_ClearChatUI), RpcTarget.AllViaServer);
            _uiReady = true;
        }

#if UNITY_EDITOR
        private void Update()
        {
            // 에디터에서만 디버그 초기화
            if (Input.GetKeyDown(KeyCode.Z))
            {
                photonView.RPC(nameof(C_ClearChatUI), RpcTarget.AllViaServer);
                if (PhotonNetwork.IsMasterClient) _history.Clear();
            }
        }
#endif

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

        // UI 버튼에 연결
        public void C_TrySendMessage()
        {
            if (messageInputField == null) return;

            var raw = messageInputField.text;
            var msg = SanitizeMessage(raw, maxCharsPerMessage);
            if (string.IsNullOrEmpty(msg)) // 빈 메시지 or 전부 공백
            {
                // 포커스 유지
                messageInputField.ActivateInputField();
                return;
            }

            // 서버 타임스탬프 함께 전달 (정렬 보조)
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
            if (pc) pc.DisablePlayerControl(disableControl);
        }

        #endregion

        #region Server (Host-only)

        private void S_InitHistory()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            _history.Clear();
            _seq = 0;
            _lastSendAt.Clear();
        }

        [PunRPC]
        private void S_ReceiveMessage(int playerId, string message, int serverTs, PhotonMessageInfo info)
        {
            // 안정성: 호스트만
            if (!PhotonNetwork.IsMasterClient) return;

            // 신뢰성: 보낸 사람 검증 (스푸핑 방지)
            if (info.Sender == null || info.Sender.ActorNumber != playerId) return;

            // 스팸 방지
            var now = Time.realtimeSinceStartupAsDouble; // 호스트 로컬 시간
            if (_lastSendAt.TryGetValue(playerId, out var lastAt))
            {
                if (now - lastAt < minSendIntervalSec) return;
            }
            _lastSendAt[playerId] = now;

            // 길이 제한 (클라에서 이미 잘랐어도 한 번 더)
            if (string.IsNullOrEmpty(message)) return;
            if (message.Length > maxCharsPerMessage)
                message = message.Substring(0, maxCharsPerMessage);

            // 기록 추가
            var msg = new ChatMessage(++_seq, playerId, message, serverTs);
            _history.Add(msg);

            // 히스토리 용량 제한
            if (_history.Count > maxMessages)
            {
                var removeCount = _history.Count - maxMessages;
                _history.RemoveRange(0, removeCount);
            }

            // 전체 클라에 단건 브로드캐스트
            photonView.RPC(nameof(C_ReceiveMessage), RpcTarget.AllViaServer, msg.seq, msg.playerId, msg.message, msg.serverTs);
        }

        // 신규 참여자/마스터 교체 시 배치로 싱크
        private void S_SyncHistoryTo(Photon.Realtime.Player target)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (target == null) return;

            // 최근순으로 잘라서 보냄 (패킷 크기 방지)
            const int kBatch = 50;
            var slice = _history.Count > kBatch ? _history.Skip(_history.Count - kBatch).ToArray() : _history.ToArray();

            var seqs = slice.Select(m => m.seq).ToArray();
            var pids = slice.Select(m => m.playerId).ToArray();
            var texts = slice.Select(m => m.message).ToArray();
            var tss = slice.Select(m => m.serverTs).ToArray();

            photonView.RPC(nameof(C_BulkReceive), target, seqs, pids, texts, tss);
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
            // 스크롤 맨 아래
            if (scrollRect) scrollRect.verticalNormalizedPosition = 0;
        }

        [PunRPC]
        private void C_ReceiveMessage(int seq, int playerId, string message, int serverTs)
        {
            if (!_uiReady) return;
            AddBubble(playerId, message);
        }

        [PunRPC]
        private void C_BulkReceive(int[] seqs, int[] playerIds, string[] messages, int[] serverTss)
        {
            if (!_uiReady) return;
            if (seqs == null || messages == null) return;

            // 순서 보장 (seq 기준)
            var items = new List<(int seq, int pid, string msg, int ts)>();
            for (int i = 0; i < seqs.Length; i++)
            {
                items.Add((seqs[i], playerIds[i], messages[i], serverTss[i]));
            }
            items.Sort((a, b) => a.seq.CompareTo(b.seq));

            foreach (var it in items)
                AddBubble(it.pid, it.msg);
        }

        private void AddBubble(int playerId, string message)
        {
            if (!chatContentRoot) return;

            // 프리팹 결정 (직관적인 내/타인 구분)
            var isMine = PhotonNetwork.LocalPlayer != null && playerId == PhotonNetwork.LocalPlayer.ActorNumber;

            // 리소스 경로/관리 방침에 따라 GameResources 사용 or 직접 SerializeField 된 프리팹 사용
            var prefab = isMine
                ? (myChatPrefab ? myChatPrefab : GameResources.Instance.myChatContentPrefab.GetComponent<ChatBubbleView>())
                : (otherChatPrefab ? otherChatPrefab : GameResources.Instance.chatContentPrefab.GetComponent<ChatBubbleView>());

            if (!prefab)
            {
                Debug.LogWarning("[ChatManager] ChatBubbleView prefab is missing.");
                return;
            }

            var view = Instantiate(prefab, chatContentRoot);
            view.Bind(playerId.ToString(), message);

            // 컨테이너의 활성 상태에 맞춰 UI도 맞춤
            view.gameObject.SetActive(chatContentRoot.gameObject.activeSelf);

            // 스크롤 맨 아래로
            if (scrollRect)
            {
                // 한 프레임 뒤로 미루면 레이아웃 반영 후 정확히 바닥으로 내려감
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        #endregion

        #region Helpers & Callbacks

        private static string SanitizeMessage(string src, int maxLen)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            // 개행 정리, 앞뒤 공백 제거
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
            // 새 마스터가 이 클라라면, UI는 그대로 두고 서버 역할만 인계됨.
            if (PhotonNetwork.LocalPlayer != null && newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // 아무 것도 하지 않아도 됨(이 인스턴스가 호스트로 승격). 필요하면 여기서 _lastSendAt 초기화 등 수행.
                _lastSendAt.Clear();
            }
        }

        // 미사용 콜백
        void IInRoomCallbacks.OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { }
        void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }

        #endregion
    }

    [Serializable]
    public struct ChatMessage
    {
        public int seq;        // 서버(호스트) 증가 시퀀스
        public int playerId;
        public string message;
        public int serverTs;   // Photon 서버 타임스탬프

        public ChatMessage(int seq, int playerId, string message, int serverTs)
        {
            this.seq = seq;
            this.playerId = playerId;
            this.message = message;
            this.serverTs = serverTs;
        }
    }
}