using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class Settings
    {
        #region Base Parameters
        public const string gameVersion = "1";
        public const string inGameLobbyScene = "InLobby";
        public const string inGameScene = "InGame";
        public const string testMainScene = "InMainMenu";
        public const string InGameScene = "InGame";

        public const int maxPlayer = 10;

        public static Vector3 playerSpawnPointInGame = Vector3.zero;

        #endregion

        #region Network Parameters
        public const float RoomJoinTimeoutSeconds = 8f;
        public const float RoomJoinRetryIntervalSeconds = 0.2f;
        #endregion

        #region Player Parameters
        public const float moveSpeed = 4f;

        public const float defaultCooldown = 120f;

        public const string mafiaAbilityComponentName = "ColorPicker.InGame.MafiaAbility";
        public const string citizenAbilityComponentName = "ColorPicker.InGame.CitizenAbility";
        #endregion

        #region Animate Parameters
        public static int isIdle = Animator.StringToHash("isIdle");
        public static int isMoving = Animator.StringToHash("isMoving");
        public static int isRight = Animator.StringToHash("isRight");
        public static int isLeft = Animator.StringToHash("isLeft");
        public static int isDead = Animator.StringToHash("isDead");
        #endregion

        #region Others Parameters
        public const int maxTryCount = 1000;
        #endregion
    }
}
