using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Minseok
{
    struct Boundary
    {
        public float Up, Down, Left, Right;

        public Boundary(float up, float down, float left, float right)
        {
            Up = up; Down = down; Left = left; Right = right;
        }
    }
}