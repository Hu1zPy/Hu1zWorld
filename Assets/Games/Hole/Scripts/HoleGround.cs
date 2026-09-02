using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 黑洞的核心视觉（对齐源项目 Hole Market 3D 的做法）：
/// 1) 地面是【真实开洞的网格】 —— 以玩家位置为圆心挖出一个圆形洞
///    （简化版源项目 GroundBehavior：把洞内的网格顶点推挤到圆边上）；
/// 2) 洞下面是【黑色深渊圆柱】 —— 深色圆柱从地面向下延伸（简化版源项目 VoidMesh）。
/// 玩家移动 / 黑洞变大时，开洞地面和深渊会重新填充（复用同一个 Mesh，不反复 new，避免内存泄漏）。
/// 未运行时（编辑器里）用 previewRadius / previewCenter 做静态预览。
/// </summary>
public class HoleGround : MonoBehaviour
{
    [Header("预览（未运行时在编辑器里看效果用）")]
    public float previewRadius = 2f;         // 编辑器里黑洞的预览半径
    public Vector3 previewCenter = Vector3.zero; // 编辑器里黑洞的预览中心

    [Header("地面网格")]
    public float cellSize = 0.8f;            // 地面网格精度（越小洞越圆滑，也越耗性能；视觉与碰撞共用）
    public float margin = 6f;                // 地面超出游玩场地的边距

    [Header("深渊")]
    public float voidDepth = 10f;            // 黑洞向下延伸的深度
    public int segments = 32;                // 深渊圆柱细分段数
    public Color voidColor = new Color(0.02f, 0.02f, 0.03f); // 深渊颜色（近黑）

    private MeshFilter groundFilter;
    private MeshCollider groundCollider;
    private MeshFilter voidFilter;
    private Transform voidTransform;

    private Mesh groundMesh;          // 复用：开洞地面（视觉与碰撞共用，只 new 一次）
    private Mesh voidMesh;            // 复用：深渊的网格（同上）

    private Vector3 lastCenter = Vector3.zero;
    private float lastRadius = -1f;

    // 复用：网格临时缓冲（避免每帧 new List / new bool[] 造成 GC 峰值）
    private readonly List<Vector3> groundVerts = new List<Vector3>();
    private readonly List<Vector2> groundUvs = new List<Vector2>();
    private readonly List<int> groundTris = new List<int>();
    private bool[] groundPushed;

    private readonly List<Vector3> voidVerts = new List<Vector3>();
    private readonly List<int> voidTris = new List<int>();
    private readonly List<Vector3> voidNormals = new List<Vector3>();

    // 由 HoleGameManager 创建时调用：生成地面和深渊两个子对象
    public void Init(Material groundMaterial)
    {
        var groundGO = new GameObject("Hole Ground");
        groundGO.transform.SetParent(transform, true);
        groundGO.hideFlags = HideFlags.HideAndDontSave;
        groundFilter = groundGO.AddComponent<MeshFilter>();
        var groundRenderer = groundGO.AddComponent<MeshRenderer>();
        groundRenderer.sharedMaterial = groundMaterial;
        groundMaterial.SetInt("_Cull", 0); // 地面双面渲染，绕开三角形绕序问题

        // 地面碰撞体：带洞的网格碰撞体（非凸）。物体站在地面上，洞到脚下就失去支撑掉进去
        groundCollider = groundGO.AddComponent<MeshCollider>();
        groundCollider.convex = false;

        var voidGO = new GameObject("Void");
        voidGO.transform.SetParent(transform, true);
        voidGO.hideFlags = HideFlags.HideAndDontSave;
        voidFilter = voidGO.AddComponent<MeshFilter>();
        var voidRenderer = voidGO.AddComponent<MeshRenderer>();
        var voidMat = HoleGameManager.CreateMaterial(voidColor, unlit: true); // 不受光照，保证纯黑
        voidMat.SetInt("_Cull", 0);
        voidRenderer.sharedMaterial = voidMat;
        voidTransform = voidGO.transform;

        Rebuild();
    }

    // 由 HoleGameManager 转发外观参数（在 HoleGameManager 的 Inspector 里调即可）
    public void Configure(float cellSize, float margin, float voidDepth, int segments, Color voidColor)
    {
        this.cellSize = cellSize;
        this.margin = margin;
        this.voidDepth = voidDepth;
        this.segments = segments;
        this.voidColor = voidColor;
        Rebuild();
    }

