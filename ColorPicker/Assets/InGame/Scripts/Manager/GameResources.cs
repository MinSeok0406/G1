using ColorPicker.InGame;
using UnityEngine;
using UnityEngine.Rendering;

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
                    instance = Resources.Load<GameResources>("InGameResources");
                }
                return instance;
            }
        }

        #region Header Base Ref
        [Space(10)]
        [Header("Base Ref")]
        #endregion
        public GameObject lobbyPlayerPrefab;
        public GameObject playerPrefab;
        public GameObject playerDeathBodyPrefab;

        public GameObject mainCameraPrefab;

        #region Header Others Ref
        [Space(10)]
        [Header("Others Ref")]
        #endregion
        public GameObject chatContentPrefab;
        public GameObject myChatContentPrefab;

        #region Header Material Ref
        [Space(10)]
        [Header("Material Ref")]
        #endregion
        public Material outlineMaterial;
        public Material interactiveMaterial;

        [Header("VFX Ref")]
        public VolumeProfile deathVFXVolumeProfile;
    }
}