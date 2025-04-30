using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class InteractiveItem : MonoBehaviour, IInteractive
    {
        public GameObject PopupItem;
        public Button interactiveUI;

        private bool isActive = false;

        public Vector3 GetPosition()
        {
            return gameObject.transform.position;
        }

        public void Interactive()
        {
            PopupItem.SetActive(!isActive);
        }

        public void SetUI(bool isActive)
        {
            interactiveUI.interactable = isActive;
        }
    }
}
