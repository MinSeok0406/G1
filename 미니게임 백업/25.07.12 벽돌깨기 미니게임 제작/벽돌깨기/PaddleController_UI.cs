using UnityEngine;
using UnityEngine.EventSystems;

public class PaddleController_UI : MonoBehaviour
{
    [Header("UI References (GameObjects)")]
    [SerializeField] private GameObject paddleObject;
    [SerializeField] private GameObject miniGameAreaObject;
    [SerializeField] private GameObject touchInputObject;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 1f;

    private RectTransform paddleRect;
    private RectTransform miniGameAreaRect;
    private RectTransform touchInputRect;

    public float CurrentVelocityX { get; private set; }

    private Vector2 lastTouchPos;
    private Vector2 lastPaddlePos;
    private bool isDragging = false;

    void Awake()
    {
        // 내부에서 RectTransform 자동 캐싱
        paddleRect = paddleObject.GetComponent<RectTransform>();
        miniGameAreaRect = miniGameAreaObject.GetComponent<RectTransform>();
        touchInputRect = touchInputObject.GetComponent<RectTransform>();

        lastPaddlePos = paddleRect.anchoredPosition;
    }

    void Update()
    {
        HandleTouchInput();

        Vector2 currentPos = paddleRect.anchoredPosition;
        CurrentVelocityX = (currentPos.x - lastPaddlePos.x) / Time.deltaTime;
        lastPaddlePos = currentPos;
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        var touch = Input.GetTouch(0);
        Vector2 screenTouchPos = touch.position;

        if (!RectTransformUtility.RectangleContainsScreenPoint(touchInputRect, screenTouchPos))
            return;

        if (touch.phase == TouchPhase.Began)
        {
            isDragging = true;
            lastTouchPos = screenTouchPos;
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            float deltaX = screenTouchPos.x - lastTouchPos.x;
            lastTouchPos = screenTouchPos;

            MovePaddle(deltaX * sensitivity);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDragging = false;
        }
    }

    private void MovePaddle(float deltaX)
    {
        Vector2 pos = paddleRect.anchoredPosition;
        pos.x += deltaX;

        float halfWidth = paddleRect.rect.width / 2f;
        float areaHalfWidth = miniGameAreaRect.rect.width / 2f;
        pos.x = Mathf.Clamp(pos.x, -areaHalfWidth + halfWidth, areaHalfWidth - halfWidth);

        paddleRect.anchoredPosition = pos;
    }
}
