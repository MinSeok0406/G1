using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using SharedDB;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        /*public void HandleLogin(C_Login loginPacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            // TODO : 문제가 있긴 있다
            // - 동시에 다른 사람이 같은 UniqueId을 보낸다면?
            // - 악의적으로 여러번 보낸다면
            // - 쌩뚱맞은 타이밍에 그냥 이 패킷을 보낸다면?

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

                if (findAccount != null)
                {
                    // AccountDbId 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            Speed = playerDb.Speed
                        };

                        // 메모리에도 들고 있다
                        LobbyPlayers.Add(lobbyPlayer);

                        // 패킷에 넣어준다
                        loginOk.Players.Add(lobbyPlayer);
                    }

                    Send(loginOk);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;

                    // AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }*/
        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null)
                return;

            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.None;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;
                MyPlayer.Info.PosInfo.Speed = playerInfo.Speed;
                MyPlayer.Session = this;

                S_AchievementList achievementListPacket = new S_AchievementList(); 

                // 아이템 목록을 갖고 온다
                using (AppDbContext db = new AppDbContext())
                {
                    List<AchievementDb> achievements = db.Achievements
                        .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                        .ToList();

                    foreach (AchievementDb achievementDb in achievements)
                    {
                        Achievement achievement = Achievement.GetAchievement(achievementDb);
                        if (achievement != null)
                        {
                            MyPlayer.slot.Add(achievement);

                            AchievementInfo info = new AchievementInfo();
                            info.MergeFrom(achievement.Info);
                            achievementListPacket.Achievements.Add(info);
                        }
                    }
                }

                Send(achievementListPacket);
            }

            ServerState = PlayerServerState.ServerStateGame;

            GameRoom room = GameLogic.Instance.Find(1);
            room.Push(room.EnterGame, MyPlayer);
        }
        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

                if (findPlayer != null)
                {
                    // 이름이 겹친다
                    Send(new S_CreatePlayer());
                }
                else
                {
                    // DB에 플레이어 만들어줘야 함
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        // Speed 서버에서 직접 관리 버전
                        //Speed = 10.0f;
                        AccountDbId = AccountDbId
                    };

                    db.Players.Add(newPlayerDb);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;

                    // 메모리에 추가
                    LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name = createPacket.Name,
                        // Speed 서버에서 직접 관리 버전
                        // Speed = 10.0f;
                    };

                    // 메모리에도 들고 있다
                    LobbyPlayers.Add(lobbyPlayer);

                    // 클라에 전송
                    S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
                    newPlayer.Player.MergeFrom(lobbyPlayer);

                    Send(newPlayer);
                }
            }
        }

        public void HandleAuth(C_Auth req)
        {
            // 로그인 상태에서만 허용
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            // 기본 검증
            if (req.AccountId <= 0 || req.Token <= 0)
            {
                Send(new S_Auth { Ok = false });
                return;
            }

            bool ok = ValidateToken(req.AccountId, req.Token);
            if (!ok)
            {
                Send(new S_Auth { Ok = false });
                // 필요하면 즉시 끊기: Disconnect();
                return;
            }

            // 인증 성공 → 세션에 바인딩
            IsAuthed = true;
            GoogleId = req.GoogleId ?? string.Empty;
            UserName = req.Name ?? string.Empty;

            // === 로컬(Game DB) 계정 동기화 ===
            // 기존엔 UniqueId(디바이스ID)로 찾았지만, 이제는 GoogleId를 키로 삼는다.
            // (GoogleId가 없으면 이름, 둘 다 없으면 외부ID 기반 문자열)
            string accountKey = !string.IsNullOrWhiteSpace(GoogleId)
                                ? GoogleId
                                : (!string.IsNullOrWhiteSpace(UserName) ? UserName : $"ext_{req.AccountId}");

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                // GoogleId(=accountKey)로 계정 찾기
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == accountKey)
                    .FirstOrDefault();

                if (findAccount == null)
                {
                    // 없으면 생성 (기존 UniqueId 흐름과 동일한 동작 보장)
                    var newAccount = new AccountDb { AccountName = accountKey };
                    db.Accounts.Add(newAccount);
                    bool created = db.SaveChangesEx();
                    if (!created)
                    {
                        Send(new S_Auth { Ok = false });
                        return;
                    }
                    findAccount = newAccount;
                }

                // 로컬(Game DB)의 AccountDbId를 세션에 보관
                AccountDbId = findAccount.AccountDbId;

                // 인증 OK 응답
                Send(new S_Auth { Ok = true });

                // === 기존 로그인 스냅샷(S_Login) 내려주기 ===
                S_Login loginOk = new S_Login() { LoginOk = 1 };
                foreach (PlayerDb playerDb in findAccount.Players)
                {
                    var lobbyPlayer = new LobbyPlayerInfo
                    {
                        PlayerDbId = playerDb.PlayerDbId,
                        Name = playerDb.PlayerName,
                        Speed = playerDb.Speed
                    };

                    LobbyPlayers.Add(lobbyPlayer);
                    loginOk.Players.Add(lobbyPlayer);
                }
                Send(loginOk);

                // 로비로 전환
                ServerState = PlayerServerState.ServerStateLobby;
            }
        }

        private bool ValidateToken(int accountId, int token)
        {
            // 1) Redis fast-path
            try
            {
                if (RedisDb != null)
                {
                    string key = $"acc:token:{accountId}";
                    var val = RedisDb.StringGet(key);
                    if (val.HasValue && int.TryParse(val.ToString(), out var cached) && cached == token)
                        return true;
                }
            }
            catch { /* 로그 옵션 */ }

            // 2) Fallback: SharedDB (만료 포함)
            try
            {
                if (SharedDbFactory != null)
                {
                    using var shared = SharedDbFactory();
                    var rec = shared.Tokens
                        .AsNoTracking()
                        .FirstOrDefault(t => t.AccountDbId == accountId && t.Expired > DateTime.UtcNow);

                    if (rec != null && rec.Token == token)
                        return true;
                }
            }
            catch { /* 로그 옵션 */ }

            return false;
        }
    }
}
