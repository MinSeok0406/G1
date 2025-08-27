using TMPro;
using UnityEngine;

namespace ColorPicker.InGame
{
     public sealed class ChatBubbleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerText;
        [SerializeField] private TMP_Text messageText;

        public void Bind(string playerLabel, string message)
        {
            if (playerText) playerText.text = playerLabel;
            if (messageText) messageText.text = message;
        }
    }
}