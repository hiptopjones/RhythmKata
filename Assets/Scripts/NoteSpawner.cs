using Assets.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnParent;

    [SerializeField]
    private GameObject spawnPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SpawnNote(Note note, double noteTimeInSeconds, double startAudioTimeInSeconds)
    {
        float xPosition = 0; // Note will work this out
        float yPosition = GetLanePosition(note.NoteType);
        float zPosition = transform.position.z;

        Vector3 spawnPosition = new Vector3(xPosition, yPosition, zPosition);
        GameObject noteGameObject = Instantiate(spawnPrefab, spawnPosition, transform.rotation, spawnParent.transform);
        noteGameObject.name = $"Note {note.PositionInSteps} {note.NoteType}";

        NoteController controller = noteGameObject.GetComponent<NoteController>();
        controller.positionInSteps = note.PositionInSteps;
        controller.noteTimeInSeconds = noteTimeInSeconds;
        controller.startAudioTimeInSeconds = startAudioTimeInSeconds;
        controller.noteType = note.NoteType;
    }

    private int GetLanePosition(int noteType)
    {
        switch (noteType)
        {
            // Kick drum
            case 0:
                return -3;

            // Snare drum
            case 1:
                return 1;

            // Hi-hat
            case 2:
                return 5;

            // Tom 1
            case 3:
                return 3;

            // Tom 2
            case 4:
                return 2;

            // Crash
            case 5:
                return 6;

            // Fake (marker)
            case -1:
                return -6;

            default:
                throw new Exception($"Unknown note type: {noteType}");
        }
    }
}
