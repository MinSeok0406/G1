using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalSelector : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Sprite[] options;
    
    private int currentIndex = 0;
    
    void Start()
    {
        // image가 Inspector에서 할당되지 않았으면 자동으로 찾기
        if (image == null)
            image = GetComponent<Image>();
        
        // 버튼 이벤트 연결
        if (leftButton != null)
            leftButton.onClick.AddListener(PreviousOption);
        if (rightButton != null)
            rightButton.onClick.AddListener(NextOption);
        
        // 초기 이미지 설정
        UpdateImage();
    }
    
    public void NextOption()
    {
        if (options != null && options.Length > 0)
        {
            currentIndex = (currentIndex + 1) % options.Length;
            UpdateImage();
        }
    }
    
    public void PreviousOption()
    {
        if (options != null && options.Length > 0)
        {
            currentIndex = (currentIndex - 1 + options.Length) % options.Length;
            UpdateImage();
        }
    }
    
    private void UpdateImage()
    {
        if (options != null && options.Length > 0 && 
            currentIndex >= 0 && currentIndex < options.Length && 
            options[currentIndex] != null && image != null)
        {
            image.sprite = options[currentIndex];
        }
    }
    
    public Sprite GetCurrentOption()
    {
        if (options != null && options.Length > 0 && 
            currentIndex >= 0 && currentIndex < options.Length)
        {
            return options[currentIndex];
        }
        return null;
    }
    
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}
