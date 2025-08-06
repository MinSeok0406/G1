using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class InteractiveObjectEffect : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetOutline(false);
        }

        public void SetOutline(bool active)
        {
            spriteRenderer.material = active ? GameResources.Instance.interactiveMaterial : GameResources.Instance.outlineMaterial;
        }
    }
}
