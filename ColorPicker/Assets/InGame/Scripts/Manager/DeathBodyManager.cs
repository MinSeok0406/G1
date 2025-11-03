using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 시체 오브젝트 관리 매니저
    /// 시체 생성, 등록, 제거를 담당
    /// </summary>
    public sealed class DeathBodyManager : SingletonNetworkBehaviour<DeathBodyManager>
    {
        private readonly List<GameObject> _deathBodies = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 시체 등록
        /// </summary>
        public void RegisterDeathBody(GameObject deathBody)
        {
            if (deathBody == null)
            {
                Debug.LogWarning("[DeathBodyManager] Null death body cannot be registered.");
                return;
            }

            if (!_deathBodies.Contains(deathBody))
            {
                _deathBodies.Add(deathBody);
                Debug.Log($"[DeathBodyManager] Registered death body. Total: {_deathBodies.Count}");
            }
        }

        /// <summary>
        /// [Host Only] 모든 시체 제거
        /// </summary>
        public void ClearAllDeathBodies()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[DeathBodyManager] ClearAllDeathBodies is host-only.");
                return;
            }

            photonView.RPC(nameof(RPC_ClearAllDeathBodies), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_ClearAllDeathBodies()
        {
            // 역순으로 제거하여 리스트 인덱스 오류 방지
            for (int i = _deathBodies.Count - 1; i >= 0; i--)
            {
                var body = _deathBodies[i];
                if (body != null)
                {
                    // Photon으로 생성된 객체는 PhotonNetwork.Destroy 사용
                    var pv = body.GetComponent<PhotonView>();
                    if (pv != null && PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(body);
                    }
                    else if (pv == null)
                    {
                        Destroy(body);
                    }
                }
            }

            _deathBodies.Clear();
            Debug.Log("[DeathBodyManager] All death bodies cleared.");
        }

        /// <summary>
        /// 시체 등록 해제 (개별 제거 시 사용)
        /// </summary>
        public void UnregisterDeathBody(GameObject deathBody)
        {
            if (deathBody != null && _deathBodies.Contains(deathBody))
            {
                _deathBodies.Remove(deathBody);
                Debug.Log($"[DeathBodyManager] Unregistered death body. Remaining: {_deathBodies.Count}");
            }
        }

        /// <summary>
        /// 현재 등록된 시체 수
        /// </summary>
        public int GetDeathBodyCount()
        {
            return _deathBodies.Count;
        }
    }
}
