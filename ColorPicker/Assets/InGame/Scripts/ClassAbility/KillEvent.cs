
using System;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class KillEvent : MonoBehaviour
    {
        public event Action<KillEvent, KillEventArgs> OnKill;

        public void CallKillByActor(int actorNumber)
        {
            OnKill?.Invoke(this, new KillEventArgs
            {
                actorNumber = actorNumber,
                targetViewID = -1
            });
        }

        public void CallKillByView(int viewID)
        {
            OnKill?.Invoke(this, new KillEventArgs
            {
                targetViewID = viewID,
                actorNumber = -1
            });
        }
        
        public void CallKill(PhotonView view)
        {
            if (!view) return;
            CallKillByView(view.ViewID);
        }

        public void CallKillEvent(int playerId)
        {
            CallKillByActor(playerId);
        }
    }

    public class KillEventArgs : EventArgs
    {
        public int targetViewID = -1;
        public int actorNumber = -1;
    }
}