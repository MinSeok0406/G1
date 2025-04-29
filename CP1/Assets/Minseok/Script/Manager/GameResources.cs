using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Interactive Icon
    public Sprite missionInteractiveIcon;
    #endregion  


    #region Validation
    private void OnValidate()
    {
        ValidateCheck.ValidateCheckNullValue(this, nameof(missionInteractiveIcon), missionInteractiveIcon);
    }
    #endregion
}