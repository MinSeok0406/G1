using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CorrectImageManager : MonoBehaviour
{
    [Header("정답 이미지 리스트")]
    [SerializeField] private List<Sprite> correctSprites; // 인스펙터에서 직접 연결

    [Header("정답 이미지가 표시될 UI 오브젝트")]
    [SerializeField] private Image correctImageTarget; // DrawingCanvas 아래 CorrectImg

    private Sprite currentCorrectSprite;

    public Sprite CurrentCorrectSprite => currentCorrectSprite;

    private void Start()
    {
        LoadRandomCorrectImage();
    }

    public void LoadRandomCorrectImage()
    {
        if (correctSprites == null || correctSprites.Count == 0)
        {
            Debug.LogError("[CorrectImageManager] No correct images assigned.");
            return;
        }

        currentCorrectSprite = correctSprites[Random.Range(0, correctSprites.Count)];
        correctImageTarget.sprite = currentCorrectSprite;
        correctImageTarget.SetNativeSize();

        Debug.Log($"[CorrectImageManager] Loaded: {currentCorrectSprite.name}");
    }
}
