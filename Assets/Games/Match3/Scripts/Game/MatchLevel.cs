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

    public virtual void GameWin()
    {
        GameGrid.GameOver();
    }

    public virtual void GameLose()
    {
        
    }

    public virtual void OnMove()
    {
        
    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        Debug.Log("测试---当前分数为：" + currentScore);
    }
}
