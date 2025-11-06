using UnityEngine;

/// <summary>
/// 약물 보관함
/// </summary>
public class PillContainer : MonoBehaviour
{
    [SerializeField] private PillType containerType = PillType.Red;

    public PillType GetContainerType() => containerType;
}
