using UnityEngine;

namespace ColorPicker.Chat
{
    public sealed class ChatInstaller : MonoBehaviour
    {
        [SerializeField] private ChatConfig config;
        [SerializeField] private PhotonChatTransport transport;
        [SerializeField] private ChatUIController ui;

        private void Awake()
        {
            // Domain helpers
            var aliveQuery = new GameDataAliveQuery();

            // Infrastructure 준비
            transport.Init(aliveQuery);
            var adapter = transport.AsAdapter();

            // Application 서비스 구성
            var sanitizer = new SimpleSanitizer();
            var limiter = new SimpleRateLimiter();
            var history = new RingChatHistory();
            var service = new ChatService(config, sanitizer, limiter, history, aliveQuery, adapter);

            // UI에 주입
            ui.Install(service, transport);
        }
    }
}