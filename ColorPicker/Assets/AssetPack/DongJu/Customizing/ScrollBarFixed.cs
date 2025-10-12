using UnityEngine;
using UnityEngine.UI;

public class ScrollbarFixedSync : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollbar;

    private float fixedSize;

    private void Awake()
    {
        // Scrollbar에 설정된 초기 size 값을 고정값으로 저장
        fixedSize = scrollbar.size;
    }

    private void Start()
    {
        // 1. 핸들 크기 고정
        scrollbar.size = fixedSize;

        // 2. 스크롤 동기화: ScrollRect → Scrollbar
        scrollRect.onValueChanged.AddListener(pos =>
        {
            scrollbar.value = scrollRect.horizontalNormalizedPosition;
        });

        // 3. 핸들 드래그 시: Scrollbar → ScrollRect
        scrollbar.onValueChanged.AddListener(val =>
        {
            scrollRect.horizontalNormalizedPosition = val;
        });
    }
}
