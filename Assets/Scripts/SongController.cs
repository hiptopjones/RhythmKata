using Assets.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongController : MonoBehaviour
{
    public int CurrentScore { get; set; } = 0;
    public int CurrentMultiplier { get; set; } = 1;
    public TimeSpan CurrentSongTime { get; set; } = TimeSpan.FromSeconds(0);
    public int CurrentSongProgress { get; set; } = 0;

    [SerializeField]
    private double startPlaybackDelayTimeInSeconds = 2;

    [SerializeField]
    private double spawnAheadTimeInSeconds = 2;

    [SerializeField]
    private double calibrationOffsetTimeInSeconds = 0.58;

    [SerializeField]
    private double hitThresholdInSeconds = 0.05;

    [SerializeField]
    private int noteHitScore = 100;

    [SerializeField]
    private GameObject spawnParent;

    [SerializeField]
    private TextAsset songChartTextAsset;

    [SerializeField]
    private GameObject endScreen;

    private NoteSpawner noteSpawner;
    private AudioSource audioSource;

    private SongChart songChart;
    private Queue<Note> queuedNotes;
    private double initAudioTimeInSeconds;
    private double startAudioTimeInSeconds;
    private double previousAudioTimeInSeconds;

    private bool isPaused;
    private bool isComplete;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure any lingering pause state is cleared
        AudioListener.pause = false;
        Time.timeScale = 1;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            throw new Exception($"Unable to find {nameof(AudioSource)} component");
        }

        noteSpawner = GetComponent<NoteSpawner>();
        if (noteSpawner == null)
        {
            throw new Exception($"Unable to find {nameof(NoteSpawner)} component");
        }

        SongChartParser parser = new SongChartParser(songChartTextAsset.text);
        songChart = parser.ParseChart();
        if (songChart == null)
        {
            throw new Exception($"Unable to parse song chart");
        }

        // Use a queue to consume notes as we spawn them
        queuedNotes = new Queue<Note>(songChart.Notes);

        StartSong();
    }

    // Update is called once per frame
    void Update()
    {
        // Check pause state
        if (Time.timeScale == 0)
        {
            if (!isPaused)
            {
                isPaused = true;
                AudioListener.pause = true;
            }

            return;
        }
        else
        {
            if (isPaused)
            {
                isPaused = false;
                AudioListener.pause = false;
            }
        }

        // Check if audio source is finished
        if (audioSource.time >= audioSource.clip.length)
        {
            isComplete = true;
            endScreen.SetActive(true);
            return;
        }

        // DSP time may remain the same over multiple calls to Update(), so this code tweens it
        double currentAudioTimeInSeconds = AudioSettings.dspTime;
        if (currentAudioTimeInSeconds == previousAudioTimeInSeconds)
        {
            currentAudioTimeInSeconds += Time.unscaledDeltaTime;
        }

        previousAudioTimeInSeconds = currentAudioTimeInSeconds;

        // Check if we have played the whole clip
        double currentPlaybackTimeInSeconds = currentAudioTimeInSeconds - startAudioTimeInSeconds;
        if (currentPlaybackTimeInSeconds > audioSource.clip.length)
        {
            isComplete = true;
            endScreen.SetActive(true);
            return;
        }

        double currentSpawnTimeInSeconds = currentPlaybackTimeInSeconds + spawnAheadTimeInSeconds;
        int currentSpawnPositionInSteps = songChart.GetPositionFromTime(currentSpawnTimeInSeconds);

        // Spawn any notes that have come into range
        while (queuedNotes.Count > 0 && queuedNotes.Peek().PositionInSteps <= currentSpawnPositionInSteps)
        {
            Note note = queuedNotes.Dequeue();
            double noteTimeInSeconds = songChart.GetTimeFromPosition(note.PositionInSteps);
            double calibratedNoteTimeInSeconds = noteTimeInSeconds + calibrationOffsetTimeInSeconds;

            noteSpawner.SpawnNote(note, calibratedNoteTimeInSeconds, startAudioTimeInSeconds, OnNoteHit);
        }

        NoteController[] noteControllers = spawnParent.GetComponentsInChildren<NoteController>();
        foreach (NoteController noteController in noteControllers)
        {
            double noteOffsetInSeconds = Math.Abs(currentPlaybackTimeInSeconds - noteController.noteTimeInSeconds);
            if (noteOffsetInSeconds < hitThresholdInSeconds)
            {
                // Inside the target zone, so notify the note if it was previously outside
                if (!noteController.InTargetZone)
                {
                    Debug.Log("Enter target zone: " + noteController.name);
                    noteController.OnTargetZoneEnter();
                }
            }
            else
            {
                // Outside the target zone, so notify the note if it was previously inside
                if (noteController.InTargetZone)
                {
                    Debug.Log("Leave target zone: " + noteController.name);
                    noteController.OnTargetZoneExit();
                }
            }
        }

        if (currentPlaybackTimeInSeconds >= 0)
        {
            CurrentSongTime = TimeSpan.FromSeconds(currentPlaybackTimeInSeconds);
            CurrentSongProgress = (int)((currentPlaybackTimeInSeconds / audioSource.clip.length) * 100);
        }
    }

    public void StartSong()
    {
        initAudioTimeInSeconds = AudioSettings.dspTime;
        startAudioTimeInSeconds = initAudioTimeInSeconds + startPlaybackDelayTimeInSeconds;
        audioSource.PlayScheduled(startAudioTimeInSeconds);
    }

    private void OnNoteHit()
    {
        CurrentScore += noteHitScore;
    }
}
