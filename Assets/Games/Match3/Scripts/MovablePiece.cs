using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private GamePiece _gamePiece;

    private void Awake()
    {
        _gamePiece = GetComponent<GamePiece>();
    }

    public void Move(int newX, int newY)
    {
        _gamePiece.X = newX;
        _gamePiece.Y = newY;

        _gamePiece.transform.position = _gamePiece.GridRef.GetWorldPosition(newX, newY);
    }
}
