using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObstacle : MatchLevel
{
    public int numberMoves;
    public GameGrid.PieceType[] obstacles;

    private int _moveUsed = 0;
    private int _obstaclesLeft;

    private void Start()
    {
        _type = LevelType.Obstacle;
        for (int i = 0; i < obstacles.Length; i++)
        {
            _obstaclesLeft += gameGrid.GetPiecesOfType(obstacles[i]).Count;
        }
    }

    public override void OnMove()
    {
        base.OnMove();
        _moveUsed++;
        int remainingMoves = numberMoves - _moveUsed;
        
        Debug.Log("===当前分数为 ："+currentScore+"===");
        Debug.Log("===剩余步数为 ："+ remainingMoves +"===");
        Debug.Log("===剩余障碍数为 ："+ _obstaclesLeft +"===");
        
        if (remainingMoves == 0 && _obstaclesLeft > 0)
        {
            GameLose();
        }
    }

    public override void OnPieceCleared(GamePiece piece)
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (piece.PieceType == obstacles[i])
            {
                _obstaclesLeft--;
                int remainingMoves = numberMoves - _moveUsed;
                
                Debug.Log("===当前分数为 ："+currentScore+"===");
                Debug.Log("===剩余步数为 ："+ remainingMoves +"===");
                Debug.Log("===剩余障碍数为 ："+ _obstaclesLeft +"===");
                
                if (_obstaclesLeft == 0)
                {
                    currentScore += 1000 * remainingMoves;
                    GameWin();
                }
            }
        }
    }
}
