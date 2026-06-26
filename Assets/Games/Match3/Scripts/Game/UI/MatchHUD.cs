using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchHUD : MonoBehaviour
{
    public MatchLevel level;
    public MatchGameOver gameOver;
    
    public TextMeshProUGUI remainingNumber;
    public TextMeshProUGUI remainingSubText;
    public TextMeshProUGUI targetNumber;
    public TextMeshProUGUI targetSubText;
    public TextMeshProUGUI score;
    public GameObject[] stars;

    private int starIndex = 0;

    private void Awake()
    {
        stars[0].gameObject.SetActive(true);
        for (int i = 0; i < stars.Length; i++)
        {
            if (starIndex >= i)
            {
                stars[i].gameObject.SetActive(true);
            }
            else
            {
                stars[i].gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM("gamebgm");
    }

    public void SetScore(int number)
    {
        score.SetText(number.ToString());

        if (number >= level.score1Star && number < level.score2Star)
        {
            starIndex = 1;
        }else if (number >= level.score2Star && number < level.score3Star)
        {
            starIndex = 2;
        }else if (number >= level.score3Star)
        {
            starIndex = 3;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (starIndex >= i)
            {
                stars[i].gameObject.SetActive(true);
            }
            else
            {
                stars[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetTarget(int number)
    {
        targetNumber.SetText(number.ToString());
    }

    public void SetRemaining(int number)
    {
        remainingNumber.SetText(number.ToString());
    }
    
    public void SetRemaining(string timer)
    {
        remainingNumber.SetText(timer);
    }

    public void SetLevelType(MatchLevel.LevelType type)
    {
        switch (type)
        {
            case MatchLevel.LevelType.Moves:
                remainingSubText.SetText("Remaining\nMoves");
                targetSubText.SetText("Target\nScore");
                break;
            case MatchLevel.LevelType.Obstacle:
                remainingSubText.SetText("Remaining\nMoves");
                targetSubText.SetText("Bubbles\nRemaining");
                break;
            case MatchLevel.LevelType.Timer:
                remainingSubText.SetText("Remaining\nTime");
                targetSubText.SetText("Target\nScore");
                break;
        }
    }

    public void OnGameWin(int scores)
    {
        gameOver.ShowWin(scores,starIndex);
        if (starIndex > PlayerPrefs.GetInt(SceneManager.GetActiveScene().name, 0))
        {
            PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, starIndex);
        }
    }

    public void OnGameLose(int scores)
    {
        gameOver.ShowLose(scores);
    }

    public void BackToSelect()
    {
        AudioManager.Instance.PlayClip("load");
        AudioManager.Instance.PlayBGM("mainbgm");
        SceneManager.LoadSceneAsync("MatchLevelSelect");
    }
}
