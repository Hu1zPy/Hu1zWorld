using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public string sceneKey;

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(sceneKey);
    }
}
