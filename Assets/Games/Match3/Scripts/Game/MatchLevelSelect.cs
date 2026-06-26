using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchLevelSelect : MonoBehaviour
{
    [Serializable]
    public struct ButtonPlayerPrefs
    {
        public GameObject button;
        public string playerPrefsKey;
    }

    public ButtonPlayerPrefs[] buttons;
    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int score = PlayerPrefs.GetInt(buttons[i].playerPrefsKey, 0);
            for (int starIndex = 1; starIndex <= 3; starIndex++)
            {
                Transform star = buttons[i].button.transform.Find("star" + starIndex);
                if (starIndex <= score)
                {
                    star.gameObject.SetActive(true);
                }
                else
                {
                    star.gameObject.SetActive(false);
                }
            }
        }
    }
    public void OnButtonClick(string levelName)
    {
        AudioManager.Instance.PlayClip("load");
        SceneManager.LoadSceneAsync(levelName);
    }
}
