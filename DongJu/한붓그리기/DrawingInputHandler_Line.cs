using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DrawingInputHandler_Line : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private RectTransform drawingDisplay;
    [SerializeField] private CorrectJudgeManager judgeManager;

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

        if (judgeManager != null)
        {
            bool success = judgeManager.JudgeFromRenderTexture(drawnPoints);

            if (!success)
            {
                lineRenderer.positionCount = 0;
                drawnPoints.Clear();
                Debug.Log("[InputHandler] 실패 판정 → 선 제거");
            }
        }
        else
        {
            Debug.LogWarning("[InputHandler] 판정 매니저가 연결되지 않았습니다.");
        }
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
            drawingDisplay, screenPosition, null, out Vector2 localPos);

        Vector3 worldPos = drawingDisplay.TransformPoint(localPos);
        return worldPos;
    }
}
