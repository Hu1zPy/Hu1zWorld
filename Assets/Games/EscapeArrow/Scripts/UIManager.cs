using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("LEVEL STATE")]
    public TMP_Text levelText;
    public TMP_Text hpText;
    [Header("LEVEL FAILED")] 
    public GameObject failedPanel;
    public Button replayBtn;
    [Header("LEVEL WIN")]
    public GameObject winPanel;
    public Button goBtn;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        replayBtn.onClick.AddListener(OnReplayButtonClick);
        goBtn.onClick.AddListener(OnGoButtonClick);
    }

    public void UpdateHeader(int level, int hp)
    {
        levelText.text = "Level : " + level;
        hpText.text = "HP : " + hp;
    }

    public void ShowWin()
    {
        winPanel.gameObject.SetActive(true);
    }

    public void ShowFailed()
    {
        failedPanel.gameObject.SetActive(true);
    }

    void OnGoButtonClick()
    {
        StartCoroutine(LevelManager.Instance.LoadNextLevelCoroutine());
        winPanel.gameObject.SetActive(false);
    }

    void OnReplayButtonClick()
    {
        LevelManager.Instance.Replay();
        failedPanel.gameObject.SetActive(false);
    }
}
