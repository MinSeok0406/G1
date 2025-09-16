using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class ColorPickButton : MonoBehaviour
    {
        [SerializeField] private ColorType color;
        [SerializeField] private Image setIcon;

        public void OnClick_SetColor()
        {
            UIManager.Instance.InitializeButtonIcon();
            UIManager.Instance.SetDeductionColor(color);
            if(!setIcon.gameObject.activeSelf) setIcon.gameObject.SetActive(true);
        }

        public void DisableIcon() {
            if(setIcon.gameObject.activeSelf) setIcon.gameObject.SetActive(false);
        }
    }
}
