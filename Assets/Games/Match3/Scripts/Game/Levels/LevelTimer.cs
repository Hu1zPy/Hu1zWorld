using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MatchLevel
{
    public float timeInSeconds;
    public int targetScore;

    private float _timer;
    private bool _timeout = false;

    private void Start()
    {
        _type = LevelType.Timer;
        
        matchHUD.SetLevelType(_type);
        matchHUD.SetScore(currentScore);
        matchHUD.SetTarget(targetScore);
        matchHUD.SetRemaining(string.Format("{0}:{1:00}",timeInSeconds / 60 ,timeInSeconds % 60));
        
        Debug.Log("===进入游戏 ：计时模式===");
        Debug.Log("===当前分数为 ："+currentScore+"===");
        Debug.Log("===目标分数为 ："+ targetScore +"===");
        StartCoroutine(ShowRemainingTime());
    }

    private void Update()
    {
        if (!_timeout)
        {
            _timer += Time.deltaTime;
            if (timeInSeconds - _timer <= 0)
            {
                if (currentScore >= targetScore)
                {
                    GameWin();
                }
                else
                {
                    GameLose();
                }

                _timeout = true;
            }
        }
    }

    IEnumerator ShowRemainingTime()
    {
        while (!_timeout)
        {
            yield return new WaitForSeconds(1f);
            float remainingTime = timeInSeconds - _timer;
            
            matchHUD.SetRemaining(string.Format("{0}:{1:00}",(int)(remainingTime / 60) ,(int)(remainingTime % 60)));
            Debug.Log("===剩余时间为 ："+ remainingTime +"===");
        }
    }

    public override void OnPieceCleared(GamePiece piece)
    {
        base.OnPieceCleared(piece);
        matchHUD.SetScore(currentScore);
    }
}
