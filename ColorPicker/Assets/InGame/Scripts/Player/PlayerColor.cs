using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(PlayerDataChangedEvent))]
    [DisallowMultipleComponent]
    public class PlayerColor : MonoBehaviour
    {
        private PlayerDataChangedEvent playerDataChangedEvent;
        private Player player;

        private void Awake()
        {
            playerDataChangedEvent = GetComponent<PlayerDataChangedEvent>();
            player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            playerDataChangedEvent.OnPlayerDataChangeEvent += PlayerDataChangedEvent_OnPlayerDataChangeEvent;
        }

        private void OnDisable()
        {
            playerDataChangedEvent.OnPlayerDataChangeEvent += PlayerDataChangedEvent_OnPlayerDataChangeEvent;   
        }

        private void PlayerDataChangedEvent_OnPlayerDataChangeEvent(PlayerDataChangedEvent playerDataChangedEvent, PlayerDataChangedEventArgs playerDataChangedEventArgs)
        {
            ChangedColor(playerDataChangedEventArgs.playerData.playerColor);
        }

        private void ChangedColor(int playerColor)
        {
            player.spriteRenderer.color = HelperUtilities.GetUnityColor((CustomizationColor)playerColor);
        }
    }
}
