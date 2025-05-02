using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*#region RequireComponent
[RequireComponent(typeof(CircleCollider2D))]
#endregion*/
//[DisallowMultipleComponent]
public class InteractiveControl : MonoBehaviour
{
    private CircleCollider2D circleCollider2D;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
        circleCollider2D.radius = Settings.interactionRange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Settings.interactable))
        {
            if (other.GetComponent<IInteractable>() != null)
            {
                UIManager.Instance.SetInterativeButton(other.GetComponent<IInteractable>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Settings.interactable))
        {

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Settings.interactionRange); 
    }
}
