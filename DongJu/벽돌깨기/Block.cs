using UnityEngine;

public class Block : MonoBehaviour
{
    [Tooltip("이 블록이 파괴되기까지 필요한 공 충돌 횟수")]
    [SerializeField] private int requiredHits = 3;

    [Tooltip("이 블록이 파괴되었을 때 호출될 클리어 콜백")]
    public System.Action OnBlockDestroyed;

    private RectTransform rect;
    private int hitCount = 0;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 공의 RectTransform을 받아 충돌 여부 체크 및 Hit 처리
    /// </summary>
    public bool TryHit(RectTransform ballRect)
    {
        if (IsOverlapping(rect, ballRect))
        {
            hitCount++;

            if (hitCount >= requiredHits)
            {
                gameObject.SetActive(false);
                OnBlockDestroyed?.Invoke(); // 게임 클리어 알림
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 두 RectTransform이 겹치는지 확인
    /// </summary>
    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        Rect ra = GetWorldRect(a);
        Rect rb = GetWorldRect(b);
        return ra.Overlaps(rb);
    }

    /// <summary>
    /// RectTransform의 월드 좌표 Rect 반환
    /// </summary>
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }
}
