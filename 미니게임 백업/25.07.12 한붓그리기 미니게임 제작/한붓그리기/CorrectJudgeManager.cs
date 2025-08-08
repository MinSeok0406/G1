using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CorrectJudgeManager : MonoBehaviour
{
    [Header("레퍼런스 (필수)")]
    [SerializeField] private RawImage drawingDisplay;   // RenderTexture 기반 RawImage
    [SerializeField] private Image correctImage;        // 정답 도형 이미지

    [Header("판정 기준 설정")]
    [Tooltip("정답과 일치하는 최소 비율 (0~1)")]
    [SerializeField, Range(0f, 1f)] private float matchThreshold = 0.8f;

    [Tooltip("지나치게 많이 그린 경우 허용 기준 (0~1)")]
    [SerializeField, Range(0f, 1f)] private float overdrawThreshold = 0.2f;

    [Tooltip("되돌아간 선이 얼마나 허용되는지 (0~1)")]
    [SerializeField, Range(0f, 1f)] private float backtrackThreshold = 0.15f;

    public void JudgeFromRenderTexture(List<Vector3> drawnPoints)
    {
        if (drawingDisplay.texture is not RenderTexture rt)
        {
            Debug.LogError("[JudgeManager] drawingDisplay.texture이 RenderTexture가 아님");
            return;
        }

        if (correctImage.sprite == null)
        {
            Debug.LogError("[JudgeManager] 정답 이미지(Sprite) 없음");
            return;
        }

        Texture2D drawnTex = ReadRenderTexture(rt);
        Texture2D correctTex = correctImage.sprite.texture;

        float match = CalculateMatchRatio(drawnTex, correctTex);
        float overdraw = CalculateOverdrawRatio(drawnTex, correctTex);
        float backtrack = CalculateBacktrackRatio(drawnPoints);

        Debug.Log($"[Judge 결과] Match: {match:F2}, Overdraw: {overdraw:F2}, Backtrack: {backtrack:F2}");

        List<string> failReasons = new();

        if (match < matchThreshold) failReasons.Add($"[정답 불일치] {match:P0} < {matchThreshold:P0}");
        if (overdraw > overdrawThreshold) failReasons.Add($"[과도한 색칠] {overdraw:P0} > {overdrawThreshold:P0}");
        if (backtrack > backtrackThreshold) failReasons.Add($"[역방향 이동] {backtrack:P0} > {backtrackThreshold:P0}");

        if (failReasons.Count == 0)
        {
            Debug.Log($"[판정] ✅ 성공! (일치율 {match:F3} / 기준 {matchThreshold:F3}, 과도한 색칠 {overdraw:F3} / 기준 {overdrawThreshold:F3}, 역방향 {backtrack:F3} / 기준 {backtrackThreshold:F3})");
        }
        else
        {
            string resultSummary = $"(일치율 {match:F3} / 기준 {matchThreshold:F3}, 과도한 색칠 {overdraw:F3} / 기준 {overdrawThreshold:F3}, 역방향 {backtrack:F3} / 기준 {backtrackThreshold:F3})";
            Debug.LogWarning($"[판정] ❌ 실패\n{string.Join("\n", failReasons)}\n{resultSummary}");
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

    private float CalculateBacktrackRatio(List<Vector3> points)
    {
        if (points.Count < 5) return 0f;

        // 방문 기록을 저장하는 해시셋 (2D 격자 단위로 단순화)
        HashSet<Vector2Int> visited = new();
        int revisitCount = 0;
        int sharpTurnCount = 0;

        // 샘플 간격 설정 (매 프레임 전부 저장하면 과잉 감지될 수 있음)
        int sampleInterval = 1; // 필요한 경우 2~3으로 조정 가능

        for (int i = 2; i < points.Count; i += sampleInterval)
        {
            // --- [1] 거리 기반 경로 되짚음 판정 ---
            Vector2Int current = new Vector2Int(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y));
            if (visited.Contains(current))
            {
                revisitCount++;
            }
            else
            {
                visited.Add(current);
            }

            // --- [2] 방향 급회전 판단 ---
            Vector2 dirA = (points[i - 1] - points[i - 2]).normalized;
            Vector2 dirB = (points[i] - points[i - 1]).normalized;

            float dot = Vector2.Dot(dirA, dirB);
            if (dot < -0.5f)
            {
                sharpTurnCount++;
            }
        }

        // 총 판정 횟수
        int totalSamples = (points.Count - 2) / sampleInterval;
        if (totalSamples <= 0) return 0f;

        // 두 기준 모두 충족한 것처럼 취급: 더 엄격한 역주행 감지
        float revisitRatio = (float)revisitCount / totalSamples;
        float sharpTurnRatio = (float)sharpTurnCount / totalSamples;

        // 조합 방식 예: 평균값 또는 가중치 부여도 가능
        float finalBacktrackRatio = (revisitRatio + sharpTurnRatio) / 2f;

        return finalBacktrackRatio;
    }
}
