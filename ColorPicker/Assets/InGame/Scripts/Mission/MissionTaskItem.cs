using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ColorPicker.InGame
{
    public class MissionTaskItem : MonoBehaviour
    {
        public TMP_Text label;
        [HideInInspector] public MiniGameType type;

        public void Init(MiniGameType miniGameType, string text)
        {
            type = miniGameType;
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
            if (label) { label.text = text; label.fontStyle &= ~FontStyles.Strikethrough; }
        }

        public void SetCompleted(bool done)
        {
            if (!label) return;
            if (done) label.fontStyle |= FontStyles.Strikethrough;
            else label.fontStyle &= ~FontStyles.Strikethrough;
        }
    }
}