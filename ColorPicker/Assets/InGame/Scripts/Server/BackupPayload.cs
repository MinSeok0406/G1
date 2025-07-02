using ColorPicker.InGame;
using System.Collections.Generic;

namespace ColorPicker.InGame
{
    [System.Serializable]
    public class BackupPayload
    {
        // 클라이언트 사용
        public List<PublicPlayerData> publicPlayerDataList;
        public List<InGameDataEntry> inGameDataList;
        public GameStateType gameState; // 게임 상태 동기화 
        
        // 백업용 
        public List<string> encryptedPrivatePlayerDataList; // 암호화된 JSON 문자열
        public List<ViewIDEntry> viewIDMappings;
    }
}
