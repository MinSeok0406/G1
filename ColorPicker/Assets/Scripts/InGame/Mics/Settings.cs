using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class Settings
    {
        #region Player Parameters
        public const float moveSpeed = 7;
        #endregion 
        
        #region Camera Parameters
        public const float cameraLocalPositionZ = -10f;
        public const float cameraOrthographicSize = 5f;
        #endregion

        #region Animate Parameters
        public static int isIdle = Animator.StringToHash("isIdle");
        public static int isMoving = Animator.StringToHash("isMoving");
        public static int isRight = Animator.StringToHash("isRight");
        public static int isLeft = Animator.StringToHash("isLeft");
        #endregion

        #region Other Parameters
        public const string inGameSceneName = "InGame";
        public const int maxTryCount = 1000;
        #endregion

    }
}
