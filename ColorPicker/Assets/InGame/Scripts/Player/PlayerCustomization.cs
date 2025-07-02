using ColorPicker.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Google.Protobuf.WellKnownTypes.Field.Types;

namespace ColorPicker.InGame
{
    public class PlayerCustomization : MonoBehaviour
    {
        [SerializeField] private GameObject[] hats;

        private string googleUID;
        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();  
        }

        public void Initialize(string uid)
        {
            googleUID = uid;

            //PlayerCustomizationData data = CharacterCustomizeManager.Instance.GetCustomization(uid);
            //if (data != null)
            //{
            //    ApplyCustomization(data);
            //}
        }

        public void ApplyCustomization(PlayerCustomizationData data)
        {
            ApplyColor(data.customColorId);
            ApplyHat(data.hatId);
        }

        private void ApplyColor(int colorId)
        {
            Color color = HelperUtilities.GetUnityColor((CustomizationColor)colorId);
            if (player.spriteRenderer != null)
            {
                player.spriteRenderer.material.color = color;
            }
        }

        private void ApplyHat(int hatId)
        {
            for (int i = 0; i < hats.Length; i++)
            {
                hats[i].SetActive(i == hatId);
            }
        }
    }
}

