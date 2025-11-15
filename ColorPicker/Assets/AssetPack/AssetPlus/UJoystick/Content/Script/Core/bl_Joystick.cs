using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    [SerializeField, Range(1, 15)] private float Radio = 5;
    [SerializeField, Range(0.01f, 1)] private float SmoothTime = 0.5f;
    [SerializeField, Range(0.5f, 4)] private float OnPressScale = 1.5f;
    public Color NormalColor = new Color(1, 1, 1, 1);
    public Color PressColor = new Color(1, 1, 1, 1);
    [SerializeField, Range(0.1f, 5)] private float Duration = 1;

    [Header("Reference")]
    [SerializeField] private RectTransform StickRect;
    [SerializeField] private RectTransform CenterReference;

    private Vector3 DeathArea;
    private Vector3 currentVelocity;
    private bool isFree = false;
    private int lastPointerId = int.MinValue; // 현재 드래그 중 포인터ID
    private Image stickImage;
    private Image backImage;
    private Canvas m_Canvas;
    private float diff;
    private Vector3 PressScaleVector;

    void Start()
    {
        if (StickRect == null)
        {
            Debug.LogError("Please add the stick for joystick work!.");
            enabled = false; return;
        }

        m_Canvas = transform.root.GetComponentInChildren<Canvas>();
        if (m_Canvas == null)
        {
            Debug.LogError("Required at least one canvas for joystick work!");
            enabled = false; return;
        }

        DeathArea = CenterReference.position;
        diff = CenterReference.position.magnitude;
        PressScaleVector = new Vector3(OnPressScale, OnPressScale, OnPressScale);

        backImage = GetComponent<Image>();
        stickImage = StickRect.GetComponent<Image>();
        if (backImage != null)
        {
            backImage.CrossFadeColor(NormalColor, 0.1f, true, true);
            stickImage.CrossFadeColor(NormalColor, 0.1f, true, true);
        }
    }

    void Update()
    {
        DeathArea = CenterReference.position;
        if (!isFree) return;

        StickRect.position = Vector3.SmoothDamp(StickRect.position, DeathArea, ref currentVelocity, smoothTime);
        if (Vector3.Distance(StickRect.position, DeathArea) < .1f)
        {
            isFree = false;
            StickRect.position = DeathArea;
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (lastPointerId != int.MinValue) return;

        lastPointerId = data.pointerId;
        StopAllCoroutines();
        StartCoroutine(ScaleJoystick(true));

        // 이벤트 좌표 사용
        Vector3 pos = m_Canvas.ToCanvasPosition(data.position);
        MoveStick(pos);

        if (backImage != null)
        {
            backImage.CrossFadeColor(PressColor, Duration, true, true);
            stickImage.CrossFadeColor(PressColor, Duration, true, true);
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (data.pointerId != lastPointerId) return;

        isFree = false;

        Vector3 pos = m_Canvas.ToCanvasPosition(data.position);
        MoveStick(pos);
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (data.pointerId != lastPointerId) return;

        isFree = true;
        currentVelocity = Vector3.zero;
        lastPointerId = int.MinValue;

        StopAllCoroutines();
        StartCoroutine(ScaleJoystick(false));

        if (backImage != null)
        {
            backImage.CrossFadeColor(NormalColor, Duration, true, true);
            stickImage.CrossFadeColor(NormalColor, Duration, true, true);
        }
    }

    private void MoveStick(Vector3 position)
    {
        if (Vector2.Distance(DeathArea, position) < radio)
            StickRect.position = position;
        else
            StickRect.position = DeathArea + (position - DeathArea).normalized * radio;
    }

    IEnumerator ScaleJoystick(bool increase)
    {
        float t = 0f;
        Vector3 from = StickRect.localScale;
        Vector3 to = increase ? new Vector3(OnPressScale, OnPressScale, OnPressScale) : Vector3.one;

        while (t < Duration)
        {
            StickRect.localScale = Vector3.Lerp(from, to, t / Duration);
            t += Time.deltaTime;
            yield return null;
        }
        StickRect.localScale = to;
    }

    private float radio => (Radio * 5 + Mathf.Abs((diff - CenterReference.position.magnitude)));
    private float smoothTime => (1 - (SmoothTime));

<<<<<<< HEAD
    /// <summary>
    /// 조이스틱이 현재 터치되어 있는지 여부
    /// </summary>
    public bool IsTouching
    {
        get
        {
            return lastId != -2;
        }
    }

    /// <summary>
    /// Value Horizontal of the Joystick
    /// Get this for get the horizontal value of joystick
    /// </summary>
    public float Horizontal
    {
        get
        {
            return (StickRect.position.x - DeathArea.x) / Radio;
        }
    }

    /// <summary>
    /// Value Vertical of the Joystick
    /// Get this for get the vertical value of joystick
    /// </summary>
    public float Vertical
    {
        get
        {
            return (StickRect.position.y - DeathArea.y) / Radio;
        }
    }
}
=======
    public float Horizontal => (StickRect.position.x - DeathArea.x) / Radio;
    public float Vertical => (StickRect.position.y - DeathArea.y) / Radio;
}
>>>>>>> 9ccbd7be9016a3c41e786a51375977596e1b44b2
