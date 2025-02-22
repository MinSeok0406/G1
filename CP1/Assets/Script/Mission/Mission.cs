using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission : MonoBehaviour, IInteractable
{
    public MissionDetailsSO missionDetails;

    public Sprite GetIcon()
    {
        return GameResources.Instance.missionInteractiveIcon;
    }

    public void Interact()
    {
        SceneManager.LoadScene(missionDetails.sceneName, LoadSceneMode.Additive);
    }
}
