using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class Define
    {
        public enum Scene
        {
            Unknown,
            Login,
            Lobby,
            InLocal,
            Game,
        }

        public enum Sound
        {
            Bgm,
            Effect,
            MaxCount,
        }

        public enum UIEvent
        {
            Click,
            Drag,
        }

        public enum Color
        {
            RED,
            GREEN,
            BLUE,
            YELLOW,
            BLACK,
            GREY,
            WHITE
        }
    }
}