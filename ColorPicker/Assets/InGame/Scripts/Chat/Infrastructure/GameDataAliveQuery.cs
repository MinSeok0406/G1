using ColorPicker.InGame;

namespace ColorPicker.Chat
{
    public sealed class GameDataAliveQuery : IAliveQuery
    {
        public bool TryGetAliveState(int actorNumber, out bool hasInGame, out bool isAlive)
        {
            hasInGame = false; isAlive = true;
            var gdm = GameDataManager.Instance;
            if (!gdm) return false; // 매니저 없음 → 로비로 처리

            if (!gdm.TryGetInGameDataByActorId(actorNumber, out var data) || data == null)
                return true; // 매니저는 있으나 인게임 데이터 없음(로비)

            hasInGame = true;
            isAlive = data.isAlive; // 실제 필드명에 맞춰 조정
            return true;
        }
    }
}