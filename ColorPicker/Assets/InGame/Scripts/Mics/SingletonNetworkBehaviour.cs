using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class SingletonNetworkBehaviour<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
    {
            private static T instance;


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
        }
    }
}
