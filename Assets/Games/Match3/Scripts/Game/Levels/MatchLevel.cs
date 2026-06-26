using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private bool didWin = false;
    private void Start()
    {
        matchHUD.SetScore(currentScore);
    }

    protected virtual void GameWin()
    {
        gameGrid.GameOver();
        AudioManager.Instance.PlayClip("gameover");
        didWin = true;
        StartCoroutine(WaitForGridFill());
    }

    protected virtual void GameLose()
    {
        gameGrid.GameOver();
        didWin = false;
        StartCoroutine(WaitForGridFill());
    }

    public virtual void OnMove()
    {
        
    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        matchHUD.SetScore(currentScore);
    }

    IEnumerator WaitForGridFill()
    {
        if (gameGrid.IsFilling)
        {
            yield return 0;
        }

        if (didWin)
        {
            matchHUD.OnGameWin(currentScore);
        }
        else
        {
            matchHUD.OnGameLose(currentScore);
        }
    }
}
