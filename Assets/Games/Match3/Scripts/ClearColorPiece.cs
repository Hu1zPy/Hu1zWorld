using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearColorPiece : ClearablePiece
{
    public ColorPiece.ColorType Color { set; get; }
    
    public override void Clear()
    {
        base.Clear();
        piece.GridRef.ClearColor(Color);
    }
}
