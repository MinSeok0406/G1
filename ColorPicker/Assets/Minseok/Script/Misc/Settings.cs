
using UnityEngine;

public static class Settings
{
    #region Player Parameters
    public const float playerMoveSpeed = 12;
    public const float interactionRange = 3;
    #endregion

    #region Animate Parmeters
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int isRight = Animator.StringToHash("isRight");
    public static int isLeft = Animator.StringToHash("isLeft");
    #endregion 

    #region Tag
    public const string interactable = "Interactable";
    #endregion
}

