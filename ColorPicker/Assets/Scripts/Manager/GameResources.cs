using UnityEngine;

namespace ColorPicker
{
    public class GameResources : MonoBehaviour
    {
        private static GameResources instance;

        public static GameResources Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GameResources>("GameResources");
                }
                return instance;
            }
        }

        #region Header Base Ref
        [Space(10)]
        [Header("Base Ref")]
        #endregion
        public GameObject playerPrefab;

        #region Header Others Ref
        [Space(10)]
        [Header("Others Ref")]
        #endregion
        public GameObject chatContentPrefab;

    }
}