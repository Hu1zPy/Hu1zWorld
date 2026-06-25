using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchLevel : MonoBehaviour
{
    public enum LevelType
    {
        Timer,
        Obstacle,
        Moves
    }
    protected LevelType _type;
    public LevelType Type
    {
        set => _type = value;
        get => _type;
    }
    public GameGrid GameGrid;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    protected virtual void GameWin()
    {
        GameGrid.GameOver();
        Debug.Log("===关卡胜利===");
    }

    protected virtual void GameLose()
    {
        GameGrid.GameOver();
        Debug.Log("===关卡失败===");
    }

    public virtual void OnMove()
    {
        
    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
    }
}
