using Assets.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongController : MonoBehaviour
{
    [SerializeField]
    private double startPlaybackDelayTimeInSeconds = 2;

    [SerializeField]
    private double spawnAheadTimeInSeconds = 5;

    [SerializeField]
    private double calibrationOffsetTimeInSeconds = 0.58;

    [SerializeField]
    private TextAsset songChartTextAsset;

    private NoteSpawner noteSpawner;
    private AudioSource audioSource;

    private SongChart songChart;
    private Queue<Note> notesQueue;
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
        notesQueue = new Queue<Note>(songChart.Notes);

        StartSong();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Time: {Time.time} Delta Time: {Time.deltaTime} DSP Time: {AudioSettings.dspTime}");

        double currentAudioTimeInSeconds = AudioSettings.dspTime;

        double currentPlaybackTimeInSeconds = currentAudioTimeInSeconds - startAudioTimeInSeconds;
        double currentSpawnTimeInSeconds = currentPlaybackTimeInSeconds + spawnAheadTimeInSeconds;
        int currentSpawnPositionInSteps = songChart.GetPositionFromTime(currentSpawnTimeInSeconds);

        Debug.Log($"Playback Time: {currentPlaybackTimeInSeconds} Spawn Time: {currentSpawnTimeInSeconds} Spawn Position: {currentSpawnPositionInSteps}");

        while (notesQueue.Count > 0 && notesQueue.Peek().PositionInSteps <= currentSpawnPositionInSteps)
        {
            Note note = notesQueue.Dequeue();
            double noteTimeInSeconds = songChart.GetTimeFromPosition(note.PositionInSteps);
            double calibratedNoteTimeInSeconds = noteTimeInSeconds + calibrationOffsetTimeInSeconds;

            Debug.Log($"Note Position: {note.PositionInSteps} Note Time: {noteTimeInSeconds} Calibrated Time: {calibratedNoteTimeInSeconds}");
            noteSpawner.SpawnNote(note, calibratedNoteTimeInSeconds, startAudioTimeInSeconds);
        }

        // Add fake notes on space bar hits to allow testing the tempo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Note fakeNote = new Note(songChart.GetPositionFromTime(currentPlaybackTimeInSeconds), -1, 0);
            double noteTimeInSeconds = songChart.GetTimeFromPosition(fakeNote.PositionInSteps);
            noteSpawner.SpawnNote(fakeNote, noteTimeInSeconds, startAudioTimeInSeconds);
        }
    }

    public void StartSong()
    {
        initAudioTimeInSeconds = AudioSettings.dspTime;
        startAudioTimeInSeconds = initAudioTimeInSeconds + startPlaybackDelayTimeInSeconds;
        audioSource.PlayScheduled(startAudioTimeInSeconds);
    }
}
