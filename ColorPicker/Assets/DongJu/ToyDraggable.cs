using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ToyDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("필수 설정")]
    [SerializeField] private RectTransform dragArea; // 드래그 제한 영역 (예: DragArea)

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;
    private Vector2 dragOffset;

    private Vector2 originalPosition; // 원래 위치 저장용

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        if (dragArea == null)
        {
            Debug.LogWarning($"[{name}] dragArea가 설정되지 않아 부모로 대체됩니다.");
            dragArea = parentRect;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 pointerLocalPos);

        dragOffset = rectTransform.anchoredPosition - pointerLocalPos;

        // 🎯 시작 위치 저장
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 pointerLocalPos);

        Vector2 targetPos = pointerLocalPos + dragOffset;

        ClampToDragArea(ref targetPos);
        rectTransform.anchoredPosition = targetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[{name}] 드래그 종료");

        BoxObject[] boxes = FindObjectsOfType<BoxObject>();
        ObjectColor myColor = GetComponent<ObjectColor>();
        if (myColor == null) return;

        bool wasOverlapping = false;
        bool matchedAndConsumed = false;

        foreach (var box in boxes)
        {
            if (box.IsOverlapping(myColor)) // 충돌 중인지만 먼저 확인
            {
                wasOverlapping = true;
                if (box.TryConsumeToy(myColor)) // 색상도 맞았는지 판단
                {
                    matchedAndConsumed = true;
                    break;
                }
            }
        }

        // 상자에 닿아있고, 색상이 안 맞았으면 → 복귀
        if (wasOverlapping && !matchedAndConsumed)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }


    /// <summary>
    /// 회전된 오브젝트의 외곽까지 고려하여 dragArea 안에 유지되도록 제한
    /// </summary>
    private void ClampToDragArea(ref Vector2 targetPos)
    {
        // dragArea의 월드 좌표 경계 계산
        Vector3[] areaCorners = new Vector3[4];
        dragArea.GetWorldCorners(areaCorners);
        float minX = areaCorners[0].x;
        float maxX = areaCorners[2].x;
        float minY = areaCorners[0].y;
        float maxY = areaCorners[2].y;

        // 오브젝트의 크기 및 회전각 반영한 외곽 크기 계산
        float width = rectTransform.rect.width * rectTransform.lossyScale.x;
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;

        float angleRad = rectTransform.eulerAngles.z * Mathf.Deg2Rad;
        float cos = Mathf.Abs(Mathf.Cos(angleRad));
        float sin = Mathf.Abs(Mathf.Sin(angleRad));

        float boundWidth = width * cos + height * sin;
        float boundHeight = width * sin + height * cos;

        float halfW = boundWidth / 2f;
        float halfH = boundHeight / 2f;

        // 이동 목표 위치를 월드 좌표로 변환
        Vector3 worldTarget = parentRect.TransformPoint(targetPos);

        // dragArea 바깥으로 벗어나지 않게 Clamp
        float clampedX = Mathf.Clamp(worldTarget.x, minX + halfW, maxX - halfW);
        float clampedY = Mathf.Clamp(worldTarget.y, minY + halfH, maxY - halfH);

        // 다시 로컬 좌표로 변환하여 최종 위치 지정
        Vector3 clampedWorldPos = new Vector3(clampedX, clampedY, 0f);
        targetPos = parentRect.InverseTransformPoint(clampedWorldPos);
    }
}
