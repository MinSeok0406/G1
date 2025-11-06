using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface ITargetingStrategy
    {
        PhotonView AcquireNearest(Transform origin);
    }

    /// <summary>
    /// OverlapCircleNonAlloc으로 가장 가까운 remote PhotonView 타깃을 획득.
    /// </summary>
    public sealed class CircleNearestTargeting2D : ITargetingStrategy
    {
        private readonly float _radius;
        private readonly LayerMask _mask;
        private readonly Collider2D[] _buf;

        public CircleNearestTargeting2D(float radius, LayerMask mask, int bufferSize = 16)
        {
            _radius = Mathf.Max(0.01f, radius);
            _mask = mask;
            _buf = new Collider2D[Mathf.Max(4, bufferSize)];
        }

        public PhotonView AcquireNearest(Transform origin)
        {
            if (!origin) return null;

            int hit = Physics2D.OverlapCircleNonAlloc(origin.position, _radius, _buf, _mask);
            if (hit <= 0) return null;

            // 같은 뷰 중복 제거(더 가까운 거리만 유지)
            var map = DictionaryPool<int, (PhotonView view, float d2)>.Get();
            try
            {
                Vector3 o = origin.position;
                for (int i = 0; i < hit; i++)
                {
                    var c = _buf[i];
                    if (!c) continue;

                    var view = c.GetComponentInParent<PhotonView>();
                    if (!view || view.ViewID <= 0 || view.IsMine) continue;

                    float d2 = (view.transform.position - o).sqrMagnitude;
                    if (map.TryGetValue(view.ViewID, out var cur))
                    {
                        if (d2 < cur.d2) map[view.ViewID] = (view, d2);
                    }
                    else
                    {
                        map.Add(view.ViewID, (view, d2));
                    }
                }

                PhotonView nearest = null;
                float best = float.MaxValue;
                foreach (var kv in map.Values)
                {
                    if (kv.d2 < best)
                    {
                        best = kv.d2;
                        nearest = kv.view;
                    }
                }
                return nearest;
            }
            finally
            {
                DictionaryPool<int, (PhotonView, float)>.Release(map);
            }
        }

        private static class DictionaryPool<TKey, TValue>
        {
            [System.ThreadStatic] private static Dictionary<TKey, TValue> _inst;
            public static Dictionary<TKey, TValue> Get() => _inst ??= new Dictionary<TKey, TValue>(8);
            public static void Release(Dictionary<TKey, TValue> d) => d?.Clear();
        }
    }
}
