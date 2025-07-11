using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ToyObject : MonoBehaviour
{
    [Header("필수 설정")]
    [SerializeField] private RectTransform randomArea; // 배치 기준 영역 (예: DragArea)

    private RectTransform toyRect;
    private RectTransform parentRect;

    private void Start()
    {
        StartCoroutine(DelayInit());
    }

    private System.Collections.IEnumerator DelayInit()
    {
        yield return null; // UI 레이아웃이 완료될 때까지 대기

        toyRect = GetComponent<RectTransform>();
        parentRect = toyRect.parent as RectTransform;

        if (randomArea == null)
        {
            Debug.LogError($"[{name}] 랜덤 배치 영역(randomArea)이 설정되지 않았습니다.");
            yield break;
        }

        Vector2 localPos = GetRandomPositionInRandomArea_LocalToParent();
        toyRect.anchoredPosition = localPos;

        // 🎯 눈에 띄는 회전 (-30도 ~ 30도)
        float randomAngle = Random.Range(-30f, 30f);
        toyRect.localRotation = Quaternion.Euler(0f, 0f, randomAngle);

        Debug.Log($"[{name}] 랜덤 위치: {localPos}, 회전: {randomAngle}");
    }

    private Vector2 GetRandomPositionInRandomArea_LocalToParent()
    {
        Vector3[] areaCorners = new Vector3[4];
        randomArea.GetWorldCorners(areaCorners);

        float minX = areaCorners[0].x;
        float maxX = areaCorners[2].x;
        float minY = areaCorners[0].y;
        float maxY = areaCorners[2].y;

        // 오브젝트의 절반 크기만큼 여백 확보
        float halfWidth = toyRect.rect.width * toyRect.lossyScale.x / 2f;
        float halfHeight = toyRect.rect.height * toyRect.lossyScale.y / 2f;

        float randomX = Random.Range(minX + halfWidth, maxX - halfWidth);
        float randomY = Random.Range(minY + halfHeight, maxY - halfHeight);
        Vector3 worldPos = new Vector3(randomX, randomY, 0);

        // 부모 기준 로컬 좌표로 변환
        return parentRect.InverseTransformPoint(worldPos);
    }
}
