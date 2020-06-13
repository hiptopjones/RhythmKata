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
    private GameObject kickDrumPrefab;
    [SerializeField]
    private GameObject snareDrumPrefab;
    [SerializeField]
    private GameObject tomDrumPrefab;
    [SerializeField]
    private GameObject openHiHatPrefab;
    [SerializeField]
    private GameObject closedHiHatPrefab;
    [SerializeField]
    private GameObject rideCymbalPrefab;
    [SerializeField]
    private GameObject crashCymbalPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnParent == null)
        {
            throw new Exception("No spawn parent present");
        }
    }

    public void SpawnNote(Note note, double noteTimeInSeconds, double startAudioTimeInSeconds, Action onHitEvent)
    {
        float xPosition = 0; // Note will work this out
        float yPosition = GetLanePosition(note.MidiNote);
        float zPosition = transform.position.z;

        Vector3 spawnPosition = new Vector3(xPosition, yPosition, zPosition);
        GameObject spawnPrefab = GetPrefab(note.MidiNote);
        GameObject noteGameObject = Instantiate(spawnPrefab, spawnPosition, spawnPrefab.transform.rotation, spawnParent.transform);
        noteGameObject.name = $"Note {note.PositionInSteps} {note.MidiNote}";

        NoteController controller = noteGameObject.GetComponent<NoteController>();
        controller.positionInSteps = note.PositionInSteps;
        controller.noteTimeInSeconds = noteTimeInSeconds;
        controller.startAudioTimeInSeconds = startAudioTimeInSeconds;
        controller.midiNote = note.MidiNote;

        controller.OnHitEvent = onHitEvent;
    }

    private GameObject GetPrefab(MidiNote midiNote)
    {
        switch (midiNote)
        {
            case MidiNote.BassDrum:
                return kickDrumPrefab;

            case MidiNote.SnareDrum:
                return snareDrumPrefab;

            case MidiNote.ClosedHiHat:
                return closedHiHatPrefab;

            case MidiNote.OpenHiHat:
                return openHiHatPrefab;

            case MidiNote.HighTom:
            case MidiNote.MediumTom:
            case MidiNote.FloorTom:
                return tomDrumPrefab;

            case MidiNote.RideCymbal:
                return rideCymbalPrefab;

            case MidiNote.CrashCymbal:
                return crashCymbalPrefab;

            default:
                throw new Exception($"Unknown note type: {midiNote}");
        }
    }

    private int GetLanePosition(MidiNote midiNote)
    {
        switch (midiNote)
        {
            case MidiNote.BassDrum:
                return -3;

            case MidiNote.SnareDrum:
                return 1;

            case MidiNote.ClosedHiHat:
            case MidiNote.OpenHiHat:
                return 5;

            case MidiNote.HighTom:
                return 3;

            case MidiNote.MediumTom:
                return 2;

            case MidiNote.FloorTom:
                return -1;

            case MidiNote.RideCymbal:
                return 4;

            case MidiNote.CrashCymbal:
                return 6;

            default:
                throw new Exception($"Unknown note type: {midiNote}");
        }
    }
}
