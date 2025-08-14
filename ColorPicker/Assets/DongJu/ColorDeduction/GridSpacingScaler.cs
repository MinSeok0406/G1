using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class GridSpacingScalerCurve : UIBehaviour
{
    public float width1 = 1270f; // 최대 폭
    public float width2 = 910f;  // 최소 폭

    public float spacingX1 = 80f; // 최대 폭일 때 spacing
    public float spacingX2 = 15f; // 최소 폭일 때 spacing

    [Tooltip("0~1 사이 값에 대응하는 곡선 (X: 0=width2, 1=width1)")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GridLayoutGroup grid;
    private RectTransform rt;
    private float lastWidth = -1f;

    protected override void Awake()
    {
        base.Awake();
        Cache();
        UpdateSpacing(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Cache();
        UpdateSpacing(true);
    }

    void Cache()
    {
        if (!grid) grid = GetComponent<GridLayoutGroup>();
        if (!rt) rt = GetComponent<RectTransform>();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        UpdateSpacing();
    }

    protected override void OnValidate()
    {
        Cache();
        UpdateSpacing(true);
    }

    void Update()
    {
        if (!rt) return;
        if (!Mathf.Approximately(lastWidth, rt.rect.width))
            UpdateSpacing();
    }

    void UpdateSpacing(bool force = false)
    {
        if (!grid || !rt) return;

        float w = rt.rect.width;
        if (!force && Mathf.Approximately(lastWidth, w)) return;
        lastWidth = w;

        // 0~1 정규화
        float t = Mathf.InverseLerp(width2, width1, w);

        // 곡선 적용
        float curveValue = curve.Evaluate(t);

        // spacingX1 ~ spacingX2 범위에 맞게 보간
        float newX = Mathf.Lerp(spacingX2, spacingX1, curveValue);

        var s = grid.spacing;
        if (!Mathf.Approximately(s.x, newX))
        {
            grid.spacing = new Vector2(newX, s.y);
            LayoutRebuilder.MarkLayoutForRebuild(rt);
        }
    }
}
