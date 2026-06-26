using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchGameOver : MonoBehaviour
{
    public GameObject screenParent;
    public GameObject scoreParent;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI titleText;
    public GameObject[] stars;

    private void Start()
    {
        screenParent.gameObject.SetActive(false);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(false);
        }
    }

    public void ShowLose(int score)
    {
        screenParent.gameObject.SetActive(true);
        scoreText.SetText(score.ToString());
        scoreText.gameObject.SetActive(true);
        
        Animator animator = GetComponent<Animator>();
        
        if (animator)
        {
            animator.Play("GameOverShow");
        }
        titleText.SetText("GAME LOSE");
    }

    public void ShowWin(int score, int star)
    {
        screenParent.gameObject.SetActive(true);
        scoreText.SetText(score.ToString());
        scoreText.gameObject.SetActive(false);
        
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.Play("GameOverShow");
        }
        titleText.SetText("GAME WIN");

        StartCoroutine(ShowStars(star));
    }

    IEnumerator ShowStars(int star)
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < stars.Length; i++)
        {
            if (i <= star)
            {
                stars[i].gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(0.5f);
        }
        scoreText.gameObject.SetActive(true);
    }

    public void OnRetryBtnClick()
    {
        AudioManager.Instance.PlayClip("load");
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void OnExitBtnClick()
    {
        AudioManager.Instance.PlayClip("load");
        SceneManager.LoadSceneAsync("MatchMainScene");
    }
}
