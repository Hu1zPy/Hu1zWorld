using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int cellId;
    public Vector2Int girdPos;
    public Arrow occupiedBy;

    public static Action<GridCell> OnCellPointerClicked;

    public bool IsOccupied() => occupiedBy != null;
    public void SetOccupied(Arrow arrow) => occupiedBy = arrow;
    public void SetEmpty() => occupiedBy = null;

    public void OnCellClick()
    {
        OnCellPointerClicked?.Invoke(this);
    }
   
}
