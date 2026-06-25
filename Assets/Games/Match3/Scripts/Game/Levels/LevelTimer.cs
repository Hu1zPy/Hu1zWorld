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
            Debug.Log("===剩余时间为 ："+ remainingTime +"===");
        }
    }

    public override void OnPieceCleared(GamePiece piece)
    {
        base.OnPieceCleared(piece);
        
        float remainingTime = timeInSeconds - _timer;
        Debug.Log("===当前分数为 ："+ currentScore +"===");
        Debug.Log("===剩余时间为 ："+ remainingTime +"===");
    }
}
