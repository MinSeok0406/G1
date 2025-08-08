using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DrawingInputHandler_Line : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Camera drawingCamera;             // (Overlay라면 null 무방)
    [SerializeField] private LineRenderer lineRenderer;        // 선 그리기용
    [SerializeField] private RectTransform drawingDisplay;     // RawImage 기준
    [SerializeField] private CorrectJudgeManager judgeManager; // 판정 매니저

    [Header("Settings")]
    [SerializeField] private float minDistance = 0.05f;

    private List<Vector3> drawnPoints = new();

    private void Start()
    {
        lineRenderer.numCapVertices = 5;
        lineRenderer.numCornerVertices = 0;

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPositions(new Vector3[0]);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        drawnPoints.Clear();
        lineRenderer.positionCount = 0;

        Vector3 worldPos = ScreenToWorldOnDisplay(eventData.position);
        AddPoint(worldPos);

        Debug.Log($"[PointerDown] screenPos: {eventData.position}, worldPos: {worldPos}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = ScreenToWorldOnDisplay(eventData.position);
        AddPoint(worldPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("[Input] PointerUp – Drawing End");

        // 드로잉 완료 후 자동 판정 호출
        if (judgeManager != null)
            judgeManager.JudgeFromRenderTexture(drawnPoints);
        else
            Debug.LogWarning("[InputHandler] 판정 매니저가 연결되지 않았습니다.");
    }

    private void AddPoint(Vector3 worldPos)
    {
        if (drawnPoints.Count == 0 || Vector3.Distance(drawnPoints[^1], worldPos) > minDistance)
        {
            drawnPoints.Add(worldPos);
            lineRenderer.positionCount = drawnPoints.Count;
            lineRenderer.SetPosition(drawnPoints.Count - 1, worldPos);

            Debug.Log($"[Draw] Added Point: world={worldPos} | Total={drawnPoints.Count}");
        }
    }

    private Vector3 ScreenToWorldOnDisplay(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingDisplay, screenPosition, null, out Vector2 localPos); // Overlay 모드에서는 camera null

        Vector3 worldPos = drawingDisplay.TransformPoint(localPos);
        return worldPos;
    }
}
