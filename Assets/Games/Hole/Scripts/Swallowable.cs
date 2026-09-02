using UnityEngine;

/// <summary>
/// 可被黑洞吞噬的物体。用真实物理实现「倾斜后掉入洞口」（对应源项目 PropBehavior 的做法）：
/// 物体是 Rigidbody，初始 isKinematic = true 冻在原位。碰撞体【始终开启】，只靠 kinematic 冻结，
/// 这样静止的物体对掉落的动态物体是实心障碍，不会互相穿模；当黑洞靠近（半径 × 1.1）时切回动态刚体、
/// 重力接管，物体失去地面支撑后自然倾倒、翻滚着掉进洞口。
/// 掉到地面以下（说明真的进了洞）才会被判定为吞下。
/// 物体比洞大时会卡在洞边/落在地面上掉不下去 —— 大小门槛是物理涌现出来的，无需显式判断。
/// </summary>
public class Swallowable : MonoBehaviour
{
    [Header("属性")]
    public float size = 1f;         // 物体尺寸（影响能否从洞口掉下去）
    public int xpReward = 1;        // 吞下后给的经验

    [Header("吞入判定")]
    public float eatenYOffset = -30f; // 掉到「地面 Y + 该值」以下判定为已吞

    public bool IsActive { get; private set; }
    public bool IsEaten { get; private set; }

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // 碰撞体【始终开启】，只靠 isKinematic 冻结：静止的物体不受重力、不动，
        // 但会挡住掉落中的动态物体（不会互相穿模）。黑洞靠近后切回动态刚体即可。
        rb.isKinematic = true;
    }

    public void Init(float size)
    {
        this.size = size;
        xpReward = Mathf.Max(1, Mathf.RoundToInt(size * 2f)); // 越大经验越多
        transform.localScale = Vector3.one * size;
    }

    private void Update()
    {
        var hole = HoleController.Instance;
        if (hole == null || IsEaten) return;

        if (!IsActive)
        {
            // 黑洞靠近（中心距 < 半径 × 1.1）→ 激活物理，重力接管。
            // 物体变动态后，脚下的支撑面是【重新烘焙过的】真实开洞地面，
            // 走到洞边失去支撑，自然倾倒、翻滚着掉进洞口。
            Vector3 d = transform.position - hole.transform.position;
            if (d.x * d.x + d.z * d.z < hole.Radius * hole.Radius * 1.1f)
            {
                IsActive = true;
                rb.isKinematic = false; // 变动态 → 受重力掉落
                rb.WakeUp();            // 确保从冻结/休眠状态醒来，立刻响应重力
            }
        }
        else if (transform.position.y < HoleGameManager.Instance.groundHeight + eatenYOffset)
        {
            // 掉到地面以下（真的进了洞）→ 判定吞下
            IsEaten = true;

            hole.AddXp(xpReward);
            HoleGameManager.Instance.OnPropConsumed(this);
        }
    }
}