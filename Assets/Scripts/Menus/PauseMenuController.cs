using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField]
    private string mainMenuSceneName;

    [SerializeField]
    private GameObject pauseScreen;

    private bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeSong();
            }
            else
            {
                PauseSong();
            }
        }
    }

    public void PauseSong()
    {
        isPaused = true;
        pauseScreen.SetActive(isPaused);

        // Hacky way to send an event to song controller
        Time.timeScale = 0;
    }

    public void ResumeSong()
    {
        isPaused = false;
        pauseScreen.SetActive(isPaused);

        // Hacky way to send an event to song controller
        Time.timeScale = 1;
    }

    public void RestartSong()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void QuitSong()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
