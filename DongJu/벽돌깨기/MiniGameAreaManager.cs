using UnityEngine;

public class MiniGameAreaManager : MonoBehaviour
{
    [Header("🧱 Block (단일 블록)")]
    [SerializeField] private Block targetBlock;

    [Header("🏐 Ball Controller")]
    [SerializeField] private BallController_UI ballController;

    private bool isGameEnded = false;

    void Start()
    {
        // 게임오버 조건: 공이 바닥에 닿았을 때
        ballController.OnMiss += OnGameOver;

        // 게임 클리어 조건: 블록이 파괴되었을 때
        targetBlock.OnBlockDestroyed += OnGameClear;
    }

    private void OnGameClear()
    {
        if (isGameEnded) return;

        isGameEnded = true;
        Debug.Log("🎉 게임 클리어!");

        // 공 정지
        if (ballController != null)
            ballController.enabled = false;
    }

    private void OnGameOver()
    {
        if (isGameEnded) return;

        isGameEnded = true;
        Debug.Log("💀 게임 오버!");

        // 공 정지
        if (ballController != null)
            ballController.enabled = false;
    }
}
