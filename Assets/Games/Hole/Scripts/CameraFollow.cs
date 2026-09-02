using UnityEngine;

/// <summary>
/// 跟随相机：正交俯视，始终看向黑洞（黑洞始终在画面中心），并随半径增大自动拉远视野。
/// 挂到场景的主摄像机上。
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("俯视角度")]
    public float height = 22f;   // 相机相对黑洞的高度
    public float back = 2f;      // 相机相对黑洞向后（-Z）的水平距离；0 = 正俯视，越大越斜

    [Header("缩放（正交）")]
    public float minSize = 6f;       // 初始视野
    public float maxSize = 16f;      // 最大视野（黑洞最大时的视野）

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        //cam.orthographic = true;
    }

    private void LateUpdate()
    {
        var hole = HoleController.Instance;
        if (hole == null) return;

        // 相机位于黑洞的斜后上方
        Vector3 targetPos = hole.transform.position + new Vector3(0f, height, -back);
        transform.position = Vector3.Lerp(transform.position, targetPos, 8f * Time.deltaTime);

        // 始终看向黑洞，保证它始终在画面中心
        Vector3 lookDir = hole.transform.position - transform.position;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }

        // 黑洞越大，视野越远
        float t = Mathf.InverseLerp(hole.startRadius, hole.maxRadius, hole.Radius);
        cam.orthographicSize = Mathf.Lerp(minSize, maxSize, t);
    }
}
