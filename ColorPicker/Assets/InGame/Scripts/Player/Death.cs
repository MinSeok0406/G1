
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
            deathEvent.OnDeathEvent += DeathEvent_OnDeathEvent; 
        }

        private void DeathEvent_OnDeathEvent(DeathEvent obj)
        {
            if (!player.photonView.IsMine)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Camera.main.GetComponent<Volume>().profile = GameResources.Instance.deathVFXVolumeProfile;
                AbilityManager.Instance.abilityBinder.DisableAllAbilities();
            }

            if (!PhotonNetwork.IsMasterClient) return;

            PhotonNetwork.Instantiate(GameResources.Instance.playerDeathBodyPrefab.name, transform.position, Quaternion.identity);
            //GameDataManager.Instance.TryGetPlayerDataByActorId(player.photonView.Owner.ActorNumber, out PlayerData playerData);

            //InGameData data = GameDataManager.Instance.GetInGameData(player.photonView);

            //data.isAlive = false;
        }
    }
}
