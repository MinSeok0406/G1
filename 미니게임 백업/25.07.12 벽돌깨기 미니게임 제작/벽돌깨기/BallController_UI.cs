using UnityEngine;

public class BallController_UI : MonoBehaviour
{
    public enum StartDirectionMode { Manual, Random }

    [Header("💡 [오브젝트 연결]")]
    [Tooltip("공이 움직일 수 있는 전체 영역 (MiniGameArea 오브젝트)")]
    [SerializeField] private GameObject miniGameAreaObject;

    [Tooltip("충돌 대상이 되는 Paddle 오브젝트")]
    [SerializeField] private GameObject paddleObject;

    [Tooltip("PaddleController_UI 스크립트가 붙은 오브젝트")]
    [SerializeField] private PaddleController_UI paddleController;

    [Header("🧱 [Block 관련]")]
    [Tooltip("BlockGroupManager 오브젝트")]
    [SerializeField] private BlockGroupManager blockGroupManager;

    [Header("⚙️ [공 기본 속도 설정]")]
    [Tooltip("공의 기준 속도 (초당 픽셀)")]
    [SerializeField] private float baseSpeed = 300f;

    [Header("🌀 [감속 설정]")]
    [Tooltip("X 방향 감속 속도 (값이 클수록 더 빨리 원래 속도로 복귀)")]
    [SerializeField] private float xDecayRate = 2f;

    [Tooltip("Y 방향 감속 속도")]
    [SerializeField] private float yDecayRate = 2f;

    [Header("🏹 [시작 방향 설정]")]
    [Tooltip("공이 시작할 때 방향을 랜덤으로 할지, 수동으로 각도를 줄지 설정")]
    [SerializeField] private StartDirectionMode startMode = StartDirectionMode.Random;

    [Tooltip("시작 각도 (0° = 오른쪽, 90° = 위쪽)")]
    [SerializeField, Range(0f, 360f)] private float startAngleDeg = 45f;

    [Header("🚀 [Paddle 영향력 설정]")]
    [Tooltip("Paddle의 X 속도가 공의 X 속도에 얼마나 영향을 미치는지")]
    [SerializeField] private float paddleInfluenceX = 0.005f;

    [Tooltip("Paddle의 X 속도가 공의 Y 속도에 얼마나 영향을 미치는지")]
    [SerializeField] private float paddleInfluenceY = 0.003f;

    public System.Action OnMiss;

    private RectTransform ballRect;
    private RectTransform miniGameAreaRect;
    private RectTransform paddleRect;

    private Vector2 currentVelocity;
    private Vector2 initialDirection;
    private bool isStarted = false;

    void Awake()
    {
        ballRect = GetComponent<RectTransform>();
        miniGameAreaRect = miniGameAreaObject.GetComponent<RectTransform>();
        paddleRect = paddleObject.GetComponent<RectTransform>();
    }

    void Start()
    {
        Vector2 dir = (startMode == StartDirectionMode.Random)
            ? Random.insideUnitCircle.normalized
            : AngleToVector2(startAngleDeg);

        if (dir.y < 0f)
            dir.y *= -1f;

        initialDirection = dir;
        currentVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!isStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isStarted = true;
                currentVelocity = initialDirection * baseSpeed;
            }
            return;
        }

        float targetX = currentVelocity.normalized.x * baseSpeed;
        float targetY = currentVelocity.normalized.y * baseSpeed;

        float newX = Mathf.Lerp(currentVelocity.x, targetX, Time.deltaTime * xDecayRate);
        float newY = Mathf.Lerp(currentVelocity.y, targetY, Time.deltaTime * yDecayRate);
        currentVelocity = new Vector2(newX, newY);

        ballRect.anchoredPosition += currentVelocity * Time.deltaTime;

        CheckWallCollision();
        CheckPaddleCollision();
        CheckBlockCollision();
    }

    private void CheckPaddleCollision()
    {
        if (IsOverlapping(ballRect, paddleRect) && currentVelocity.y < 0f)
        {
            currentVelocity.y *= -1f;

            float paddleSpeedX = paddleController.CurrentVelocityX;

            currentVelocity.x += paddleSpeedX * paddleInfluenceX;
            currentVelocity.y += Mathf.Abs(paddleSpeedX) * paddleInfluenceY;
        }
    }

    private void CheckBlockCollision()
    {
        if (blockGroupManager == null) return;

        var blocks = blockGroupManager.GetActiveBlocks();

        foreach (var block in blocks)
        {
            if (block.CheckCollision(ballRect))
            {
                block.DestroyBlock();

                Rect ballWorld = GetWorldRect(ballRect);
                Rect blockWorld = GetWorldRect(block.GetComponent<RectTransform>());

                float overlapLeft   = ballWorld.xMax - blockWorld.xMin;
                float overlapRight  = blockWorld.xMax - ballWorld.xMin;
                float overlapTop    = blockWorld.yMax - ballWorld.yMin;
                float overlapBottom = ballWorld.yMax - blockWorld.yMin;

                float minHorizontal = Mathf.Min(overlapLeft, overlapRight);
                float minVertical   = Mathf.Min(overlapTop, overlapBottom);

                if (minHorizontal < minVertical)
                    currentVelocity.x *= -1f;
                else
                    currentVelocity.y *= -1f;

                break;
            }
        }
    }

    private void CheckWallCollision()
    {
        float halfWidth = ballRect.rect.width / 2f;
        float halfHeight = ballRect.rect.height / 2f;

        float areaHalfWidth = miniGameAreaRect.rect.width / 2f;
        float areaHalfHeight = miniGameAreaRect.rect.height / 2f;

        Vector2 pos = ballRect.anchoredPosition;

        if (pos.x - halfWidth < -areaHalfWidth || pos.x + halfWidth > areaHalfWidth)
        {
            currentVelocity.x *= -1f;
            pos.x = Mathf.Clamp(pos.x, -areaHalfWidth + halfWidth, areaHalfWidth - halfWidth);
        }

        if (pos.y + halfHeight > areaHalfHeight)
        {
            currentVelocity.y *= -1f;
            pos.y = areaHalfHeight - halfHeight;
        }

        if (pos.y - halfHeight < -areaHalfHeight)
        {
            OnMiss?.Invoke();
            enabled = false;
        }

        ballRect.anchoredPosition = pos;
    }

    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        Rect ra = GetWorldRect(a);
        Rect rb = GetWorldRect(b);
        return ra.Overlaps(rb);
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }

    private Vector2 AngleToVector2(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
}
