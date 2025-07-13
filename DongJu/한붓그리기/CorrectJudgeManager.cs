using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CorrectJudgeManager : MonoBehaviour
{
    [Header("레퍼런스 (필수)")]
    [SerializeField] private RawImage drawingDisplay;
    [SerializeField] private Image correctImage;

    [Header("판정 기준 설정")]
    [SerializeField, Range(0f, 1f)] private float matchThreshold = 0.8f;
    [SerializeField, Range(0f, 1f)] private float overdrawThreshold = 0.2f;

    public bool JudgeFromRenderTexture(List<Vector3> drawnPoints)
    {
        if (drawingDisplay.texture is not RenderTexture rt)
        {
            Debug.LogError("[JudgeManager] drawingDisplay.texture이 RenderTexture가 아님");
            return false;
        }

        if (correctImage.sprite == null)
        {
            Debug.LogError("[JudgeManager] 정답 이미지(Sprite) 없음");
            return false;
        }

        Texture2D drawnTex = ReadRenderTexture(rt);
        Texture2D correctTex = correctImage.sprite.texture;

        float match = CalculateMatchRatio(drawnTex, correctTex);
        float overdraw = CalculateOverdrawRatio(drawnTex, correctTex);

        Debug.Log($"[Judge 결과] Match: {match:F2}, Overdraw: {overdraw:F2}");

        List<string> failReasons = new();

        if (match < matchThreshold) failReasons.Add($"[정답 불일치] {match:P0} < {matchThreshold:P0}");
        if (overdraw > overdrawThreshold) failReasons.Add($"[과도한 색칠] {overdraw:P0} > {overdrawThreshold:P0}");

        if (failReasons.Count == 0)
        {
            Debug.Log($"[판정] ✅ 성공! (일치율 {match:F3} / 기준 {matchThreshold:F3}, 과도한 색칠 {overdraw:F3} / 기준 {overdrawThreshold:F3})");
            return true;
        }
        else
        {
            string resultSummary = $"(일치율 {match:F3} / 기준 {matchThreshold:F3}, 과도한 색칠 {overdraw:F3} / 기준 {overdrawThreshold:F3})";
            Debug.LogWarning($"[판정] ❌ 실패\n{string.Join("\n", failReasons)}\n{resultSummary}");
            return false;
        }
    }

    private Texture2D ReadRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    private float CalculateMatchRatio(Texture2D drawn, Texture2D correct)
    {
        int matchCount = 0, totalCorrect = 0;

        for (int y = 0; y < correct.height; y++)
        {
            for (int x = 0; x < correct.width; x++)
            {
                Color c = correct.GetPixel(x, y);
                if (c.a > 0.1f)
                {
                    totalCorrect++;
                    if (drawn.GetPixel(x, y).a > 0.1f)
                        matchCount++;
                }
            }
        }

        return totalCorrect > 0 ? (float)matchCount / totalCorrect : 0f;
    }

    private float CalculateOverdrawRatio(Texture2D drawn, Texture2D correct)
    {
        int overdraw = 0, drawnCount = 0;

        for (int y = 0; y < drawn.height; y++)
        {
            for (int x = 0; x < drawn.width; x++)
            {
                if (drawn.GetPixel(x, y).a > 0.1f)
                {
                    drawnCount++;
                    if (correct.GetPixel(x, y).a <= 0.1f)
                        overdraw++;
                }
            }
        }

        return drawnCount > 0 ? (float)overdraw / drawnCount : 0f;
    }
}
