using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IInteractive
    {
        public void OnInteract();
        public Vector3 GetPosition();
        void ToggleHighlight(bool active);
    }
}
