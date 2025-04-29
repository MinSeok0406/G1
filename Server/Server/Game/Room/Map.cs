using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Game
{
    public struct Vector2Float
    {
        public float x;
        public float y;

        public Vector2Float(float x, float y) { this.x = x; this.y = y; }

        public static Vector2Float operator +(Vector2Float a, Vector2Float b)
        {
            return new Vector2Float(a.x + b.x, a.y + b.y);
        }
    }

    public class Map
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }

        public float SizeX { get { return MaxX - MinX + 1; } }
        public float SizeY { get { return MaxY - MinY + 1; } }

        bool[,] _collision;
        GameObject[,] _objects;

        public bool CanGo(Vector2Float cellPos, bool checkObjects = true)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return false;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return false;

            float x = cellPos.x - MinX;
            float y = MaxY - cellPos.y;
            return !_collision[(int)y, (int)x] && (!checkObjects || _objects[(int)y, (int)x] == null);
        }

        public GameObject Find(Vector2Float cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return null;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return null;

            float x = cellPos.x - MinX;
            float y = MaxY - cellPos.y;
            return _objects[(int)y, (int)x];
        }

        public bool ApplyLeave(GameObject gameObject)
        {
            PositionInfo posInfo = gameObject.PosInfo;
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
                return false;

            {
                float x = posInfo.PosX - MinX;
                float y = MaxY - posInfo.PosY;
                if (_objects[(int)y, (int)x] == gameObject)
                    _objects[(int)y, (int)x] = null;
            }

            return true;
        }

        public bool ApplyMove(GameObject gameObject, Vector2Float dest)
        {
            ApplyLeave(gameObject);

            PositionInfo posInfo = gameObject.PosInfo;
            if (CanGo(dest, true) == false)
                return false;

            {
                float x = dest.x - MinX;
                float y = MaxY - dest.y;
                _objects[(int)y, (int)x] = gameObject;
            }

            // 실제 좌표 이동
            posInfo.PosX = dest.x;
            posInfo.PosY = dest.y;
            return true;
        }

        public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");

            // Collision 관련 파일
            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new StringReader(text);

            MinX = float.Parse(reader.ReadLine());
            MaxX = float.Parse(reader.ReadLine());
            MinY = float.Parse(reader.ReadLine());
            MaxY = float.Parse(reader.ReadLine());

            float xCount = MaxX - MinX + 1;
            float yCount = MaxY - MinY + 1;
            _collision = new bool[(int)yCount, (int)xCount];
            _objects = new GameObject[(int)yCount, (int)xCount];

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }
        }
    }
}
