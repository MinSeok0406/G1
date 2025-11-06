
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(DeathEvent))]
    [RequireComponent(typeof(Player))]
    [DisallowMultipleComponent]
    public class Death : MonoBehaviour
    {
        private DeathEvent deathEvent;
        private Player player;
        private PlayerControl playerControl;

        // WaitForSeconds 캐싱 (GC 최적화)
        private WaitForSeconds waitForDeathAnimation;

        private void Awake()
        {
            deathEvent = GetComponent<DeathEvent>();
            player = GetComponent<Player>();
            playerControl = GetComponent<PlayerControl>();

            // WaitForSeconds 캐싱
            waitForDeathAnimation = new WaitForSeconds(1f);
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
            // 호스트만 시체 생성
            if (PhotonNetwork.IsMasterClient)
            {
                var deathBody = PhotonNetwork.Instantiate(
                    GameResources.Instance.playerDeathBodyPrefab.name,
                    transform.position,
                    Quaternion.identity);

                if (deathBody != null && DeathBodyManager.Instance != null)
                {
                    DeathBodyManager.Instance.RegisterDeathBody(deathBody);
                }
            }

            // 죽은 플레이어가 내 캐릭터인지 확인
            bool isMyCharacter = player.photonView != null && player.photonView.IsMine;

            if (isMyCharacter)
            {
                // === 내가 죽었을 때 ===
                HandleMyDeath();
            }
            else
            {
                // === 다른 사람이 죽었을 때 ===
                HandleOtherPlayerDeath();

                // 승리 조건 체크 (호스트만)
                if (PhotonNetwork.IsMasterClient && VictoryConditionManager.Instance != null)
                {
                    VictoryConditionManager.Instance.CheckVictoryConditionOnDeath();
                }
            }
        }

        /// <summary>
        /// 내 캐릭터가 죽었을 때 처리
        /// </summary>
        private void HandleMyDeath()
        {
            Debug.Log("[Death] My character died - showing all players");

            // 모든 플레이어를 볼 수 있도록 활성화
            ShowAllPlayers();

            // 카메라 VFX 효과 적용
            if (Camera.main != null)
            {
                var volume = Camera.main.GetComponent<Volume>();
                if (volume != null && GameResources.Instance != null && GameResources.Instance.deathVFXVolumeProfile != null)
                {
                    volume.profile = GameResources.Instance.deathVFXVolumeProfile;
                }
            }

            // 능력 비활성화
            if (AbilityManager.Instance != null && AbilityManager.Instance.abilityBinder != null)
            {
                AbilityManager.Instance.abilityBinder.DisableAllAbilities();
            }

            // 승리 조건 체크 (호스트만)
            if (PhotonNetwork.IsMasterClient && VictoryConditionManager.Instance != null)
            {
                VictoryConditionManager.Instance.CheckVictoryConditionOnDeath();
            }
        }

        /// <summary>
        /// 다른 플레이어가 죽었을 때 처리
        /// </summary>
        private void HandleOtherPlayerDeath()
        {
            // 내가 살아있는지 확인
            GameDataManager.Instance.RequestPlayerAliveStates((aliveStates) =>
            {
                int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;

                // 내가 살아있는지 확인 (안전하게)
                if (aliveStates.TryGetValue(myActorId, out bool isAlive) && isAlive)
                {
                    // 내가 살아있으면 죽은 플레이어를 숨김 (애니메이션 재생을 위해 지연 후 비활성화)
                    StartCoroutine(HideDeadPlayerAfterAnimation());
                    Debug.Log($"[Death] Other player died - will hide after death animation (I'm alive)");
                }
                else
                {
                    // 내가 이미 죽었으면 모든 플레이어를 볼 수 있으므로 활성화 유지
                    Debug.Log($"[Death] Other player died - keeping visible (I'm dead)");
                }
            });
        }

        /// <summary>
        /// 죽음 애니메이션 재생 후 플레이어를 숨김
        /// </summary>
        private System.Collections.IEnumerator HideDeadPlayerAfterAnimation()
        {
            // 죽음 애니메이션이 재생될 시간을 줌 (캐싱된 WaitForSeconds 사용)
            yield return waitForDeathAnimation;

            // 컨트롤 비활성화 (움직임 방지) - 캐싱된 참조 사용
            if (playerControl != null)
            {
                playerControl.SetControlEnabled(false);
            }

            // 렌더러만 비활성화 (애니메이터는 유지)
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            // Collider 비활성화 (상호작용 방지)
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            Debug.Log($"[Death] Dead player hidden after animation");
        }

        private void ShowAllPlayers()
        {
            // 먼저 생사여부 조회
            GameDataManager.Instance.RequestPlayerAliveStates((aliveStates) =>
            {
                // 각 플레이어의 PhotonView를 가져와서 활성화
                foreach (var kv in aliveStates)
                {
                    int actorId = kv.Key;

                    GameDataManager.Instance.RequestPhotonViewByActorId(actorId, (photonView) =>
                    {
                        if (photonView != null && !photonView.gameObject.activeSelf)
                        {
                            photonView.gameObject.SetActive(true);
                            Debug.Log($"Activated player actor {actorId}");
                        }
                    });
                }
            });
        }
    }
}
