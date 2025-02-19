using Google.Protobuf.WellKnownTypes;
using System.Collections;
using TMPro;
using UnityEngine;

#region RequireComponent
[RequireComponent(typeof(MouseInteractiveEvent))]
#endregion
[DisallowMultipleComponent]
public class MouseInteractiveObject : MonoBehaviour
{
    [HideInInspector] public MouseInteractiveEvent mouseInteractiveEvent;

    public MouseInteractive mouseInteractive = MouseInteractive.Button;

    public PRS originPRS { get; private set;}

    private void Awake()
    {
        originPRS = new PRS(transform.position, transform.rotation, transform.localScale);

        mouseInteractiveEvent = GetComponent<MouseInteractiveEvent>();
    }

    public void MoveTransform(PRS prs, float duration)
    {
        StartCoroutine(MoveTransformProcess(prs, duration));
    }

    private IEnumerator MoveTransformProcess(PRS prs, float duration)
    {
        float elapsedTime = 0;

        if (duration != 0)
        {
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float moveTime = Mathf.Clamp01(elapsedTime / duration);

                transform.position = Vector3.Lerp(transform.position, prs.pos, moveTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, prs.rot, moveTime);
                transform.localScale = Vector3.Lerp(transform.localScale, prs.scale, moveTime);

                yield return null;
            }
        }

        transform.position = prs.pos;
        transform.rotation = prs.rot;
        transform.localScale = prs.scale;
    }
}
