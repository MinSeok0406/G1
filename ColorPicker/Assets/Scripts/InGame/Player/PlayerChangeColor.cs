using ColorPicker.Server;
using Mirror;
using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    #region RequireComponent
    [RequireComponent(typeof(ColorChangedEvent))]
    #endregion
    [DisallowMultipleComponent]
    public class PlayerChangeColor : MonoBehaviour
    {
        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void Start()
        {
            Initialized();
        }


        private void OnEnable()
        {
            player.colorChangedEvent.OnColorChagned += ColorChangedEvent_OnColorChagned;
        }

        private void OnDisable()
        {
            player.colorChangedEvent.OnColorChagned -= ColorChangedEvent_OnColorChagned;
        }

        private void ColorChangedEvent_OnColorChagned(ColorChangedEvent colorChangedEvent, ColorChangedEventArgs colorChangedEventArgs)
        {
            player.SetPlayerColor_Hook(player.playerColorType, colorChangedEventArgs.color);
        }

        public void Initialized()
        {
            InitializedColor();

            player.SetPlayerColor_Hook(ColorType.White, player.playerColorType);
        }

        public void SetPlayerColor(ColorType oldColor, ColorType newColor)
        {
            player.spriteRenderer.material.color = HelperUtilities.GetUnityColor(newColor);
        }

        private void InitializedColor()
        {
            var roomSlots = (NetworkManager.singleton as RoomManager).roomSlots;

            ColorType color = ColorType.White;

            int count = Enum.GetValues(typeof(ColorType)).Length;

            for(int i =0; i < count; i++)
            {
                bool isFindSameColor = false;

                foreach(var roomPlayer in roomSlots)
                {
                    var user = roomPlayer as Player;
                    if (user.playerColorType == (ColorType)i && roomPlayer.netId != player.netId)
                    {
                        isFindSameColor = true;
                        break;
                    }
                }

                if (!isFindSameColor)
                {
                    if(Enum.IsDefined(typeof(ColorType), i))
                    {
                        color = (ColorType)i;
                        break;
                    }
                    else
                    {
                        Debug.Log("Is not defined color from colorType enum class");
                        break;
                    }
                }
            }

            player.playerColorType = color;
        }


    }
}
