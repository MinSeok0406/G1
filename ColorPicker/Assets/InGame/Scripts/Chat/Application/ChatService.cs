using System.Collections.Generic;

namespace ColorPicker.Chat
{
    /// 전송/플랫폼 의존성 없는 순수 도메인 서비스
    public sealed class ChatService : IChatService
    {
        private readonly ChatConfig _cfg;
        private readonly IChatSanitizer _sanitizer;
        private readonly IRateLimiter _rateLimiter;
        private readonly IChatHistory _history;
        private readonly IAliveQuery _aliveQuery;
        private readonly IChatTransport _transport; // 인프라 주입
        private int _seq;

        private readonly List<ChatMessage> _tmp = new(128);

        public ChatService(
            ChatConfig cfg,
            IChatSanitizer sanitizer,
            IRateLimiter rateLimiter,
            IChatHistory history,
            IAliveQuery aliveQuery,
            IChatTransport transport)
        {
            _cfg = cfg; _sanitizer = sanitizer; _rateLimiter = rateLimiter;
            _history = history; _aliveQuery = aliveQuery; _transport = transport;
            _history.Clear(_cfg.MaxMessages);
        }

        public int NextSeq() => unchecked(++_seq);

        public ChatChannel ResolveChannelForSender(int actorNumber)
        {
            if (_aliveQuery.TryGetAliveState(actorNumber, out bool hasInGame, out bool alive))
                return (hasInGame && !alive) ? ChatChannel.Ghost : ChatChannel.Global;

            // 매니저 미존재(로비 등): 글로벌 처리
            return ChatChannel.Global;
        }

        /// 클라가 보낸 메시지를 서버(호스트)에서 수신 처리
        public void ReceiveClientMessage(int playerId, string rawText, int serverTs, object senderContext)
        {
            if (!_transport.IsServer) return;

            // 송신자 검증(스푸핑 방지) - 인프라가 제공
            if (!_transport.ValidateSenderContext(senderContext, playerId)) return;

            // 레이트리밋
            double now = _transport.NowSeconds;
            if (!_rateLimiter.CanSend(playerId, now, _cfg.MinSendIntervalSec)) return;

            // 정화/길이 제한
            var text = _sanitizer.Sanitize(rawText, _cfg.MaxCharsPerMessage);
            if (string.IsNullOrEmpty(text)) return;

            // 채널 판정
            var channel = ResolveChannelForSender(playerId);

            // 기록 & 시퀀스
            var msg = new ChatMessage(NextSeq(), playerId, text, serverTs, channel);
            _history.Add(msg, _cfg.MaxMessages);

            // 타깃 전송
            if (channel == ChatChannel.Global)
            {
                _transport.BroadcastToAll(msg, isGhost:false);
            }
            else
            {
                _transport.BroadcastToGhostOnly(msg);
            }
        }

        public void SyncHistoryToTarget(int targetActorNumber)
        {
            if (!_transport.IsServer) return;

            bool includeGhost = false;
            if (_aliveQuery.TryGetAliveState(targetActorNumber, out bool has, out bool alive))
                includeGhost = (has && !alive);

            _history.GetTailFiltered(_cfg.SyncBatchCount, includeGhost, _tmp);
            _transport.SendBulkTo(targetActorNumber, _tmp);
        }
    }
}
