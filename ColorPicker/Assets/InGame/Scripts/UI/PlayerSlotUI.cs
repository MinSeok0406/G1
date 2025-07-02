using TMPro;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerSlotUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text readyIndicator;

        public void InitializedSlot(LobbyPlayerData data)
        {
            if (nicknameText != null)
                nicknameText.text = data.staticData.nickname;

            if (readyIndicator != null)
                readyIndicator.gameObject.SetActive(data.dynamicData.isReady);
        }

        public void SetReady(bool isReady)
        {
            if (readyIndicator != null)
                readyIndicator.gameObject.SetActive(isReady);
        }
    }
}