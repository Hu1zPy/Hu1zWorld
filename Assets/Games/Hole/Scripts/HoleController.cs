using UnityEngine;

/// <summary>
/// 黑洞玩家：处理移动输入、经验/升级、半径增长。
/// 挂到一个空物体上即可（视觉由 HoleGround 负责，它会每帧读 Radius）。
/// </summary>
public class HoleController : MonoBehaviour
{
    public static HoleController Instance { get; private set; }

    [Header("移动")]
    public float moveSpeed = 10f;            // 最大移动速度
    public float acceleration = 45f;         // 加速/跟手度（越大越跟手）
    public float touchSensitivity = 0.03f;   // 触摸拖拽灵敏度（每 1 像素 ≈ 多少世界单位）

    [Header("黑洞属性")]
    public float startRadius = 2f;           // 初始半径
    public float radiusPerLevel = 0.6f;      // 每升 1 级增加的半径
    public float maxRadius = 14f;            // 半径上限

    [Header("经验/升级")]
    public int startXpToLevel = 4;           // 升到 2 级所需经验
    public float xpCurve = 1.35f;            // 之后每级所需经验 = 上一级 × 该倍率

    public float Radius { get; private set; }
    public int Level { get; private set; } = 1;
    public int XP { get; private set; }
    public int XpToLevel { get; private set; }

    private Vector3 velocity;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Level = 1;
        XP = 0;
        Radius = startRadius;
        XpToLevel = startXpToLevel;
        transform.position = new Vector3(0f, 0f, 0f);
    }

    private void Update()
    {
        Move();

        // 按 R 重开（方便测试）
        if (Input.GetKeyDown(KeyCode.R)) Restart();
    }

    private void Move()
    {
        Vector3 desired = GetDesiredVelocity();

        // 平滑过渡到目标速度，避免瞬间启动/停止的生硬感
        velocity = Vector3.Lerp(velocity, desired, acceleration * Time.deltaTime);

        Vector3 pos = transform.position + velocity * Time.deltaTime;

        // 限制在场地内
        pos.x = Mathf.Clamp(pos.x, HoleGameManager.Min.x, HoleGameManager.Max.x);
        pos.z = Mathf.Clamp(pos.z, HoleGameManager.Min.y, HoleGameManager.Max.y);

        transform.position = pos;
    }

    private Vector3 GetDesiredVelocity()
    {
        Vector3 v = Vector3.zero;

        // 键盘：WASD / 方向键
        Vector2 key = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (key.sqrMagnitude > 1f) key.Normalize();          // 斜向不超过满速
        v += new Vector3(key.x, 0f, key.y) * moveSpeed;

        // 触摸拖拽（移动端）：把手指的像素增量换算成世界速度
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            v += new Vector3(t.deltaPosition.x, 0f, t.deltaPosition.y) * touchSensitivity / dt;
        }

        // 防止极端帧率下触摸产生过大速度
        v = Vector3.ClampMagnitude(v, moveSpeed * 2f);
        return v;
    }

    /// <summary>物体被吞时调用，增加经验并触发升级。</summary>
    public void AddXp(int amount)
    {
        XP += amount;

        while (XP >= XpToLevel)
        {
            XP -= XpToLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        Radius = Mathf.Min(Radius + radiusPerLevel, maxRadius);
        XpToLevel = Mathf.RoundToInt(startXpToLevel * Mathf.Pow(xpCurve, Level - 1));

        // 开洞地面和深渊由 HoleGround 每帧读 Radius 自动重建，无需在此通知

        Debug.Log($"升级！等级 {Level}，黑洞半径 {Radius:0.0}");
    }

    private void Restart()
    {
        Level = 1;
        XP = 0;
        Radius = startRadius;
        XpToLevel = startXpToLevel;
        velocity = Vector3.zero;
        transform.position = new Vector3(0f, 0f, 0f);

        HoleGameManager.Instance.ResetField();
    }
}
