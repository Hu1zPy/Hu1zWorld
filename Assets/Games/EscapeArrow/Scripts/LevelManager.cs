using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public ArrowManager arrowManager;
    public GridManager gridManager;

    public int maxLife = 3;
    public int currentLife;
    
    // 先硬编码测试数据（后面再换 JSON）：
    private List<List<int>> testLevel = new List<List<int>>
    {
        new List<int> { 38, 31, 24, 17, 10, 3 },   // 第一条箭头：从头到尾占的格子ID
        new List<int> { 35, 36, 29, 22, 15, 8, 1 },// 第二条
        new List<int> { 6, 5, 12, 19, 26, 33, 40 } // 第三条
    };
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        StartLevel();
    }

    private void OnEnable()
    {
        GridCell.OnCellPointerClicked += HandleGridCellClick;
    }

    private void OnDisable()
    {
        GridCell.OnCellPointerClicked -= HandleGridCellClick;
    }

    private void StartLevel()
    {
        gridManager.CreateGrid(7,6);
        arrowManager.SetUpArrows(testLevel);
        currentLife = maxLife;
        gridManager.transform.position -= new Vector3(0, 1, 0);
    }

    void HandleGridCellClick(GridCell cell)
    {
        var arrow = cell.occupiedBy;
        if (arrow == null || arrow.isEscape) return;
        if (arrow.CanEscape(out List<GridCell> path))
        {
            arrow.EscapeArrow();
        }
        else
        {
            arrow.EscapeFailed(path);
        }
    }

    public void OnArrowEscape()
    {
        arrowManager.remainingCount--;
        if (arrowManager.remainingCount <= 0)
        {
            GameWin();
        }
    }

    public void OnArrowFailed()
    {
        currentLife--;
        if (currentLife <= 0)
        {
            GameLose();
        }
    }

    void GameWin()
    {
        Debug.Log("游戏胜利");
    }

    void GameLose()
    {
        Debug.Log("游戏失败");
    }
}
