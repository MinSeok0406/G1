using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRuleSettings
{
    public int maxPlayerAmount;
    public int mafiaAmount;

    public void SetRuleSettingRecomend()
    {
        maxPlayerAmount = 10;
        mafiaAmount = 1;
    }
}
