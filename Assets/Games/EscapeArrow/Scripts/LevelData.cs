using System;
using System.Collections.Generic;

// ============================================================
// 关卡数据对象：与 Resources/Levels 目录下的关卡 JSON 文件一一对应
// 字段名必须与 JSON 文件中的键名保持一致，JsonUtility 才能正确反序列化。
//
// JSON 示例（Level_1.json）：
// {
//     "GridXSize": 7,
//     "GridYSize": 6,
//     "Arrows": [
//         { "Indices": [38, 31, 24, 17, 10, 3], "ColorIndex": 0 },
//         { "Indices": [35, 36, 29, 22, 15, 8, 1], "ColorIndex": 1 }
//     ]
// }
// ============================================================
[Serializable]
public class LevelData
{
    public int GridXSize;                // 网格横向格子数
    public int GridYSize;                // 网格纵向格子数
    public List<LevelArrowData> Arrows;  // 关卡中所有箭头的数据
}

// 单条箭头的数据
[Serializable]
public class LevelArrowData
{
    public List<int> Indices;  // 箭头从头到尾依次占用的格子 ID
    public int ColorIndex;     // 箭头颜色索引（对应 ArrowManager 中的颜色表）
}
