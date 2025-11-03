
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(DeathEvent))]
    [DisallowMultipleComponent]
    public class Death : MonoBehaviour
    {
        private DeathEvent deathEvent;
        private Player player;

        private void Awake()
        {
            deathEvent = GetComponent<DeathEvent>();
            player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            deathEvent.OnDeathEvent += DeathEvent_OnDeathEvent; 
        }

        private void OnDisable()
        {
            deathEvent.OnDeathEvent -= DeathEvent_OnDeathEvent;
        }

        private void DeathEvent_OnDeathEvent(DeathEvent obj)
        {
            if (!player.photonView.IsMine)
            {
                // 다른 플레이어가 죽었을 때는 아무것도 하지 않음
                // Player.cs의 RPC_UpdateGhostVisibility가 모든 가시성을 처리함
                return;
            }

            // 내가 죽었을 때
            if (Camera.main != null)
            {
                var volume = Camera.main.GetComponent<Volume>();
                if (volume != null && GameResources.Instance != null && GameResources.Instance.deathVFXVolumeProfile != null)
                {
                    volume.profile = GameResources.Instance.deathVFXVolumeProfile;
                }
            }

            if (AbilityManager.Instance != null && AbilityManager.Instance.abilityBinder != null)
            {
                AbilityManager.Instance.abilityBinder.DisableAllAbilities();
            }

            if (!PhotonNetwork.IsMasterClient) return;

            // 시체 생성 및 DeathBodyManager에 등록
            var deathBody = PhotonNetwork.Instantiate(
                GameResources.Instance.playerDeathBodyPrefab.name,
                transform.position,
                Quaternion.identity);

            if (deathBody != null)
            {
                if (DeathBodyManager.Instance != null)
                {
                    DeathBodyManager.Instance.RegisterDeathBody(deathBody);
                }
            }
        }
    }
}
