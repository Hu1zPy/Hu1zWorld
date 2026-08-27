using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour
{
// 简易版箭头渲染器：把 Arrow 占用的格子画成一条线 + 头尾贴图
// 设计原则：不做事件订阅，由外部主动调用（Setup / DrawLine）
    [Header("引用（Prefab 里拖好）")]
    public Arrow arrow;                    // 本物体上的 Arrow
    public LineRenderer lineRenderer;      // 身体：一条线
    public Transform headTransform;        // 头部挂点（子物体 Head）
    public Transform tailTransform;        // 尾部挂点（子物体 Tail）
    public SpriteRenderer headSprite;      // 头部尖角贴图
    public SpriteRenderer tailSprite;      // 尾部贴图

    [Header("外观")]
    public float lineWidth = 0.2f;         // 线条粗细

    private void Awake()
    {
        // 引用自动补齐，防止漏拖
        if (arrow == null) arrow = GetComponent<Arrow>();
        if (lineRenderer == null) lineRenderer = GetComponentInChildren<LineRenderer>();

        lineRenderer.useWorldSpace = true; // 直接用世界坐标
        lineRenderer.loop = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;    // 先不画，等数据来
    }

    // ================= 入口 1：初始化（生成箭头后调用一次） =================
    public void Setup()
    {
        // 把占用的格子中心点收集成路径
        List<Vector3> points = new List<Vector3>();
        foreach (var cell in arrow.placeCells)
            points.Add(cell.transform.position);

        DrawLine(points);
    }

    // ================= 入口 2：画线（动画每帧也调它） =================
    public void DrawLine(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
        {
            lineRenderer.enabled = false;   // 点数不够就不画
            return;
        }

        // ① 身体：把点写进 LineRenderer
        lineRenderer.enabled = true;
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        // ② 头部：贴到第一个点，朝向箭头方向
        headTransform.position = points[0];
        float angle = Mathf.Atan2(arrow.arrowDirection.x, arrow.arrowDirection.y) * Mathf.Rad2Deg;
        headTransform.rotation = Quaternion.Euler(90, angle, 0);   // 只转 Y 轴

        // ③ 尾部：贴到最后一个点
        tailTransform.position = points[^1];
    }

    // ================= 附带：整体染色（可选） =================
    public void SetColor(Color c)
    {
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
        headSprite.color = c;
        tailSprite.color = c;
    }

    // ================= 附带：隐藏（逃逸完成后调用） =================
    public void Hide()
    {
        lineRenderer.enabled = false;
        headTransform.gameObject.SetActive(false);
        tailTransform.gameObject.SetActive(false);
    }
}

