using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoxObject : MonoBehaviour
{
    public ToyColorCategory acceptedCategory;

    [Header("Animation")]
    [SerializeField] private string triggerName = "Bounce"; // 트리거 이름만 인스펙터에서 설정 가능

    private Animator boxAnimator;

    private readonly HashSet<ObjectColor> overlappingToys = new HashSet<ObjectColor>();

    private void Awake()
    {
        boxAnimator = GetComponent<Animator>();
        if (boxAnimator == null)
            Debug.LogWarning($"[BoxObject] Animator가 없습니다: {name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var toy = other.GetComponent<ObjectColor>();
        if (toy != null)
        {
            overlappingToys.Add(toy);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var toy = other.GetComponent<ObjectColor>();
        if (toy != null)
        {
            overlappingToys.Remove(toy);
        }
    }

    public bool IsOverlapping(ObjectColor toy)
    {
        return overlappingToys.Contains(toy);
    }

    public bool TryConsumeToy(ObjectColor toy)
    {
        if (!overlappingToys.Contains(toy))
            return false;

        if (toy.colorCategory == acceptedCategory)
        {
            Debug.Log($"[BoxObject] 색상 일치! 장난감 제거됨: {toy.name}");
            overlappingToys.Remove(toy);
            Destroy(toy.gameObject);

            PlayBoxAnimation();
            return true;
        }
        else
        {
            Debug.Log($"[BoxObject] 색상 불일치: {toy.name}");
            return false;
        }
    }

    private void PlayBoxAnimation()
    {
        if (boxAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            boxAnimator.SetTrigger(triggerName);
        }
    }
}
