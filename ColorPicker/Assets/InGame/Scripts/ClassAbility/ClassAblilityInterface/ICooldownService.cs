using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface ICooldownService
    {
        bool IsOnCooldown(int viewId);
        float GetRemaining(int viewId);
        void StartCooldownHostOnly(int viewId, float durationSec, PhotonView sender);
        void RemoveForPlayerLeftRoom(int actorNumber);
        void SetEndTimeFromServer(int viewId, double endTime); // ★ 클라 로컬 캐시용
        bool TryGetEndTime(int viewId, out double endTime);    // 동기화용
    }

    public sealed class CooldownService : ICooldownService
    {
        private readonly Dictionary<int, double> _endTimes = new(32);

        public bool IsOnCooldown(int viewId) =>
            _endTimes.TryGetValue(viewId, out double end) && PhotonNetwork.Time < end;

        public float GetRemaining(int viewId) =>
            _endTimes.TryGetValue(viewId, out double end)
                ? Mathf.Max(0f, (float)(end - PhotonNetwork.Time)) : 0f;

        public void StartCooldownHostOnly(int viewId, float durationSec, PhotonView sender)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            double end = PhotonNetwork.Time + Mathf.Max(0f, durationSec);
            _endTimes[viewId] = end;

            var owner = PhotonView.Find(viewId)?.Owner;
            if (owner != null && sender != null)
                sender.RPC("RPC_SyncCooldown", owner, viewId, end);
        }

        public void RemoveForPlayerLeftRoom(int actorNumber)
        {
            var remove = ListPool<int>.Get();
            try
            {
                foreach (var kv in _endTimes)
                {
                    var pv = PhotonView.Find(kv.Key);
                    if (pv == null || pv.Owner == null || pv.OwnerActorNr == actorNumber)
                        remove.Add(kv.Key);
                }
                for (int i = 0; i < remove.Count; i++) _endTimes.Remove(remove[i]);
            }
            finally { ListPool<int>.Release(remove); }
        }

        public void SetEndTimeFromServer(int viewId, double endTime) => _endTimes[viewId] = endTime;

        public bool TryGetEndTime(int viewId, out double endTime) => _endTimes.TryGetValue(viewId, out endTime);

        private static class ListPool<T>
        {
            [System.ThreadStatic] private static List<T> _list;
            public static List<T> Get() => _list ??= new List<T>(8);
            public static void Release(List<T> list) => list?.Clear();
        }
    }
}
