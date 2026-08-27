using UnityEngine;

/// <summary>
/// 棋盘屏幕适配器：根据屏幕分辨率/宽高比自动缩放正交相机，保证棋盘完整可见（支持竖屏）。
///
/// 原理：
///  - 相机切换为正交投影，用 orthographicSize 精确控制可视范围；
///  - orthographicSize = 屏幕垂直方向半高，水平可视半宽 = orthographicSize * 宽高比；
///  - 竖屏下宽高比小，水平方向更容易被裁剪，因此 size 取"垂直需要"与"水平需要"中的较大值；
///  - 相机 XZ 对准棋盘几何中心（= GridManager 的 XZ 位置），保持高度与朝向不变。
///
/// 使用：
///  无需手动配置。场景加载后会自动挂到 Main Camera，并自动查找 GridManager；
///  也可手动把本脚本拖到任意对象上（引用会自动补齐）。
///  关卡创建后、屏幕旋转/分辨率变化时会自动重新适配。
/// </summary>
public class BoardFitter : MonoBehaviour
{
    [Header("引用（留空会自动查找）")]
    [Tooltip("要适配的相机，默认使用 Main Camera")]
    public Camera targetCamera;

    [Tooltip("棋盘数据来源，默认查找场景中的 GridManager")]
    public GridManager gridManager;

    [Header("适配参数")]
    [Tooltip("棋盘四周留白（世界单位），防止格子贴边")]
    public float padding = 0.5f;

    private int _lastWidth;
    private int _lastHeight;
    private int _lastSizeX = -1;
    private int _lastSizeY = -1;

    // 场景加载后自动挂到 Main Camera，无需手动拖拽
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttach()
    {
        if (Object.FindObjectOfType<BoardFitter>() != null) return;
        var cam = Camera.main;
        if (cam != null) cam.gameObject.AddComponent<BoardFitter>();
    }

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();

        if (targetCamera != null)
        {
            // 使用正交相机，便于按棋盘尺寸精确控制可视范围
            targetCamera.orthographic = true;
        }
    }

    private void Update()
    {
        // 屏幕尺寸（分辨率/横竖屏）或棋盘尺寸（切换关卡）变化时重新适配
        bool screenChanged = Screen.width != _lastWidth || Screen.height != _lastHeight;
        bool boardChanged = gridManager != null &&
                            (gridManager.sizeX != _lastSizeX || gridManager.sizeY != _lastSizeY);
        if (!screenChanged && !boardChanged) return;

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        if (gridManager != null)
        {
            _lastSizeX = gridManager.sizeX;
            _lastSizeY = gridManager.sizeY;
        }
        Fit();
    }

    /// <summary> 立即按当前屏幕与棋盘尺寸重新适配（关卡创建完成后可手动调用） </summary>
    public void Fit()
    {
        if (targetCamera == null || gridManager == null) return;

        int sizeX = Mathf.Max(1, gridManager.sizeX);   // 横向格子数（对应屏幕水平方向）
        int sizeY = Mathf.Max(1, gridManager.sizeY);   // 纵向格子数（对应屏幕垂直方向）

        float boardHalfWidth = sizeX * 0.5f;    // 棋盘半宽（世界单位）
        float boardHalfHeight = sizeY * 0.5f;   // 棋盘半高

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

        // 垂直方向需要：棋盘半高 + 留白
        float needVertical = boardHalfHeight + padding;
        // 水平方向需要：水平可视半宽 = size * aspect，须 ≥ 棋盘半宽 + 留白
        float needHorizontal = (boardHalfWidth + padding) / aspect;

        // 取较大值，保证横屏/竖屏、任意分辨率下棋盘完整可见
        targetCamera.orthographicSize = Mathf.Max(needVertical, needHorizontal);

        // 相机对准棋盘中心（棋盘几何中心 = GridManager 的 XZ 位置），高度与朝向保持不变
        Vector3 pos = targetCamera.transform.position;
        pos.x = gridManager.transform.position.x;
        pos.z = gridManager.transform.position.z;
        targetCamera.transform.position = pos;
    }
}
