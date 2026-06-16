using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    private int _x;
    private int _y;
    private Grid.PieceType _type;
    private Grid _grid;
    private MovablePiece _movablePiece;
    private ColorPiece _colorPiece;

    public int X
    {
        set
        {
            if (IsMovable())
            {
                _x = value;
            }
        }
        get => _x;
    }

    public int Y
    {
        set
        {
            if (IsMovable())
            {
                _y = value;
            }
        }
        get => _y;
    }
    public Grid GridRef => _grid;

    public Grid.PieceType PieceType => _type;
    public MovablePiece MovablePieceRef => _movablePiece;
    public ColorPiece ColorPieceRef => _colorPiece;

    public void Init(int x, int y, Grid grid, Grid.PieceType type)
    {
        _x = x;
        _y = y;
        _grid = grid;
        _type = type;
    }

    private void Awake()
    {
        _movablePiece = transform.GetComponent<MovablePiece>();
        _colorPiece = transform.GetComponent<ColorPiece>();
    }

    public bool IsMovable()
    {
        return _movablePiece != null;
    }

    public bool IsColored()
    {
        return _colorPiece != null;
    }

    private void OnMouseEnter()
    {
        _grid.EnterPiece(this);
    }

    private void OnMouseDown()
    {
        _grid.PressPiece(this);
    }
    
    private void OnMouseUp()
    {
        _grid.ReleasePiece();
    }
}
