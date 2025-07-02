using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        object _lock = new object();
        public int RoomId { get; set; }

        // 맵이 생긴다면 추가
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        //List<Player> _players = new List<Player>();
        public Map Map { get; private set; } = new Map();


        // 맵이 생긴다면 추가
        /*public void Init(int mapId)
        {
            Map.LoadMap(mapId);
        }*/

        public void Update()
        {
            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            Player player = gameObject as Player;
            _players.Add(gameObject.Id, player);
            player.Room = this;

            // List에 사용
            /*_players.Add(newPlayer);
            newPlayer.Room = this;*/

            // 본인한테 정보 전송
            {
                S_EnterGame enterPacket = new S_EnterGame();
                enterPacket.Player = player.Info;
                player.Session.Send(enterPacket);

                // Temp
                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players.Values)
                {
                    if (player != p)
                        spawnPacket.Objects.Add(p.Info);
                }
                player.Session.Send(spawnPacket);


                // List에 사용
                /*S_EnterGame enterPacket = new S_EnterGame();
                enterPacket.Player = newPlayer.Info;
                newPlayer.Session.Send(enterPacket);

                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players)
                {
                    if (newPlayer != p)
                        spawnPacket.Players.Add(p.Info);
                }

                newPlayer.Session.Send(spawnPacket);*/
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }

                /*S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Players.Add(newPlayer.Info);
                foreach (Player p in _players)
                {
                    if (newPlayer != p)
                    {
                        p.Session.Send(spawnPacket);
                    }
                }*/
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            Player player = null;
            if (_players.Remove(objectId, out player) == false)
                return;

            player.OnLeaveGame();
            player.Room = null;
            //Map.ApplyLeave(player);

            // 본인한테 정보 전송
            {
                S_LeaveGame leavePacket = new S_LeaveGame();
                player.Session.Send(leavePacket);
            }

            // List에 사용
            /*Player player = _players.Find(p => p.Info.PlayerId == playerId);
            if (player == null)
                return;

            _players.Remove(player);
            player.Room = null;

            // 본인한테 정보 전송
            {
                S_LeaveGame leavePacket = new S_LeaveGame();
                player.Session.Send(leavePacket);
            }*/

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }

                // List에 사용
                /*S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                foreach (Player p in _players)
                {
                    if (player != p)
                        p.Session.Send(despawnPacket);
                }*/
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // TODO : 검증
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;


            // 다른 좌표로 이동할 경우, 갈 수 있는지 체크
            // 맵이 나오고 LoadMap을 할 수 있으면 사용 가능
            //-------------------------------------------

            /*if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Float((int)movePosInfo.PosX, (int)movePosInfo.PosY)) == false)
                    return;
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Float((int)movePosInfo.PosX, (int)movePosInfo.PosY));*/
            //-------------------------------------------

            info.PosInfo = movePacket.PosInfo;
            //info.PosInfo.State = movePosInfo.State;
            //info.PosInfo.MoveDir = movePosInfo.MoveDir;

            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(resMovePacket);
        }

        public void HandleChat(Player player, C_Chat chatPacket)
        {
            if (player == null)
                return;

            if (chatPacket.Success == false)
                return;

            S_Chat schatPacket = new S_Chat();
            schatPacket.Type = chatPacket.Type;
            schatPacket.SenderId = player.Info.ObjectId;
            schatPacket.Name = player.Info.Name;
            schatPacket.Msg = chatPacket.Msg;

            if (schatPacket.Type == MessageType.Private)
            {
                Broadcast(schatPacket);
            }
            else if (schatPacket.Type == MessageType.Public)
            {
                Broadcast(schatPacket);
            }
            else if (schatPacket.Type == MessageType.System)
            {
                Broadcast(schatPacket);
            }

            Console.WriteLine($"{schatPacket.Msg}");

            chatPacket.Success = false;
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
