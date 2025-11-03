using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 짧은 이벤트 컷신(킬, 투표, 퇴장 등) 화이트리스트.
    /// </summary>
    [CreateAssetMenu(fileName = "CutsceneCatalog", menuName = "ColorPicker/Cutscene Catalog")]
    public sealed class CutsceneCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("예: Kill_Common, Vote_Start, Eject_Space")]
            public string key;

            [Tooltip("로컬 VideoClip")]
            public VideoClip clip;

            [Tooltip("스킵 가능 여부 (킬처럼 강제 재생은 false 권장)")]
            public bool allowSkip = false;

            [Tooltip("clip.length가 비어있을 때만 사용되는 추정 길이(초)")]
            [Min(0.1f)] public double fallbackDurationSec = 2.0d;
        }

        [SerializeField] private List<Entry> entries = new();
        private Dictionary<string, Entry> _map;

        private void OnEnable() => BuildIndex();

        public void BuildIndex()
        {
            _map = new(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.key) || e.clip == null) continue;
                if (_map.ContainsKey(e.key))
                {
                    Debug.LogWarning($"[CutsceneCatalog] Duplicate key ignored: {e.key}");
                    continue;
                }
                _map.Add(e.key, e);
            }
        }

        public bool TryGet(string key, out Entry entry)
        {
            entry = null;
            return _map != null && _map.TryGetValue(key, out entry);
        }

        public static double EstimateDuration(Entry e)
            => e.clip ? e.clip.length : Math.Max(0.1d, e.fallbackDurationSec);
    }
}
