using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [CreateAssetMenu(fileName = "MiniGameTemplate_", menuName = "Scriptable Objects/MiniGame/MiniGame Template")]
    public class MiniGameTemplate : ScriptableObject
    {
        public string miniGameName;
        public MiniGameType miniGameType;
        public string missionInfo;
    }
}
