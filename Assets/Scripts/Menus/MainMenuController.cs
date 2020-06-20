using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private string playSceneName;

    public void StartSong()
    {
        SceneManager.LoadScene(playSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
