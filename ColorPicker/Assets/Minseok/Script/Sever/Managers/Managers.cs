using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Minseok
{
    public class Managers : MonoBehaviour
    {
        static Managers s_instance; // 유일성이 보장된다
        public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

        #region Contents
        MapManager _map = new MapManager();
        NetworkManager _network = new NetworkManager();
        ObjectManager _obj = new ObjectManager();
        WebManager _web = new WebManager();

        public static MapManager Map { get { return Instance._map; } }
        public static ObjectManager Object { get { return Instance._obj; } }
        public static NetworkManager Network { get { return Instance._network; } }
        public static WebManager Web { get { return Instance._web; } }
        #endregion

        #region Core
        ResourceManager _resource = new ResourceManager();
        PoolManager _pool = new PoolManager();
        SceneManagerEx _scene = new SceneManagerEx();
        UI_Manager _ui = new UI_Manager();
        GameManager _gm = new GameManager();

        public static PoolManager Pool { get { return Instance._pool; } }
        public static SceneManagerEx Scene { get { return Instance._scene; } }
        public static ResourceManager Resource { get { return Instance._resource; } }
        public static UI_Manager UI { get { return Instance._ui; } }
        public static GameManager Game { get { return Instance._gm; } }
        #endregion

        void Start()
        {
            Init();
        }

        void Update()
        {
            _network.Update();
        }

        static void Init()
        {
            if (s_instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject { name = "@Managers" };
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);
                s_instance = go.GetComponent<Managers>();

                s_instance.Update();
                s_instance._pool.Init();
            }
        }

        public static void Clear()
        {
            Pool.Clear();
            Scene.Clear();
        }
    }
}