
using FunkyCode;
using UnityEngine;

namespace ColorPicker.InGame
{
    public sealed class PlayerLightController
    {
        private readonly Light2D _light;

        public PlayerLightController(Light2D light)
        {
            _light = light;

            if (_light == null)
            {
                Debug.LogWarning("[PlayerLightController] Light2D가 null입니다.");
            }
        }

        /// <summary>
        /// 라이트 활성화
        /// </summary>
        public void Enable()
        {
            SetActive(true);
        }

        /// <summary>
        /// 라이트 비활성화
        /// </summary>
        public void Disable()
        {
            SetActive(false);
        }

        /// <summary>
        /// 라이트 색상 설정
        /// </summary>
        public void SetColor(Color color)
        {
            if (_light == null) return;

            _light.color = color;
        }

        private void SetActive(bool active)
        {
            if (_light == null || _light.gameObject == null) return;

            _light.gameObject.SetActive(active);
        }
    }
}