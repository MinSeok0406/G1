﻿using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Game
{
	public struct Pos
	{
		public Pos(float y, float x) { Y = y; X = x; }
		public float Y;
		public float X;

		public static bool operator==(Pos lhs, Pos rhs)
		{
			return lhs.Y == rhs.Y && lhs.X == rhs.X;
		}

        public static bool operator!=(Pos lhs, Pos rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
			return (Pos)obj == this;
        }

        /*public override int GetHashCode()
        {
			long value = (Y << 32.0f) | X;
            return value.GetHashCode();
        }*/

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Y, X);
        }
    }

	public struct PQNode : IComparable<PQNode>
	{
		public float F;
		public float G;
		public float Y;
		public float X;

		public int CompareTo(PQNode other)
		{
			if (F == other.F)
				return 0;
			return F < other.F ? 1 : -1;
		}
	}

	public struct Vector2Float
	{
		public float x;
		public float y;

		public Vector2Float(float x, float y) { this.x = x; this.y = y; }

		public static Vector2Float up { get { return new Vector2Float(0.0f, 1.0f); } }
		public static Vector2Float down { get { return new Vector2Float(0.0f, -1.0f); } }
		public static Vector2Float left { get { return new Vector2Float(-1.0f, 0.0f); } }
		public static Vector2Float right { get { return new Vector2Float(1.0f, 0.0f); } }

		public static Vector2Float operator+(Vector2Float a, Vector2Float b)
		{
			return new Vector2Float(a.x + b.x, a.y + b.y);
		}

		public static Vector2Float operator -(Vector2Float a, Vector2Float b)
		{
			return new Vector2Float(a.x - b.x, a.y - b.y);
		}

		public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
		public float sqrMagnitude { get { return (x * x + y * y); } }
		public float cellDistFromZero { get { return Math.Abs(x) + Math.Abs(y); } }
	}

	public class Map
	{
		public int MinX { get; set; }
		public int MaxX { get; set; }
		public int MinY { get; set; }
		public int MaxY { get; set; }

		public int SizeX { get { return MaxX - MinX + 1; } }
		public int SizeY { get { return MaxY - MinY + 1; } }

		//bool[,] _collision;
		GameObject _objects;

		public bool CanGo(Vector2Float cellPos, bool checkObjects = true)
		{
			if (cellPos.x < MinX || cellPos.x > MaxX)
				return false;
			if (cellPos.y < MinY || cellPos.y > MaxY)
				return false;

			float x = cellPos.x - MinX;
			float y = MaxY - cellPos.y;
			return true;
			//!_collision[y, x] && (!checkObjects || _objects[y, x] == null);
		}

		public GameObject Find(Vector2Float cellPos)
		{
			if (cellPos.x < MinX || cellPos.x > MaxX)
				return null;
			if (cellPos.y < MinY || cellPos.y > MaxY)
				return null;

			float x = cellPos.x - MinX;
			float y = MaxY - cellPos.y;
			return _objects;
		}

		public bool ApplyLeave(GameObject gameObject)
		{
			if (gameObject.Room == null)
				return false;
			if (gameObject.Room.Map != this)
				return false;

			PositionInfo posInfo = gameObject.PosInfo;
			if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
				return false;
			if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
				return false;

			// Zone
			/*Zone zone = gameObject.Room.GetZone(gameObject.CellPos);
			zone.Remove(gameObject);*/

			// TODO
			{
				float x = posInfo.PosX - MinX;
                float y = MaxY - posInfo.PosY;
				if (_objects == gameObject)
					_objects = null;
			}

			return true;
		}

		public bool ApplyMove(GameObject gameObject, Vector2Float dest, bool checkObjects = true, bool collision = true)
		{
			// 예외 처리
			if (gameObject.Room == null)
				return false;
			if (gameObject.Room.Map != this)
				return false;

			PositionInfo posInfo = gameObject.PosInfo;

			// 목적지에 갈 수 있는지 체크
			if (CanGo(dest, checkObjects) == false)
				return false;

			// 목적지에 나를 텔레포트 시킴
			if (collision)
			{
				// TODO
                {
                    float x = posInfo.PosX - MinX;
                    float y = MaxY - posInfo.PosY;
                    if (_objects == gameObject)
                        _objects = null;
                }

				{
                    float x = dest.x - MinX;
                    float y = MaxY - dest.y;
                    _objects = gameObject;
                }
			}
			
			// Zone
			/*GameObjectType type = GameObjectType.Player;
			if (type == GameObjectType.Player)
			{
                Player player = (Player)gameObject;

                Zone now = gameObject.Room.GetZone(gameObject.CellPos);
                Zone after = gameObject.Room.GetZone(dest);
                if (now != after)
                {
                    now.Players.Remove(player);
                    now.Players.Add(player);
                }
            }*/

			// 실제 좌표 이동
			posInfo.PosX = dest.x;
			posInfo.PosY = dest.y;
			return true;
		}

		// 문제 가능성 있음
		/*public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
		{
			string mapName = "Map_" + mapId.ToString("000");

			// Collision 관련 파일
			string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
			StringReader reader = new StringReader(text);

			MinX = int.Parse(reader.ReadLine());
			MaxX = int.Parse(reader.ReadLine());
			MinY = int.Parse(reader.ReadLine());
			MaxY = int.Parse(reader.ReadLine());

			int xCount = MaxX - MinX + 1;
			int yCount = MaxY - MinY + 1;
			_collision = new bool[yCount, xCount];
			_objects = new GameObject[yCount, xCount];

			for (int y = 0; y < yCount; y++)
			{
				string line = reader.ReadLine();
				for (int x = 0; x < xCount; x++)
				{
					_collision[y, x] = (line[x] == '1' ? true : false);
				}
			}
		}*/

		#region A* PathFinding

		// U D L R
		int[] _deltaY = new int[] { 1, -1, 0, 0 };
		int[] _deltaX = new int[] { 0, 0, -1, 1 };
		int[] _cost = new int[] { 10, 10, 10, 10 };

		public List<Vector2Float> FindPath(Vector2Float startCellPos, Vector2Float destCellPos, bool checkObjects = true, int maxDist = 10)
		{
			List<Pos> path = new List<Pos>();

			// 점수 매기기
			// F = G + H
			// F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
			// G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
			// H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

			// (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
			HashSet<Pos> closeList = new HashSet<Pos>();	// CloseList

            // (y, x) 가는 길을 한 번이라도 발견했는지
            // 발견X => MaxValue
            // 발견O => F = G + H
			Dictionary<Pos, float> openList = new Dictionary<Pos, float>();	// OpenList

			// 첫 번째 인자값의 키 값을 가져와서 두 번째 인자값이 부모 값으로 설정
			Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

			// 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
			PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

			// CellPos -> ArrayPos
			Pos pos = Cell2Pos(startCellPos);
			Pos dest = Cell2Pos(destCellPos);

			// 시작점 발견 (예약 진행)
			openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));

			pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
			parent.Add(pos, pos);

			while (pq.Count > 0)
			{
				// 제일 좋은 후보를 찾는다
				PQNode pqNode = pq.Pop();
				Pos node = new Pos(pqNode.Y, pqNode.X);
				// 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
				if (closeList.Contains(node))
					continue;

				// 방문한다
				closeList.Add(node);

				// 목적지 도착했으면 바로 종료
				if (node.Y == dest.Y && node.X == dest.X)
					break;

				// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
				for (int i = 0; i < _deltaY.Length; i++)
				{
					Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

					// 너무 멀면 스킵
					if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
						continue;

					// 유효 범위를 벗어났으면 스킵
					// 벽으로 막혀서 갈 수 없으면 스킵
					if (next.Y != dest.Y || next.X != dest.X)
					{
						if (CanGo(Pos2Cell(next), checkObjects) == false) // CellPos
							continue;
					}

					// 이미 방문한 곳이면 스킵
					if (closeList.Contains(next))
						continue;

					// 비용 계산
					float g = 0.0f;// node.G + _cost[i];
					float h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));

					// 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
					float value = 0;
					if (openList.TryGetValue(next, out value) == false)
                        value = Int32.MaxValue;

                    if (value < g + h)
						continue;

					// 예약 진행
					if (openList.TryAdd(next, g + h) == false)
						openList[next] = g + h;

					pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });

					if (parent.TryAdd(next, node) == false)
						parent[next] = node;
				}
			}

			return CalcCellPathFromParent(parent, dest);
		}

		List<Vector2Float> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
		{
			List<Vector2Float> cells = new List<Vector2Float>();

			if (parent.ContainsKey(dest) == false)
			{
				Pos best = new Pos();
				float bestDist = Int32.MaxValue;

				foreach (Pos pos in parent.Keys)
				{
					float dist = Math.Abs(dest.X - pos.X) + Math.Abs(dest.Y - pos.Y);
					// 제일 우수한 후보를 뽑는다
					if (dist < bestDist)
					{
						best = pos;
						bestDist = dist;
					}
				}

				dest = best;
            }

			{
                Pos pos = dest;
                while (parent[pos] != pos)
                {
                    cells.Add(Pos2Cell(pos));
                    pos = parent[pos];
                }
                cells.Add(Pos2Cell(pos));
                cells.Reverse();
            }

			return cells;
		}

		Pos Cell2Pos(Vector2Float cell)
		{
			// CellPos -> ArrayPos
			return new Pos(MaxY - cell.y, cell.x - MinX);
		}

		Vector2Float Pos2Cell(Pos pos)
		{
			// ArrayPos -> CellPos
			return new Vector2Float(pos.X + MinX, MaxY - pos.Y);
		}

		#endregion
	}

}
