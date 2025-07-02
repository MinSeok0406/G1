
using Photon.Pun;
using UnityEngine;

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
            deathEvent.OnDeathEvent += DeathEvent_OnDeathEvent; 
        }

        private void DeathEvent_OnDeathEvent(DeathEvent obj)
        {
            PhotonNetwork.Instantiate(GameResources.Instance.playerDeathBodyPrefab.name, transform.position, Quaternion.identity);

            if (!player.photonView.IsMine)
            {
                gameObject.SetActive(false);
            }

            if (!PhotonNetwork.IsMasterClient) return;

            //GameDataManager.Instance.TryGetPlayerDataByActorId(player.photonView.Owner.ActorNumber, out PlayerData playerData);

            //InGameData data = GameDataManager.Instance.GetInGameData(player.photonView);

            //data.isAlive = false;
        }
    }
}
