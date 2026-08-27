using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    // 挂在相机或任意管理脚本上
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var parentScript = hit.collider.GetComponentInParent<GridCell>();
                if (parentScript != null)
                {
                    Debug.Log("点击Grid,ID:" + parentScript.cellId);
                    parentScript.OnCellClick();
                }
                else
                {
                    Debug.Log("点击为空");
                }
            }
        }
    }
}
