using UnityEngine;

public class EndDetection : MonoBehaviour
{
    [SerializeField] private Transform detectionTarget; // 감지 목표 설정 (예: TOYS 오브젝트)

    private bool gameCleared = false;

    private void Update()
    {
        if (gameCleared) return;

        if (detectionTarget.childCount == 0)
        {
            gameCleared = true;
            OnGameClear();
        }
    }

    private void OnGameClear()
    {
        Debug.Log("🎉 게임 클리어!");
        
        // 이후 확장: 여기서 UI 닫기, 효과음 재생, 다음 단계 호출 등을 추가하면 됨
    }
}
