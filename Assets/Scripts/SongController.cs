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

    private NoteSpawner noteSpawner;
    private AudioSource audioSource;

    private SongChart songChart;
    private Queue<Note> queuedNotes;
    private double initAudioTimeInSeconds;
    private double startAudioTimeInSeconds;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        noteSpawner = GetComponent<NoteSpawner>();

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
        double currentAudioTimeInSeconds = AudioSettings.dspTime;

        double currentPlaybackTimeInSeconds = currentAudioTimeInSeconds - startAudioTimeInSeconds;
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

        CurrentSongTime = TimeSpan.FromSeconds(currentPlaybackTimeInSeconds);
        CurrentSongProgress = (int)((currentPlaybackTimeInSeconds / audioSource.clip.length) * 100);
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
