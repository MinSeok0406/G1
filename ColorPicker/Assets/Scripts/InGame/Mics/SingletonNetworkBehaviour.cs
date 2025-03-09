using Mirror;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T instance;

        [SerializeField] private bool isDontDestroy = false;

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }

            if (isDontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
