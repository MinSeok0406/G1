using UnityEngine;

public class MiniGameAreaManager : MonoBehaviour
{
    [Header("💣 BlockGroupManager")]
    [SerializeField] private BlockGroupManager blockGroupManager;

    [Header("🏐 Ball Controller")]
    [SerializeField] private BallController_UI ballController;

    private bool isGameEnded = false;
    private int remainingBlockCount = 0;

    void Start()
    {
        InitBlockEvents();

        // Ball이 바닥에 닿았을 때 게임오버 처리
        ballController.OnMiss += OnGameOver;
    }

    private void InitBlockEvents()
    {
        var blocks = blockGroupManager.GetActiveBlocks();
        remainingBlockCount = blocks.Count;

        foreach (var block in blocks)
        {
            block.OnDestroyed += OnBlockDestroyed;
        }
    }

    private void OnBlockDestroyed(Block block)
    {
        if (isGameEnded) return;

        remainingBlockCount--;

        if (remainingBlockCount <= 0)
        {
            OnGameClear();
        }
    }

    private void OnGameClear()
    {
        if (isGameEnded) return;

        isGameEnded = true;
        Debug.Log("🎉 게임 클리어!");
        // TODO: 이후 클리어 UI 등 추가 가능

        if (ballController != null) ballController.enabled = false;
    }

    private void OnGameOver()
    {
        if (isGameEnded) return;

        isGameEnded = true;
        Debug.Log("💀 게임 오버!");
        // TODO: 이후 게임오버 UI 등 추가 가능
    }
}
