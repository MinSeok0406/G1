using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace Minseok
{
    public class MapManager
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }

        public float SizeX { get { return MaxX - MinX + 1; } }
        public float SizeY { get { return MaxY - MinY + 1; } }

        bool[,] _collision;

        public bool CanGo(Vector3Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return false;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return false;

            float x = cellPos.x - MinX;
            float y = MaxY - cellPos.y;
            return !_collision[(int)y, (int)x];
        }

        public void LoadMap(int mapId)
        {
            DestroyMap();

            string mapName = "Map_" + mapId.ToString("000");
            GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
            go.name = "Map";

            GameObject collision = Util.FindChild(go, "Tilemap_Collision", true);
            if (collision != null)
                collision.SetActive(false);

            // Collision 관련 파일
            TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
            StringReader reader = new StringReader(txt.text);

            MinX = float.Parse(reader.ReadLine());
            MaxX = float.Parse(reader.ReadLine());
            MinY = float.Parse(reader.ReadLine());
            MaxY = float.Parse(reader.ReadLine());

            float xCount = MaxX - MinX + 1;
            float yCount = MaxY - MinY + 1;
            _collision = new bool[(int)yCount, (int)xCount];

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }
        }

        public void DestroyMap()
        {
            GameObject map = GameObject.Find("Map");
            if (map != null)
            {
                GameObject.Destroy(map);
            }
        }
    }
}