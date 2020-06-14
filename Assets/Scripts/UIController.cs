using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text scoreText;
    
    [SerializeField]
    private Text multiplierText;
    
    [SerializeField]
    private Text songTimeText;

    [SerializeField]
    private Text songProgressText;

    private SongController SongController { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        SongController = GetComponent<SongController>();
        if (SongController == null)
        {
            throw new Exception("Unable to find a SongController component");
        }

        scoreText.text = "0";
        multiplierText.text = "x1";
        songTimeText.text = "0:00.00";
        songProgressText.text = "0%";
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = SongController.CurrentScore.ToString();
        multiplierText.text = "x" + SongController.CurrentMultiplier.ToString();
        songTimeText.text = SongController.CurrentSongTime.ToString(@"m\:ss\.ff");
        songProgressText.text = SongController.CurrentSongProgress.ToString() + "%";
    }
}
