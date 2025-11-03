using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

using CBV = ColorPicker.InGame.ChatBubbleView;

namespace ColorPicker.Chat
{
    [RequireComponent(typeof(PhotonView))]
    public sealed class ChatUIController : MonoBehaviourPun, IInRoomCallbacks
    {
        [Header("Refs")]
        [SerializeField] private Transform chatContentRoot;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ChatConfig config;

        [Header("Prefabs")]
        [SerializeField] private CBV myPrefab;
        [SerializeField] private CBV otherPrefab;
        [SerializeField] private CBV ghostMyPrefab;
        [SerializeField] private CBV ghostOtherPrefab;

        // DI 대상
        private IChatService _service;
        private PhotonChatTransport _transport;

        // Pools
        private ObjectPool<CBV> myPool, otherPool, ghostMyPool, ghostOtherPool;

        // 클라 측 쿨다운(서버 검증과 별개)
        private readonly SimpleRateLimiter _localLimiter = new();

        // --- config 폴백 값 ---
        private const int DEFAULT_MAX_CHARS = 200;
        private const float DEFAULT_MIN_INTERVAL = 0.5f;
        private const int DEFAULT_POOL_CAP = 32;
        private const int DEFAULT_POOL_MAX = 256;

        private void Awake()
        {
            if (!scrollRect && chatContentRoot)
                scrollRect = chatContentRoot.GetComponentInParent<ScrollRect>();
            if (input) input.richText = false;

            myPool = CreatePool(myPrefab, "MyPool");
            otherPool = CreatePool(otherPrefab, "OtherPool");
            ghostMyPool = CreatePool(ghostMyPrefab, "GhostMyPool");
            ghostOtherPool = CreatePool(ghostOtherPrefab, "GhostOtherPool");
        }

        /// <summary>외부 Installer에서 주입</summary>
        public void Install(IChatService svc, PhotonChatTransport transport)
        {
            // 이전 바인딩 정리 (이중 바인딩 방지)
            if (_transport != null)
            {
                _transport.OnClientReceive = null;
                _transport.OnClientBulkReceive = null;
            }

            _service = svc;
            _transport = transport;

            // 수신 바인딩
            if (_transport != null)
            {
                _transport.OnClientReceive = OnClientReceive;
                _transport.OnClientBulkReceive = OnClientBulkReceive;
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            PhotonNetwork.AddCallbackTarget(this);
            ClearUI();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            // 씬 언로드/비활성 시 수신 해제 (메모리 릭/유효하지 않은 콜백 방지)
            if (_transport != null)
            {
                _transport.OnClientReceive = null;
                _transport.OnClientBulkReceive = null;
            }
        }

        // === UI Entry ===
        public void OnClick_Send()
        {
            if (!input) return;

            string raw = input.text;
            if (string.IsNullOrWhiteSpace(raw)) // 공백만 전송 방지
            {
                input.ActivateInputField();
                return;
            }

            // 클라 쿨다운 (서버 검증과 별개)
            int myActor = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            float minInterval = config ? config.MinSendIntervalSec : DEFAULT_MIN_INTERVAL;
            if (!_localLimiter.CanSend(myActor, Time.realtimeSinceStartupAsDouble, minInterval))
                return;

            // 과도한 길이 클라 측에서도 1차 차단 (서버에서도 재검증)
            int maxChars = config ? config.MaxCharsPerMessage : DEFAULT_MAX_CHARS;
            if (raw.Length > maxChars) raw = raw.Substring(0, maxChars);

            photonView.RPC(nameof(S_ReceiveMessage), RpcTarget.MasterClient,
                myActor, raw, PhotonNetwork.ServerTimestamp);

            input.text = string.Empty;
            input.ActivateInputField();
            input.Select();
        }

        [PunRPC]
        private void S_ReceiveMessage(int playerId, string raw, int ts, PhotonMessageInfo info)
        {
            if (_service == null) return; // 방어
                                          // 서버에서 ChatService 처리 (송신자 검증/정화/채널 분배/브로드캐스트)
            _service.ReceiveClientMessage(playerId, raw, ts, info);
        }

        // === 클라 수신 ===
        private void OnClientReceive(ChatMessage msg) => AddBubble(msg);

        private void OnClientBulkReceive(int[] seqs, int[] pids, string[] texts, int[] tss, bool[] ghosts)
        {
            if (seqs == null || texts == null) return;
            int len = seqs.Length;
            for (int i = 0; i < len; i++)
            {
                var ch = (ghosts != null && i < ghosts.Length && ghosts[i]) ? ChatChannel.Ghost : ChatChannel.Global;
                AddBubble(new ChatMessage(seqs[i], pids[i], texts[i], tss[i], ch));
            }
        }

        private void AddBubble(ChatMessage m)
        {
            if (!chatContentRoot) return;

            bool isMine = PhotonNetwork.LocalPlayer != null &&
                          m.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber;

            var pool = ResolvePool(isMine, m.Channel == ChatChannel.Ghost);
            var view = pool?.Get();
            if (!view) return;

            view.AttachPoolOwner(pool);                 // 풀 소유권 연결 (없으면 ChatBubbleView에 구현 추가 필요)
            view.Bind(m.PlayerId.ToString(), m.Text);
            view.transform.SetParent(chatContentRoot, false);
            view.gameObject.SetActive(chatContentRoot.gameObject.activeSelf);

            if (scrollRect)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ClearUI()
        {
            if (!chatContentRoot) return;
            for (int i = chatContentRoot.childCount - 1; i >= 0; i--)
            {
                var go = chatContentRoot.GetChild(i).gameObject;
                var view = go.GetComponent<CBV>();
                if (view != null && view.HasPoolOwner)
                    view.ReleaseToPool();
                else
                    Destroy(go);
            }
            if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
        }

        private ObjectPool<CBV> ResolvePool(bool isMine, bool isGhost)
        {
            if (isGhost)
            {
                if (isMine && ghostMyPool != null) return ghostMyPool;
                if (!isMine && ghostOtherPool != null) return ghostOtherPool;
                // 폴백: 유령 프리팹 없으면 일반 사용
                return isMine ? myPool : otherPool;
            }
            else
            {
                return isMine ? myPool : otherPool;
            }
        }

        private ObjectPool<CBV> CreatePool(CBV prefab, string name)
        {
            if (!prefab) return null;

            int defaultCap = config ? config.PoolDefaultCapacity : DEFAULT_POOL_CAP;
            int maxSize = config ? config.PoolMaxSize : DEFAULT_POOL_MAX;

            return new ObjectPool<CBV>(
                createFunc: () =>
                {
                    var v = Instantiate(prefab);
                    v.name = name + "_Item";
                    v.gameObject.SetActive(false);
                    return v;
                },
                actionOnGet: (v) => v.gameObject.SetActive(true),
                actionOnRelease: (v) =>
                {
                    v.gameObject.SetActive(false);
                    if (chatContentRoot) v.transform.SetParent(chatContentRoot, false);
                },
                actionOnDestroy: (v) => { if (v) Destroy(v.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultCap,
                maxSize: maxSize
            );
        }

        // 신규 입장자 배치 싱크/마스터 전환 대응
        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
            => _service?.SyncHistoryToTarget(newPlayer.ActorNumber);

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMaster)
        {
            // TODO : 새 마스터 전환 시 UI 쪽에서도 상태 리셋/재요청 로직 추가
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer) { }
        void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
    }
}