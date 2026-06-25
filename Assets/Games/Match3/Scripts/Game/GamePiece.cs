using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int score;
    private int _x;
    private int _y;
    private GameGrid.PieceType _type;
    private GameGrid _grid;
    private MovablePiece _movablePiece;
    private ColorPiece _colorPiece;
    private ClearablePiece _clearablePiece;

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
    public GameGrid GridRef => _grid;

    public GameGrid.PieceType PieceType => _type;
    public MovablePiece MovablePieceRef => _movablePiece;
    public ColorPiece ColorPieceRef => _colorPiece;
    public ClearablePiece ClearablePieceRef => _clearablePiece;

    public void Init(int x, int y, GameGrid grid, GameGrid.PieceType type)
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
        _clearablePiece = transform.GetComponent<ClearablePiece>();
    }

    public bool IsMovable()
    {
        return _movablePiece != null;
    }

    public bool IsColored()
    {
        return _colorPiece != null;
    }

    public bool IsClearable()
    {
        return _clearablePiece != null;
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
