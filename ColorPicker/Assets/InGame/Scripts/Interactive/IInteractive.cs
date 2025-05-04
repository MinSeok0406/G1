

using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IInteractive
    {
        public void Interactive();

        public Vector3 GetPosition();

        public void SetUI(bool isActive);
    }
}
