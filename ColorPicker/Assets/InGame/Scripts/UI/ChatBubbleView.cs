using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace ColorPicker.InGame
{
    public sealed class ChatBubbleView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text playerText;
        [SerializeField] private TMP_Text messageText;

        // ObjectPool 소유자 (없으면 Destroy 사용)
        private IObjectPool<ChatBubbleView> _owner;
        public bool HasPoolOwner => _owner != null;

        /// <summary>풀 소유권 연결 (풀에서 Get() 직후 호출)</summary>
        public void AttachPoolOwner(IObjectPool<ChatBubbleView> owner)
        {
            _owner = owner;
        }

        /// <summary>풀로 반환 (없으면 Destroy)</summary>
        public void ReleaseToPool()
        {
            // 뷰 초기화(다음 재사용 대비)
            if (playerText)  playerText.text  = string.Empty;
            if (messageText) messageText.text = string.Empty;

            if (_owner != null)
                _owner.Release(this);
            else
                Destroy(gameObject);
        }

        /// <summary>텍스트 바인딩 (간단 위생 처리 포함)</summary>
        public void Bind(string playerLabel, string message)
        {
            // 간단 정리: 개행/CR 제거 및 Trim (서버에서 1차 Sanitizer 수행됨)
            if (!string.IsNullOrEmpty(playerLabel))
                playerLabel = playerLabel.Replace("\r", "").Replace("\n", " ").Trim();

            if (!string.IsNullOrEmpty(message))
                message = message.Replace("\r", "").Replace("\n", " ").Trim();

            if (playerText)  playerText.text  = playerLabel ?? string.Empty;
            if (messageText) messageText.text = message ?? string.Empty;
        }
    }
}
