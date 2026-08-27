using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public ArrowManager arrowManager;
    public GridManager gridManager;

    [Header("关卡配置")]
    [Tooltip("要加载的关卡序号，对应 Resources/Levels/Level_{序号}.json")]
    public int levelIndex = 1;

    [Tooltip("胜利后延迟加载下一关的秒数（等待逃逸动画播完）")]
    public float nextLevelDelay = 2f;

    public int maxLife = 3;
    public int currentLife;

    // 是否正在切换关卡（防止胜利/失败逻辑重复触发）
    private bool _isSwitchingLevel;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartLevel(levelIndex);
    }

    private void OnEnable()
    {
        GridCell.OnCellPointerClicked += HandleGridCellClick;
    }

    private void OnDisable()
    {
        GridCell.OnCellPointerClicked -= HandleGridCellClick;
    }

    /// <summary>
    /// 加载并开始指定关卡
    /// </summary>
    public void StartLevel(int index)
    {
        LevelData data = LoadLevelData(index);
        if (data == null)
        {
            Debug.LogError($"关卡加载失败：Level_{index}，请检查 Resources/Levels/Level_{index}.json 是否存在且格式正确");
            return;
        }

        // 先清理上一关残留的网格与箭头，支持重复加载（重玩/下一关）
        ClearLevel();

        // 根据关卡数据创建网格和箭头
        gridManager.CreateGrid(data.GridXSize, data.GridYSize);
        arrowManager.SetUpArrows(data.Arrows);
        currentLife = maxLife;
    }

    /// <summary>
    /// 通过 Resources.Load 读取关卡 JSON 并反序列化为 LevelData
    /// </summary>
    private LevelData LoadLevelData(int index)
    {
        TextAsset asset = Resources.Load<TextAsset>("Levels/Level_" + index);
        if (asset == null)
        {
            Debug.LogError($"找不到关卡资源：Resources/Levels/Level_{index}");
            return null;
        }

        try
        {
            LevelData data = JsonUtility.FromJson<LevelData>(asset.text);
            if (data == null || data.Arrows == null || data.Arrows.Count == 0)
            {
                Debug.LogError($"关卡 JSON 解析失败或缺少 Arrows 数据：Level_{index}");
                return null;
            }
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"关卡 JSON 解析异常：Level_{index}\n{e}");
            return null;
        }
    }

    /// <summary>
    /// 清理上一关残留的网格与箭头
    /// </summary>
    private void ClearLevel()
    {
        if (gridManager != null)
        {
            // 清空格子 ID 映射，避免与新关卡的 ID 冲突
            gridManager.IDMap.Clear();

            if (gridManager.gridParent != null)
            {
                for (int i = gridManager.gridParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(gridManager.gridParent.GetChild(i).gameObject);
                }
            }
        }

        if (arrowManager != null && arrowManager.arrowsParent != null)
        {
            for (int i = arrowManager.arrowsParent.childCount - 1; i >= 0; i--)
            {
                Destroy(arrowManager.arrowsParent.GetChild(i).gameObject);
            }
        }
    }

    void HandleGridCellClick(GridCell cell)
    {
        if (_isSwitchingLevel) return;   // 切换关卡期间忽略点击
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
        if (_isSwitchingLevel) return;   // 防止切换期间重复计数
        arrowManager.remainingCount--;
        if (arrowManager.remainingCount <= 0)
        {
            GameWin();
        }
    }

    public void OnArrowFailed()
    {
        if (_isSwitchingLevel) return;   // 防止切换期间重复扣命
        currentLife--;
        if (currentLife <= 0)
        {
            GameLose();
        }
    }

    void GameWin()
    {
        if (_isSwitchingLevel) return;   // 防止重复触发
        _isSwitchingLevel = true;

        Debug.Log("游戏胜利");
        StartCoroutine(LoadNextLevelCoroutine());
    }

    /// <summary>
    /// 胜利后延迟加载下一关（等逃逸动画播完后切换）
    /// </summary>
    IEnumerator LoadNextLevelCoroutine()
    {
        yield return new WaitForSeconds(nextLevelDelay);

        _isSwitchingLevel = false;
        levelIndex++;
        StartLevel(levelIndex);
    }

    void GameLose()
    {
        Debug.Log("游戏失败");
    }
}
