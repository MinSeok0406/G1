using UnityEngine;

public class Block : MonoBehaviour
{
    public System.Action<Block> OnDestroyed;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // Ball과 충돌했는지 판단
    public bool CheckCollision(RectTransform other)
    {
        return GetWorldRect(rect).Overlaps(GetWorldRect(other));
    }

    // Block 제거
    public void DestroyBlock()
    {
        gameObject.SetActive(false);
        OnDestroyed?.Invoke(this);
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }
}
