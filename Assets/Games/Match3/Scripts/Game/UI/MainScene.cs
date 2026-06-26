using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public string sceneKey;

    public void StartGame()
    {
        AudioManager.Instance.PlayClip("load");
        SceneManager.LoadSceneAsync(sceneKey);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        
    }
}
