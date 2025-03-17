using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager> 
{
    [SerializeField] private Button interactiveButton;
    [SerializeField] private Image interactiveIcon;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetInterativeButton(IInteractable interactable)
    {
        RemoveClickEvents(interactiveButton);

        interactiveButton.onClick.AddListener(interactable.Interact);

        interactiveIcon.sprite = interactable.GetIcon();
    } 

    public void SetInterativeButton(IClassAbility classAbility)
    {
        RemoveClickEvents(interactiveButton);

        interactiveButton.onClick.AddListener(classAbility.UseClassAbility);
    }

    private void RemoveClickEvents(Button button)
    {
        button.onClick.RemoveAllListeners();
    }
}