using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame.UI
{
    public class GraphicPanelController : MonoBehaviour
    {
        [System.Serializable]
        public class Row // 한 줄(가로 3버튼)
        {
            [Tooltip("Low, Middle, High 버튼을 순서대로 할당")]
            public Button low;
            public Button middle;
            public Button high;

            [Tooltip("시작 선택 (0=Low, 1=Middle, 2=High)")]
            [Range(0, 2)] public int defaultIndex = 1;

            [Header("색상 설정")]
            public Color normalBG = new(0.84f, 0.84f, 0.84f);
            public Color normalText = new(0.20f, 0.20f, 0.20f);
            public Color selectedBG = Color.black;
            public Color selectedText = Color.white;

            // 내부 상태
            [HideInInspector] public int currentIndex = -1;

            public Button Get(int idx) => idx switch { 0 => low, 1 => middle, _ => high };
        }

        [Header("그래픽 4줄")]
        public Row texture;
        public Row shadow;
        public Row antiAliasing;
        public Row filtering;

        private void Awake()
        {
            SetupRow(texture);
            SetupRow(shadow);
            SetupRow(antiAliasing);
            SetupRow(filtering);
        }

        private void Start()
        {
            InitRow(texture);
            InitRow(shadow);
            InitRow(antiAliasing);
            InitRow(filtering);
        }

        void SetupRow(Row row)
        {
            for (int i = 0; i < 3; i++)
            {
                int captured = i;
                var btn = row.Get(i);
                if (!btn) { Debug.LogError($"[{name}] 버튼 누락 (index:{i})"); continue; }

                btn.transition = Selectable.Transition.None; // 색상 수동제어
                btn.onClick.AddListener(() => Select(row, captured));
            }
        }

        void InitRow(Row row)
        {
            for (int i = 0; i < 3; i++) SetVisual(row, row.Get(i), false);
            Select(row, Mathf.Clamp(row.defaultIndex, 0, 2));
        }

        void Select(Row row, int index)
        {
            if (index == row.currentIndex) return;

            if (row.currentIndex >= 0) SetVisual(row, row.Get(row.currentIndex), false);
            row.currentIndex = index;
            SetVisual(row, row.Get(row.currentIndex), true);
        }

        void SetVisual(Row row, Button btn, bool selected)
        {
            if (!btn) return;

            var bg = btn.targetGraphic as Image ?? btn.GetComponent<Image>();
            if (bg) bg.color = selected ? row.selectedBG : row.normalBG;

            var text = btn.GetComponentInChildren<TMP_Text>(true);
            if (text) text.color = selected ? row.selectedText : row.normalText;
        }

        // 외부 접근
        public int GetTextureIndex() => texture.currentIndex;
        public int GetShadowIndex() => shadow.currentIndex;
        public int GetAAIndex() => antiAliasing.currentIndex;
        public int GetFilteringIndex() => filtering.currentIndex;
    }
}
