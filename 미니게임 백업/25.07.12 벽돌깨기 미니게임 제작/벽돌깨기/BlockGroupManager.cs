using UnityEngine;
using System.Collections.Generic;

public class BlockGroupManager : MonoBehaviour
{
    private const int rows = 3;
    private const int cols = 5;

    private Block[,] blockGrid = new Block[rows, cols];
    private List<int[,]> blockPatterns = new List<int[,]>();

    void Awake()
    {
        CacheBlocksFromChildren();
        InitPatterns();
        ApplyRandomPattern();
    }

    // 자식 오브젝트에서 Block 자동 수집
    private void CacheBlocksFromChildren()
    {
        Block[] blocks = GetComponentsInChildren<Block>();
        if (blocks.Length != rows * cols)
        {
            Debug.LogError($"Block 개수가 {rows * cols}개가 아닙니다. 현재: {blocks.Length}");
            return;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            int row = i / cols;
            int col = i % cols;
            blockGrid[row, col] = blocks[i];
        }
    }

    // 미리 정의된 블록 배치 템플릿들
    private void InitPatterns()
    {
        blockPatterns.Add(new int[3, 5] {
            {1, 1, 1, 1, 1},
            {1, 1, 0, 1, 1},
            {1, 0, 0, 0, 1}
        });

        blockPatterns.Add(new int[3, 5] {
            {1, 0, 0, 1, 1},
            {1, 1, 0, 1, 1},
            {1, 1, 0, 0, 1}
        });

        blockPatterns.Add(new int[3, 5] {
            {0, 1, 1, 1, 0},
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1}
        });
    }

    // 랜덤 템플릿 적용
    private void ApplyRandomPattern()
    {
        int[,] pattern = blockPatterns[Random.Range(0, blockPatterns.Count)];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                bool active = pattern[row, col] == 1;
                if (blockGrid[row, col] != null)
                    blockGrid[row, col].gameObject.SetActive(active);
            }
        }
    }

    // 현재 활성화된 블록 리스트 반환
    public List<Block> GetActiveBlocks()
    {
        List<Block> active = new List<Block>();
        foreach (var block in blockGrid)
        {
            if (block != null && block.gameObject.activeSelf)
                active.Add(block);
        }
        return active;
    }
}
