using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class ColorPickKillButton : MonoBehaviour
    {
        [SerializeField] private Button colorPickKillButton;
        [SerializeField] private Slider cooldownSlider;
        [SerializeField] private TMP_Text cooldownText;

        private bool coolInit;
        private float cooldownTime;

        public void UpdateCooldownUI(float remain)
        {
            if (!coolInit)
            {
                cooldownTime = remain;
                colorPickKillButton.gameObject.SetActive(false);
                cooldownSlider.gameObject.SetActive(true);
                coolInit = true;
            }

            if (remain > 0)
            {
                cooldownSlider.value = remain / cooldownTime;
                cooldownText.text = Mathf.CeilToInt(remain).ToString();
            }
        }

        public void CoolInitialize()
        {
            coolInit = false;
            colorPickKillButton.gameObject.SetActive(true);
            cooldownSlider.gameObject.SetActive(false);
        }

        public Button GetColorPickKillButton()
        {
            return colorPickKillButton;
        }
    }
}