using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public enum GameState
    {
        gameStarted,
        playingStage,
        meetingState,
        voteState,
        gameEnded
    }

    public enum PlayerClassType
    {
        citizen,
        mafia,
        detective,
        ghost
    }
}
