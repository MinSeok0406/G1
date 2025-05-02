using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class CitizenAbility : MonoBehaviourPun, IClassAbility
    {
        public void ClassAbility()
        {

        }

        protected virtual void Start()
        {
            UIManager.Instance.SetPlayerUI(true);
        }
    }
}
