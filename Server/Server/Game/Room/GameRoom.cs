﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public const int VisionCells = 5;
		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();

		public Zone[,] Zones { get; private set; }
		public float ZoneCells { get; private set; }

		public Map Map { get; private set; } = new Map();

		/*public Zone GetZone(Vector2Float cellPos)
		{
			float x = (cellPos.x - Map.MinX) / ZoneCells;
			float y = (Map.MaxY - cellPos.y) / ZoneCells;

			return GetZone(y, x);
        }*/

		public Zone GetZone(int indexY, int indexX)
		{
            if (indexX < 0 || indexX >= Zones.GetLength(1))
                return null;

            if (indexY < 0 || indexY >= Zones.GetLength(0))
                return null;

			return Zones[indexY, indexX];
        }

		public void Init(int mapId, int zoneCells)
		{
			/*Map.LoadMap(mapId);

			// Zone
			ZoneCells = zoneCells;
			int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
			int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
			Zones = new Zone[countY, countX];
			for (int y = 0; y < countY; y++)
			{
				for (int x = 0; x < countX; x++)
				{
					Zones[y, x] = new Zone(y, x);
				}
			}*/
		}

		// 누군가 주기적으로 호출해줘야 한다
		public void Update()
		{
			Flush();
		}

		Random _rand = new Random();
		public void EnterGame(GameObject gameObject, bool randomPos)
		{
			if (gameObject == null)
				return;

			if (randomPos)
			{
                Vector2Float respawnPos;
                while (true)
                {
                    respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
                    respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);

                    if (Map.Find(respawnPos) == null)
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }

			GameObjectType type = GameObjectType.Player;

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				//_players.Add(gameObject.Id, player);
				player.Room = this;

				Map.ApplyMove(player, new Vector2Float(player.CellPos.x, player.CellPos.y));

				//GetZone(player.CellPos).Players.Add(player);

				// 본인한테 정보 전송
				{
					/*S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = player.Info;
					player.Session.Send(enterPacket);

					player.Vision.Update();*/
				}
			}

			// 타인한테 정보 전송
			{
				/*S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				Broadcast(gameObject.CellPos, spawnPacket);*/
			}
		}

		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			Vector2Float cellPos;

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				cellPos = player.CellPos;

                player.OnLeaveGame();
				Map.ApplyLeave(player);
				player.Room = null;

				// 본인한테 정보 전송
				{
					/*S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);*/
				}
			}
			else
			{
				return;
			}

            // 타인한테 정보 전송
            {
                /*S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(cellPos, despawnPacket);*/
            }
        }

		Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		// 살짝 부담스러운 함수 -> 너무 많은 시간 복잡도를 가짐
		/*public Player FindClosestPlayer(Vector2Float pos, int range)
		{
			List<Player> players = GetAdjacentPlayers(pos, range);

			players.Sort((left, right) =>
			{
				float leftDist = (left.CellPos - pos).cellDistFromZero;
				float rightDist = (right.CellPos - pos).cellDistFromZero;
				return leftDist - rightDist;
			});

			foreach (Player player in players)
			{
                List<Vector2Float> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
				if (path.Count < 2 || path.Count > range)
					continue;

				return player;
            }

			return null;
		}*/

		/*public void Broadcast(Vector2Float pos, IMessage packet)
		{
			List<Zone> zones = GetAdjacentZones(pos);
			
			// 이중 for문을 사용하기 싫어서 찾아낸 대안
			foreach (Player p in zones.SelectMany(z => z.Players))
			{
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if (Math.Abs(dy) > GameRoom.VisionCells)
                    continue;

                p.Session.Send(packet);
			}
		}*/

		/*public List<Player> GetAdjacentPlayers(Vector2Float pos, int range)
		{
			List<Zone> zones = GetAdjacentZones(pos, range);
			return zones.SelectMany(z => z.Players).ToList();
		}*/

		/*public List<Zone> GetAdjacentZones(Vector2Float cellPos, int range = GameRoom.VisionCells)
		{
			HashSet<Zone> zones = new HashSet<Zone>();

			float maxY = cellPos.y + range;
			float minY = cellPos.y - range;
            float maxX = cellPos.x + range;
            float minX = cellPos.x - range;

			// 좌측 상단
			Vector2Float leftTop = new Vector2Float(minX, maxY);
			float minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
			float minIndexX = (leftTop.x - Map.MinX) / ZoneCells;

			// 우측 하단
			Vector2Float rightBot = new Vector2Float(maxX, minY);
            float maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;
            float maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;

			for (float x = minIndexX; x <= maxIndexX; x++)
			{
				for (float y = minIndexY; y <= maxIndexY; y++)
				{
					Zone zone = GetZone(y, x);
					if (zone == null)
						continue;

					zones.Add(zone);
				}
			}

            int[] delta = new int[2] { -range, +range };
			foreach (int dy in delta)
			{
				foreach (int dx in delta)
				{
					int y = cellPos.y + dy;
					int x = cellPos.x + dx;
					Zone zone = GetZone(new Vector2Float(x, y));
					if (zone == null)
						continue;

					zones.Add(zone);
				}
			}

			return zones.ToList();
		}*/
	}
}
