using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public enum PlayerClassType
    {
        citizen, // 0
        mafia, // 1
        detective, //2
        ghost // 3
    }

    public enum MiniGameType
    {
        DragTest,
        ClickTest,
        BookOrganizing,
        OrigamiFolding
    }

    public enum ColorType
    {
        White,
        Red,
        Green,
        Blue,
        Yellow,
        Orange,
        Purple,
        Cyan,
        Pink,
        Brown,
        Black
    }

    public enum CustomizationColor
    {
        White,
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta,
        Orange,
        Purple,
        Pink,
        Brown
    }

    public enum GameStateType
    {
        None = -1,
        GameStarted = 0,
        Playing = 1,
        Meeting = 2,
        Voting = 3
    }

    public enum GameEventCode : byte
    {
        RequestMeetingState = 10,
        RequestVotingState = 11,
        RequestPlayingState = 12
    }
}
