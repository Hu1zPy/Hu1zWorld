using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMoves : MatchLevel
{
    public int numberMoves;
    public int targetScore;

    private int moveUsed = 0;
    private void Start()
    {
        _type = LevelType.Moves;
        matchHUD.SetScore(currentScore);
        matchHUD.SetLevelType(_type);
        matchHUD.SetRemaining(numberMoves);
        matchHUD.SetTarget(targetScore);
        //Debug.Log("===进入游戏 ：步数模式===");
        //Debug.Log("===剩余步数 ："+numberMoves+"===");
        //Debug.Log("===目标分数 ："+targetScore+"===");
    }

    public override void OnMove()
    {
        base.OnMove();
        moveUsed++;
        int remainingMoves = numberMoves - moveUsed;
        matchHUD.SetRemaining(remainingMoves);
        //Debug.Log("===当前分数为 ："+currentScore+"===");
        //Debug.Log("===剩余步数为 ："+ remainingMoves +"===");
        if (remainingMoves == 0)
        {
            if (currentScore >= targetScore)
            {
                GameWin();
            }
            else
            {
                GameLose();
            }
        }
    }
}
