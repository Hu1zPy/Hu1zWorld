using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public Arrow prefab;
    public Transform arrowsParent;
    public int remainingCount;
    public float moveTime = .5f;

    [Header("箭头颜色表")]
    [Tooltip("与关卡 JSON 中每条箭头的 ColorIndex 一一对应，索引越界时自动使用最后一个颜色")]
    public Color[] arrowColors =
    {
        new Color(1f, 0.37f, 0.37f),   // 0 红
        new Color(1f, 0.72f, 0.35f),   // 1 橙
        new Color(1f, 0.92f, 0.42f),   // 2 黄
        new Color(0.55f, 0.9f, 0.4f),  // 3 绿
        new Color(0.45f, 0.82f, 0.95f),// 4 青
        new Color(0.5f, 0.62f, 1f),    // 5 蓝
        new Color(0.78f, 0.55f, 1f),   // 6 紫
        new Color(1f, 0.55f, 0.85f),   // 7 粉
        new Color(0.92f, 0.92f, 0.92f),// 8 白
        new Color(0.82f, 0.62f, 0.42f) // 9 棕
    };

    /// <summary>
    /// 根据关卡数据创建所有箭头（含颜色染色）
    /// </summary>
    public void SetUpArrows(List<LevelArrowData> allArrowData)
    {
        if (allArrowData == null) return;

        int createdCount = 0;
        foreach (var arrowData in allArrowData)
        {
            if (arrowData == null || arrowData.Indices == null || arrowData.Indices.Count < 2)
            {
                Debug.LogWarning("跳过无效箭头数据：Indices 为空或长度不足 2");
                continue;
            }

            var a = Instantiate(prefab, arrowsParent);
            a.SetUpFromData(arrowData.Indices);
            SetArrowColor(a, arrowData.ColorIndex);
            createdCount++;
        }

        // 用实际创建数量统计剩余箭头，而不是 childCount——
        // 上一关箭头用 Destroy 延迟销毁，切换关卡时 childCount 会把残留算进去
        remainingCount = createdCount;
    }

    /// <summary>
    /// 按颜色索引给箭头染色（索引越界时使用最后一个颜色）
    /// </summary>
    private void SetArrowColor(Arrow arrow, int colorIndex)
    {
        if (arrowColors == null || arrowColors.Length == 0) return;

        var renderer = arrow.GetComponent<ArrowRenderer>();
        if (renderer == null) return;

        int safeIndex = Mathf.Clamp(colorIndex, 0, arrowColors.Length - 1);
        renderer.SetColor(arrowColors[safeIndex]);
    }
}
