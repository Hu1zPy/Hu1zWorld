using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局管理器：负责创建「开洞地面 + 黑洞深渊」、生成/补充可吞噬物体、维护游玩区域边界。
/// 挂到一个空物体上即可，开局会自动创建地面/黑洞/第一波物体。
/// </summary>
[ExecuteAlways] // 编辑器里也能预览「开洞地面 + 黑洞」（预览生成物不会保存进场景）
public class HoleGameManager : MonoBehaviour
{
    public static HoleGameManager Instance { get; private set; }

    [Header("游玩区域（XZ 平面）")]
    public Vector2 playAreaSize = new Vector2(40f, 40f);   // 场地大小
    public float groundHeight = 0f;                        // 地面 Y 高度

    [Header("物体生成")]
    public int maxProps = 45;                              // 场上物体数量上限
    public Vector2 propSizeRange = new Vector2(0.7f, 3.6f); // 物体尺寸范围

    [Header("地面与黑洞")]
    public Color groundColor = new Color(0.42f, 0.62f, 0.34f); // 地面颜色
    public float previewHoleRadius = 2f;                  // 编辑器里（未运行时）黑洞的预览半径
    public float cellSize = 0.8f;                         // 地面网格精度（越小洞越圆滑，越耗性能；视觉与碰撞共用）
    public float margin = 6f;                             // 开洞地面超出游玩场地的边距
    public float voidDepth = 10f;                         // 黑洞深渊向下延伸的深度
    public int voidSegments = 32;                         // 深渊圆柱细分段数
    public Color voidColor = new Color(0.02f, 0.02f, 0.03f); // 深渊颜色（近黑）

    [Header("物理")]
    public float gravity = 100f;                              // 全局重力（源项目就是 -100，下落才利落）

    private readonly List<Swallowable> props = new List<Swallowable>();
    private HoleGround holeGround;

    // 场地边界（方便其它脚本用）
    public static Vector2 Min => new Vector2(-Instance.playAreaSize.x * 0.5f, -Instance.playAreaSize.y * 0.5f);
    public static Vector2 Max => new Vector2(Instance.playAreaSize.x * 0.5f, Instance.playAreaSize.y * 0.5f);

    private void Awake()
    {
        Instance = this;
        ApplyGravity();
    }

    private void OnValidate()
    {
        ApplyGravity(); // 在 Inspector 里改 gravity 时实时生效
    }

    private void ApplyGravity()
    {
        Physics.gravity = new Vector3(0f, -gravity, 0f);
    }

    private void Start()
    {
        // 清理编辑器里残留的预览黑洞（防止重复生成）；编辑模式必须用 DestroyImmediate
        foreach (var existing in FindObjectsOfType<HoleGround>())
        {
            if (existing == holeGround) continue;
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }

        EnsureWorld();

        // 只有运行时才生成可吞噬物体；编辑器里只生成「开洞地面 + 黑洞」用于预览
        if (Application.isPlaying)
        {
            for (int i = 0; i < maxProps; i++) SpawnProp();
        }
    }

    // 创建（或复用）开洞地面 + 黑洞深渊（HoleGround 由代码自动创建并挂到运行时对象上，无需手动挂）
    private void EnsureWorld()
    {
        if (holeGround == null)
        {
            var go = new GameObject("Hole Ground Controller");
            go.hideFlags = HideFlags.HideAndDontSave; // 预览对象，不保存进场景
            holeGround = go.AddComponent<HoleGround>();
            holeGround.Init(CreateMaterial(groundColor));
        }

        // 把黑洞外观参数转发给 HoleGround：直接在 HoleGameManager 的 Inspector 里调即可
        holeGround.Configure(cellSize, margin, voidDepth, voidSegments, voidColor);
        holeGround.SetPreview(previewHoleRadius);
    }

    private void OnDestroy()
    {
        if (holeGround == null) return;
        if (Application.isPlaying) Destroy(holeGround.gameObject);
        else DestroyImmediate(holeGround.gameObject); // 编辑模式下 Destroy 会报错，必须用 DestroyImmediate
    }

    // 随机位置，尽量不与其他物体重叠
    public Vector3 GetRandomPosition()
    {
        Vector3 pos;
        int attempts = 0;
        do
        {
            pos = new Vector3(Random.Range(Min.x, Max.x), groundHeight, Random.Range(Min.y, Max.y));
        }
        while (attempts++ < 30 && IsOverlapping(pos));
        return pos;
    }

    private bool IsOverlapping(Vector3 pos)
    {
        for (int i = 0; i < props.Count; i++)
        {
            if (props[i] == null) continue;
            if (Vector3.Distance(props[i].transform.position, pos) < props[i].size * 1.5f)
                return true;
        }
        return false;
    }

    public void SpawnProp()
    {
        float size = Random.Range(propSizeRange.x, propSizeRange.y);

        // 随机用方块或球体作为物体
        GameObject go = GameObject.CreatePrimitive(Random.value < 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere);
        go.transform.SetParent(transform, true);

        Vector3 pos = GetRandomPosition();
        pos.y = groundHeight + size * 0.5f; // 让物体“坐”在地面上
        go.transform.position = pos;

        var renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial(Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f));

        var sw = go.AddComponent<Swallowable>();
        sw.Init(size);
        props.Add(sw);
    }

    // 物体被吞后调用：移除并补充一个，保持场上密度
    public void OnPropConsumed(Swallowable prop)
    {
        props.Remove(prop);
        Destroy(prop.gameObject);
        SpawnProp();
    }

    // 清空并重建（重开时用）
    public void ResetField()
    {
        for (int i = props.Count - 1; i >= 0; i--)
        {
            if (props[i] != null) Destroy(props[i].gameObject);
        }
        props.Clear();
        for (int i = 0; i < maxProps; i++) SpawnProp();
    }

    // 创建基础材质（兼容 URP / 内置渲染管线）。unlit=true 用于深渊（不受光照影响，保证是纯黑）
    public static Material CreateMaterial(Color color, bool unlit = false)
    {
        Shader shader;
        if (unlit)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Diffuse");
        }
        else
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");
        }

        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else mat.color = color;
        return mat;
    }
}