    // 编辑器里改参数时实时刷新预览（管理器没就绪时跳过，避免空引用）
    private void OnValidate()
    {
        if (groundFilter == null || voidFilter == null) return;
        if (HoleGameManager.Instance == null) return;
        if (!Application.isPlaying) Rebuild();
    }

    // HoleGameManager 在预览半径变化时调用
    public void SetPreview(float radius)
    {
        previewRadius = radius;
        if (!Application.isPlaying) Rebuild();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying) return;

        var hole = HoleController.Instance;
        if (hole == null) return;

        Vector3 center = new Vector3(hole.transform.position.x, HoleGameManager.Instance.groundHeight, hole.transform.position.z);
        float radius = hole.Radius;

        // 深渊跟随玩家（水平位置）
        voidTransform.position = center;

        // 移动超过半格 / 半径变化就重建（视觉与碰撞共用同一网格，一起更新）
        float dx = center.x - lastCenter.x;
        float dz = center.z - lastCenter.z;
        bool dirty = dx * dx + dz * dz > cellSize * cellSize * 0.25f
                  || Mathf.Abs(radius - lastRadius) > 0.01f;

        if (dirty)
        {
            lastCenter = center;
            lastRadius = radius;
            RebuildVisual(center, radius);
        }
    }

    // 一次性全量重建（Init / Configure / OnValidate / SetPreview 用）
    private void Rebuild()
    {
        if (HoleGameManager.Instance == null) return; // 管理器还没就绪（如组件被单独反序列化）时跳过

        Vector3 center;
        float radius;

        if (Application.isPlaying)
        {
            var hole = HoleController.Instance;
            if (hole == null) return;
            center = new Vector3(hole.transform.position.x, HoleGameManager.Instance.groundHeight, hole.transform.position.z);
            radius = hole.Radius;
        }
        else
        {
            center = new Vector3(previewCenter.x, HoleGameManager.Instance.groundHeight, previewCenter.z);
            radius = previewRadius;
        }

        RebuildVisual(center, radius);

        if (!Application.isPlaying) voidTransform.position = center; // 编辑器预览时深渊也放在预览中心
    }

    // 重建开洞地面（视觉与碰撞共用同一 mesh）+ 深渊
    private void RebuildVisual(Vector3 center, float radius)
    {
        // 复用 mesh：只 new 一次，之后原地改顶点（Clear + SetXXX），避免每帧分配造成内存泄漏。
        // 视觉和碰撞共用同一份 groundMesh：碰撞体引用同一 mesh，改完顶点自动同步，不存在「碰撞洞滞后」。
        if (groundMesh == null)
        {
            groundMesh = new Mesh { name = "Hole Ground Mesh", hideFlags = HideFlags.HideAndDontSave };
            groundFilter.sharedMesh = groundMesh;
            groundCollider.sharedMesh = groundMesh;
        }
        if (voidMesh == null)
        {
            voidMesh = new Mesh { name = "Void Mesh", hideFlags = HideFlags.HideAndDontSave };
            voidFilter.sharedMesh = voidMesh;
        }

        FillGroundMesh(groundMesh, center, radius, cellSize);

        // 强制碰撞体重新烘焙：MeshCollider 只在 sharedMesh 被赋值时烘焙一次。
        // 原地改顶点（复用同一个 mesh）不会触发重新烘焙 → 碰撞体永远停在【第一次】的洞上，
        // 玩家一动就成了「幽灵洞」：视觉洞外的物体也直直漏进去、且不翻滚。
        // 重新赋一次 sharedMesh 才让物理引擎按最新顶点重新烘焙（不 new mesh，无内存泄漏）。
        groundCollider.sharedMesh = null;
        groundCollider.sharedMesh = groundMesh;

        FillVoidMesh(voidMesh, radius);
    }

    // 地面：矩形网格，把洞内的顶点推挤到圆边上 → 挖出真正的圆洞。
    // 复用传入的 mesh（Clear + 原地填数据），不 new。
    private void FillGroundMesh(Mesh mesh, Vector3 holeCenter, float holeRadius, float gridCellSize)
    {
        var min = HoleGameManager.Min;
        var max = HoleGameManager.Max;

        float halfW = (max.x - min.x) * 0.5f + margin;
        float halfH = (max.y - min.y) * 0.5f + margin;
        Vector3 center = new Vector3((min.x + max.x) * 0.5f, HoleGameManager.Instance.groundHeight, (min.y + max.y) * 0.5f);

        int nx = Mathf.Max(2, Mathf.CeilToInt(halfW * 2f / gridCellSize));
        int nz = Mathf.Max(2, Mathf.CeilToInt(halfH * 2f / gridCellSize));

        var verts = groundVerts; verts.Clear();
        var uvs = groundUvs; uvs.Clear();

        // 复用 pushed 数组（只按需扩容，不每次 new）
        int vcount = (nx + 1) * (nz + 1);
        if (groundPushed == null || groundPushed.Length < vcount) groundPushed = new bool[vcount];
        else Array.Clear(groundPushed, 0, vcount);
        var pushed = groundPushed;

        float startX = center.x - halfW;
        float startZ = center.z - halfH;

        for (int j = 0; j <= nz; j++)
        {
            for (int i = 0; i <= nx; i++)
            {
                int id = j * (nx + 1) + i;
                var p = new Vector3(startX + i * gridCellSize, HoleGameManager.Instance.groundHeight, startZ + j * gridCellSize);

                float dx = p.x - holeCenter.x;
                float dz = p.z - holeCenter.z;
                float d = Mathf.Sqrt(dx * dx + dz * dz);

                if (d < holeRadius)
                {
                    // 把洞内的顶点推挤到圆边上 → 形成真正的圆洞边缘
                    float dirX = d > 0.0001f ? dx / d : 1f;
                    float dirZ = d > 0.0001f ? dz / d : 0f;
                    p.x = holeCenter.x + dirX * holeRadius;
                    p.z = holeCenter.z + dirZ * holeRadius;
                    pushed[id] = true;
                }

                verts.Add(p);
                uvs.Add(new Vector2((float)i / nx, (float)j / nz));
            }
        }

        var tris = groundTris; tris.Clear();
        for (int j = 0; j < nz; j++)
        {
            for (int i = 0; i < nx; i++)
            {
                int a = j * (nx + 1) + i;         // (i, j)
                int b = a + 1;                     // (i+1, j)
                int c = a + (nx + 1) + 1;          // (i+1, j+1)
                int d = a + (nx + 1);              // (i, j+1)

                // 四个顶点全在洞内（都被挤到圆边）→ 零面积，跳过
                if (pushed[a] && pushed[b] && pushed[c] && pushed[d]) continue;

                // 跳过退化（近零面积）三角形，避免 NaN 法线导致物理烘焙崩溃
                if (!IsDegenerate(verts[a], verts[c], verts[b])) { tris.Add(a); tris.Add(c); tris.Add(b); }
                if (!IsDegenerate(verts[a], verts[d], verts[c])) { tris.Add(a); tris.Add(d); tris.Add(c); }
            }
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // 三点是否近共线/重合（面积≈0）→ 退化三角形
    private static bool IsDegenerate(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
        return cross.sqrMagnitude < 1e-10f;
    }

    // 深渊：从上环向下延伸的圆柱侧壁 + 底部盖子（保证正俯视时是实心黑）。
    // 复用传入的 mesh（Clear + 原地填数据），不 new，避免每帧分配造成内存泄漏。
    private void FillVoidMesh(Mesh mesh, float radius)
    {
        var verts = voidVerts; verts.Clear();
        var tris = voidTris; tris.Clear();
        var normals = voidNormals; normals.Clear();

        float topY = HoleGameManager.Instance.groundHeight;
        float botY = topY - voidDepth;

        // —— 侧壁 ——
        int wallStart = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float a = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(a);
            float z = Mathf.Sin(a);
            Vector3 n = new Vector3(x, 0f, z); // 向外

            verts.Add(new Vector3(x * radius, topY, z * radius));
            normals.Add(n);
            verts.Add(new Vector3(x * radius, botY, z * radius));
            normals.Add(n);
        }

        for (int i = 0; i < segments; i++)
        {
            int topA = wallStart + i * 2;
            int botA = topA + 1;
            int topB = topA + 2;
            int botB = topA + 3;

            tris.Add(topA); tris.Add(botA); tris.Add(botB);
            tris.Add(topA); tris.Add(botB); tris.Add(topB);
        }

        // —— 底部盖子 ——
        int capCenter = verts.Count;
        verts.Add(new Vector3(0f, botY, 0f));
        normals.Add(Vector3.down);

        for (int i = 0; i <= segments; i++)
        {
            float a = (float)i / segments * Mathf.PI * 2f;
            verts.Add(new Vector3(Mathf.Cos(a) * radius, botY, Mathf.Sin(a) * radius));
            normals.Add(Vector3.down);
        }

        for (int i = 0; i < segments; i++)
        {
            tris.Add(capCenter);
            tris.Add(capCenter + i + 1);
            tris.Add(capCenter + i + 2);
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds();
    }
}