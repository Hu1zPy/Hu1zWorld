using System;
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
    public GameGrid gameGrid;
    public MatchHUD matchHUD;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    private void Start()
    {
        matchHUD.SetScore(currentScore);
    }

    protected virtual void GameWin()
    {
        gameGrid.GameOver();
        matchHUD.OnGameWin(currentScore);
        Debug.Log("===关卡胜利===");
    }

    protected virtual void GameLose()
    {
        gameGrid.GameOver();
        matchHUD.OnGameLose();
        Debug.Log("===关卡失败===");
    }

    public virtual void OnMove()
    {
        
    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        matchHUD.SetScore(currentScore);
    }
}
