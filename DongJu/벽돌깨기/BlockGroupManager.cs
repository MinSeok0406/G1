using UnityEngine;
using System.Collections.Generic;

public class BlockGroupManager : MonoBehaviour
{
    private const int rows = 3;
    private const int cols = 5;

    private Block[,] blockGrid = new Block[rows, cols];
    private Block targetBlock; // 유일하게 남는 블록

    void Awake()
    {
        CacheBlocksFromChildren();
        SetRandomTargetBlock();
    }

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

    private void SetRandomTargetBlock()
    {
        // 전체 블록을 일단 비활성화
        foreach (var block in blockGrid)
        {
            if (block != null)
                block.gameObject.SetActive(false);
        }

        // 하나만 랜덤으로 활성화
        int randomRow = Random.Range(0, rows);
        int randomCol = Random.Range(0, cols);

        targetBlock = blockGrid[randomRow, randomCol];
        if (targetBlock != null)
            targetBlock.gameObject.SetActive(true);
    }

    // 현재 살아있는 블록 반환 (항상 하나일 것)
    public List<Block> GetActiveBlocks()
    {
        List<Block> list = new List<Block>();
        if (targetBlock != null && targetBlock.gameObject.activeSelf)
            list.Add(targetBlock);
        return list;
    }

    public Block GetTargetBlock() => targetBlock;
}
