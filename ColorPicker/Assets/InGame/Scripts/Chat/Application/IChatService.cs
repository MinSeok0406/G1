namespace ColorPicker.Chat
{
    public interface IChatService
    {
        // 클라 → 서버
        void ReceiveClientMessage(int playerId, string rawText, int serverTs, object senderContext);

        // 서버 → 클라 (전송은 IChatTransport 구현체가 수행)
        void SyncHistoryToTarget(int targetActorNumber);

        // 서버/클라 공용 유틸
        ChatChannel ResolveChannelForSender(int actorNumber);

        // 서버에서만 사용
        int NextSeq();
    }
}
