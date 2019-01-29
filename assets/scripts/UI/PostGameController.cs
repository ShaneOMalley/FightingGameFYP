using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PostGameController : MonoBehaviour {

    public string GameSceneName;
    public string MainMenuSceneName;

    public void Rematch()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
