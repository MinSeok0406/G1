using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleTabController : MonoBehaviour
{
    [Header("버튼 부모 (TabBTLayout). 비우면 이 오브젝트를 사용")]
    [SerializeField] private Transform tabParent;

    [Header("버튼 이름 (계층명과 동일)")]
    [SerializeField] private string graphicBtnName = "GraphicBT";
    [SerializeField] private string soundBtnName   = "SoundBT";
    [SerializeField] private string gameplayBtnName= "GameplayBT";

    [Header("탭별 패널들")]
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("버튼 배경 색상")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color normalColor   = Color.gray;

    [Header("선택 안된 버튼 글자색")]
    [SerializeField] private Color unselectedTextColor = Color.white;

    [Header("선택된 버튼 글자색")]
    [SerializeField] private Color selectedTextColor = Color.black;

    [Header("초기 선택 인덱스 (0:Graphic, 1:Sound, 2:Gameplay)")]
    [Range(0, 2)] [SerializeField] private int defaultIndex = 0;

    private readonly List<Button> _buttons = new();

    private void Awake()
    {
        if (tabParent == null) tabParent = transform;

        Add(FindBtn(graphicBtnName),  0);
        Add(FindBtn(soundBtnName),    1);
        Add(FindBtn(gameplayBtnName), 2);
    }

    private void Start()
    {
        SelectByIndex(defaultIndex);
    }

    private Button FindBtn(string childName)
    {
        var t = tabParent.Find(childName);
        return t ? t.GetComponent<Button>() : null;
    }

    private void Add(Button bt, int index)
    {
        if (bt == null) return;
        _buttons.Add(bt);
        bt.onClick.AddListener(() => SelectByIndex(index));
    }

    public void SelectByIndex(int index)
    {
        // 버튼 및 텍스트 색상 변경
        for (int i = 0; i < _buttons.Count; i++)
            SetVisual(_buttons[i], 
                i == index ? selectedColor : normalColor, 
                i == index ? selectedTextColor : unselectedTextColor);

        // 패널 ON/OFF
        if (graphicsPanel) graphicsPanel.SetActive(index == 0);
        if (soundPanel)    soundPanel.SetActive(index == 1);
        if (gameplayPanel) gameplayPanel.SetActive(index == 2);
    }

    private void SetVisual(Button bt, Color bgColor, Color txtColor)
    {
        if (bt == null) return;

        // 버튼 배경 색
        var img = bt.GetComponent<Image>();
        if (img) img.color = bgColor;

        // 자식 TMP_Text 색
        var text = bt.GetComponentInChildren<TMP_Text>(true);
        if (text) text.color = txtColor;

        // ColorBlock.normalColor 업데이트
        var cb = bt.colors;
        cb.normalColor = bgColor;
        bt.colors = cb;
    }
}
