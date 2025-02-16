using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		/*public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}*/

		public GameRoom Room { get; set; }

		//public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();

		/*public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}*/

		public MoveDir Dir
		{
			get { return PosInfo.MoveDir; }
			set { PosInfo.MoveDir = value; }
		}

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}

		public virtual void Update()
		{

		}

		public Vector2Float CellPos
		{
			get
			{
				return new Vector2Float(PosInfo.PosX, PosInfo.PosY);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
			}
		}

		public Vector2Float GetFrontCellPos()
		{
			return GetFrontCellPos(PosInfo.MoveDir);
		}

		public Vector2Float GetFrontCellPos(MoveDir dir)
		{
			Vector2Float cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos += Vector2Float.up;
					break;
				case MoveDir.Down:
					cellPos += Vector2Float.down;
					break;
				case MoveDir.Left:
					cellPos += Vector2Float.left;
					break;
				case MoveDir.Right:
					cellPos += Vector2Float.right;
					break;
			}

			return cellPos;
		}

		public static MoveDir GetDirFromVec(Vector2Float dir)
		{
			if (dir.x > 0)
				return MoveDir.Right;
			else if (dir.x < 0)
				return MoveDir.Left;
			else if (dir.y > 0)
				return MoveDir.Up;
			else
				return MoveDir.Down;
		}

		public virtual void OnDamaged(GameObject attacker, int damage)
		{
			/*if (Room == null)
				return;

			damage = Math.Max(damage - TotalDefence, 0);
			Stat.Hp = Math.Max(Stat.Hp - damage, 0);

			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			Room.Broadcast(CellPos, changePacket);

			if (Stat.Hp <= 0)
			{
				OnDead(attacker);
			}*/
		}

		public virtual void OnDead(GameObject attacker)
		{
			/*if (Room == null)
				return;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(CellPos, diePacket);

			GameRoom room = Room;
			room.LeaveGame(Id);

			Stat.Hp = Stat.MaxHp;
			PosInfo.State = CreatureState.Idle;
			PosInfo.MoveDir = MoveDir.Down;

			room.EnterGame(this, randomPos: true);*/
		}

		public virtual GameObject GetOwner()
		{
			return this;
		}
	}
}
