using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRuleSettings
{
    int maxPlayerAmount;
    int mafiaAmount;

    public void SetRuleSettingRecomend()
    {
        maxPlayerAmount = 10;
        mafiaAmount = 1;
    }
}
