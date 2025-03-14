using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class Settings
    {
        #region Base Parameters
        public const string gameVersion = "1";
        #endregion

        #region Player Parameters
        public const float moveSpeed = 7f;
        #endregion

        #region Animate Parameters
        public static int isIdle = Animator.StringToHash("isIdle");
        public static int isMoving = Animator.StringToHash("isMoving");
        public static int isRight = Animator.StringToHash("isRight");
        public static int isLeft = Animator.StringToHash("isLeft");
        #endregion

        #region Others Parameters
        public const int maxTryCount = 1000;
        #endregion
    }
}
