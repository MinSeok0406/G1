using UnityEngine;

[CreateAssetMenu(fileName ="MissionDetails_",menuName ="Scriptable Objects/Mission/MissionDetails")]
public class MissionDetailsSO : ScriptableObject
{
    public string missionName;
    public string missionInfo;
    public string sceneName;

    #region Validate
#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateCheck.ValidateCheckEmptyString(this, nameof(missionName), missionName);
        ValidateCheck.ValidateCheckEmptyString(this, nameof(missionInfo), missionInfo);
        ValidateCheck.ValidateCheckEmptyString(this, nameof(sceneName), sceneName);
    }
#endif
    #endregion
}
