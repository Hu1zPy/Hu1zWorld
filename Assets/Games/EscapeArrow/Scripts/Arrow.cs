using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public List<GridCell> placeCells = new List<GridCell>();
    public Vector2Int arrowDirection;
    public bool isEscape;
    private GridManager _gridManager;
    private ArrowManager _arrowManager;
    public LineRenderer lineRenderer;

    public event System.Action<Arrow> OnEscaped, OnFailed;

    private void Awake()
    {
        _gridManager = LevelManager.Instance.gridManager;
        _arrowManager = LevelManager.Instance.arrowManager;
    }

    public void SetUpFromData(List<int> cellIds)
    {
        placeCells.Clear();
        foreach (var id in cellIds)
        {
            var c = _gridManager.IDMap[id];
            placeCells.Add(c);
            c.SetOccupied(this);
        }
        arrowDirection = placeCells[0].girdPos - placeCells[1].girdPos;
        transform.position = placeCells[0].transform.position;
        
        GetComponent<ArrowRenderer>().Setup();
    }

    public bool CanEscape(out List<GridCell> path)
    {
        path = new List<GridCell>();
        var pos = placeCells[0].girdPos;
        int safety = 0;
        while (safety++ < 5000)
        {
            pos += arrowDirection;
            var next = _gridManager.GetCell(pos);
            if (next == null) return true;
            path.Add(next);
            if (next.IsOccupied()) return false;
        }

        return false;
    }

    public void EscapeArrow()
    {
        foreach (var c in placeCells)
        {
            c.SetEmpty();
        }
        //todo 逃离动画
        isEscape = true;
        StartCoroutine(EscapeAnimation(_arrowManager.moveTime));
        //gameObject.SetActive(false);
        LevelManager.Instance.OnArrowEscape();
    }

    public void EscapeFailed(List<GridCell> path)
    {
        //todo 弹回动画
        StartCoroutine(FailedEscapeAnimation(path,_arrowManager.moveTime));
        //OnFailed?.Invoke(this);
        LevelManager.Instance.OnArrowFailed();
    }
    
    // ===================== 成功逃逸：蛇形冲出棋盘 =====================
    // moveTime: 总时长（秒），比如 0.5
    public IEnumerator EscapeAnimation(float moveTime)
    {
        // 当前蛇身：占用的格子中心坐标（[0] = 头）
        List<Vector3> positions = new List<Vector3>();
        foreach (var c in placeCells) positions.Add(c.transform.position);

        Vector3 dir = new Vector3(arrowDirection.x, 0, arrowDirection.y);
        int totalStep = placeCells.Count + 5;   // 长度 + 出界余量（多走5格飞出屏幕）
        float stepTime = moveTime / totalStep;   // 每格耗时
        ArrowRenderer render = GetComponent<ArrowRenderer>();

        for (int step = 0; step < totalStep; step++)
        {
            Vector3 next = positions[0] + dir;   // 下一格 = 当前头 + 方向

            // 动画缓冲：比蛇身多一个点（头伸出去 + 尾还没收完）
            List<Vector3> buffer = new List<Vector3> { positions[0] };
            buffer.AddRange(positions);

            // 这一步的插值动画
            float elapsed = 0f;
            while (elapsed < stepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepTime);

                buffer[0] = Vector3.Lerp(positions[0], next, t);            // 头：向前一格
                buffer[^1] = Vector3.Lerp(positions[^1], positions[^2], t); // 尾：收缩一格
                render.DrawLine(buffer);                                    // 每帧重画
                yield return null;
            }

            // 提交：头插入新点，尾移除 → 蛇整体前进一格
            positions.Insert(0, next);
            positions.RemoveAt(positions.Count - 1);
            render.DrawLine(positions);   // 画一次干净的提交状态
        }

        render.Hide();   // 全部走完，隐藏箭头
    }

    // ===================== 失败弹回：冲过去再退回来 =====================
    // gridPath: CanEscape 的出参（被挡前走过的格子）
    public IEnumerator FailedEscapeAnimation(List<GridCell> gridPath, float moveTime)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var c in placeCells) positions.Add(c.transform.position);

        Vector3 dir = new Vector3(arrowDirection.x, 0, arrowDirection.y);
        int steps = gridPath.Count;                            // 要撞过去的格数
        float stepTime = (moveTime * 0.5f) / Mathf.Max(1, steps); // 半程给前进，半程给弹回
        ArrowRenderer render = GetComponent<ArrowRenderer>();

        // ① 前进：头一格一格冲，尾跟着收缩
        for (int s = 0; s < steps; s++)
        {
            Vector3 next = positions[0] + dir;
            yield return MoveOneStepForward(positions, next, stepTime, render);
        }

        // ② 弹回：头一格一格退，尾长回原样
        for (int s = 0; s < steps; s++)
        {
            Vector3 nextTail = positions[^1] - dir;    // 尾部要长回的格子（原尾部方向）
            yield return MoveOneStepBackward(positions, nextTail, stepTime, render);
        }
    }

    // ---------- 私有辅助：前进一步 ----------
    IEnumerator MoveOneStepForward(List<Vector3> positions, Vector3 next,
                                   float stepTime, ArrowRenderer render)
    {
        List<Vector3> buffer = new List<Vector3> { positions[0] };
        buffer.AddRange(positions);

        float elapsed = 0f;
        while (elapsed < stepTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stepTime);

            buffer[0] = Vector3.Lerp(positions[0], next, t);
            buffer[^1] = Vector3.Lerp(positions[^1], positions[^2], t);
            render.DrawLine(buffer);
            yield return null;
        }

        positions.Insert(0, next);                   // 头到位
        positions.RemoveAt(positions.Count - 1);     // 尾移除
        render.DrawLine(positions);
    }

    // ---------- 私有辅助：后退一步（尾长回） ----------
    IEnumerator MoveOneStepBackward(List<Vector3> positions, Vector3 nextTail,
                                    float stepTime, ArrowRenderer render)
    {
        // 缓冲：尾部多加一个点（尾巴长出来的那部分）
        List<Vector3> buffer = new List<Vector3>(positions);
        buffer.Add(positions[^1]);

        float elapsed = 0f;
        while (elapsed < stepTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stepTime);

            buffer[0] = Vector3.Lerp(positions[0], positions[1], t);      // 头：向后退
            buffer[^1] = Vector3.Lerp(positions[^1], nextTail, t);        // 尾：向外长回
            render.DrawLine(buffer);
            yield return null;
        }

        positions.RemoveAt(0);       // 头移除
        positions.Add(nextTail);     // 尾长回
        render.DrawLine(positions);
    }
}
