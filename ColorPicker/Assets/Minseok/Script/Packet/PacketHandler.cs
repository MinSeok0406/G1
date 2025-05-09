using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;

namespace Minseok
{
    class PacketHandler
    {
        private static int count = 0;
        public static void S_PingHandler(PacketSession session, IMessage packet)
        {
            C_Pong pongPacket = new C_Pong();
            //Debug.Log("[Server] PingCheck");
            //Managers.Network.Send(pongPacket);
        }

        public static void S_EnterGameHandler(PacketSession session, IMessage packet)
        {
            S_EnterGame enterGamePacket = (S_EnterGame)packet;
            Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
        }

        public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
        {
            S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
            Managers.Object.RemoveMyPlayer();
        }

        public static void S_SpawnHandler(PacketSession session, IMessage packet)
        {
            S_Spawn spawnPacket = (S_Spawn)packet;
            foreach (ObjectInfo obj in spawnPacket.Objects)
            {
                Managers.Object.Add(obj, myPlayer: false);
            }
        }

        public static void S_DespawnHandler(PacketSession session, IMessage packet)
        {
            S_Despawn despawnPacket = packet as S_Despawn;
            foreach (int id in despawnPacket.ObjectIds)
            {
                Managers.Object.Remove(id);
            }
        }

        public static void S_MoveHandler(PacketSession session, IMessage packet)
        {
            S_Move movePacket = packet as S_Move;

            GameObject go = Managers.Object.FindById(movePacket.ObjectId);
            if (go == null)
                return;

            PlayerControl pc = go.GetComponent<PlayerControl>();
            if (pc == null)
                return;

            pc.PosInfo = movePacket.PosInfo;
        }

        public static void S_ConnectedHandler(PacketSession session, IMessage packet)
        {
            Debug.Log("S_ConnectedHandler");
            C_Login loginPacket = new C_Login();
            loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;
            Managers.Network.Send(loginPacket);
        }

        public static void S_LoginHandler(PacketSession session, IMessage packet)
        {
            S_Login loginPacket = (S_Login)packet;
            Debug.Log($"LoginOk({loginPacket.LoginOk})");

            // TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록
            if (loginPacket.Players == null || loginPacket.Players.Count == 0)
            {
                C_CreatePlayer createPacket = new C_CreatePlayer();
                createPacket.Name = $"Player_{count++}";
                Managers.Network.Send(createPacket);
            }
            else
            {
                // 무조건 첫번째 로그인
                LobbyPlayerInfo info = loginPacket.Players[0];
                C_EnterGame enterGamePacket = new C_EnterGame();
                enterGamePacket.Name = info.Name;
                enterGamePacket.Speed = info.Speed;
                Managers.Network.Send(enterGamePacket);
            }
        }

        public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
        {
            S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

            if (createOkPacket.Player == null)
            {
                C_CreatePlayer createPacket = new C_CreatePlayer();
                createPacket.Name = $"Player_{count++}";
                Managers.Network.Send(createPacket);
            }
            else
            {
                C_EnterGame enterGamePacket = new C_EnterGame();
                enterGamePacket.Name = createOkPacket.Player.Name;
                enterGamePacket.Speed = createOkPacket.Player.Speed;
                Managers.Network.Send(enterGamePacket);
            }
        }

        public static void S_ChatHandler(PacketSession session, IMessage packet)
        {
            S_Chat chatPacket = (S_Chat)packet;

            UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_ChatScene chatUI = gameSceneUI.ChatUI;

            chatUI.ReadChat(chatPacket);
        }

        public static void S_AchievementListHandler(PacketSession session, IMessage packet)
        {
            S_AchievementList AchievementPacket = (S_AchievementList)packet;

            /*foreach (AchievementInfo achievement in AchievementPacket.Achievements)
            {
                Debug.Log($"{achievement.AchievementDbId}, {achievement.Locked}");
            }*/
        }
    }
}
