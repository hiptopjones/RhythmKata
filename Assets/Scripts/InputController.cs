using MidiJack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private ulong frameIndex = 0;
    private Dictionary<int, ulong> noteOnToFrameIndex = new Dictionary<int, ulong>();
    private Dictionary<int, ulong> noteOffToFrameIndex = new Dictionary<int, ulong>();

    // Start is called before the first frame update
    void Start()
    {
        // Register for MIDI events
        MidiMaster.noteOnDelegate += NoteOn;
        MidiMaster.noteOffDelegate += NoteOff;
    }

    // Update is called once per frame
    void Update()
    {
        frameIndex++;
    }

    public bool IsInputCodeActive(int noteType)
    {
        bool useMidiInput = true;
        if (useMidiInput)
        {
            return IsMidiNoteActive(noteType);
        }
        else
        {
            return IsInputKeyDown(noteType);
        }
    }

    private bool IsMidiNoteActive(int noteType)
    {
        int midiNoteNumber;

        switch (noteType)
        {
            case 0: // Bass drum
                midiNoteNumber = 36;
                break;

            case 1: // Snare drum
                midiNoteNumber = 38;
                break;

            case 2: // Hi-hat
                midiNoteNumber = 42;
                break;

            case 3: // High Tom
                midiNoteNumber = 48;
                break;

            case 4: // Medium Tom
                midiNoteNumber = 45;
                break;

            case 5: // Crash cymbal
                midiNoteNumber = 49;
                break;

            default:
                throw new Exception($"Unrecognized note type: {noteType}");
        }

        ulong noteOnFrameIndex;
        if (!noteOnToFrameIndex.TryGetValue(midiNoteNumber, out noteOnFrameIndex))
        {
            return false;
        }

        ulong noteOffFrameIndex;
        if (!noteOffToFrameIndex.TryGetValue(midiNoteNumber, out noteOffFrameIndex))
        {
            return true;
        }

        if (noteOnFrameIndex > noteOffFrameIndex)
        {
            // Note off has not been sent yet
            return true;
        }

        if (noteOffFrameIndex > noteOnFrameIndex)
        {
            // Note off was sent at least a frame later than the on
            return false;
        }

        if (frameIndex > noteOffFrameIndex)
        {
            // Current frame index is at least a frame later than the note off
            return false;
        }

        // In all remaining cases, the note is considered on
        return true;
    }

    private bool IsInputKeyDown(int noteType)
    {
        KeyCode keyCode;

        switch (noteType)
        {
            case 0:
                keyCode = KeyCode.Space;
                break;

            case 1:
                keyCode = KeyCode.LeftControl;
                break;

            case 2:
                keyCode = KeyCode.RightControl;
                break;

            default:
                keyCode = KeyCode.Escape;
                break;
        }

        return Input.GetKeyDown(keyCode);
    }

    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log($"{frameIndex} - NoteOn: {channel}, {note}, {velocity}");
        noteOnToFrameIndex[note] = frameIndex;
    }

    private void NoteOff(MidiChannel channel, int note)
    {
        Debug.Log($"{frameIndex} - NoteOff: {channel}, {note}");
        noteOffToFrameIndex[note] = frameIndex;
    }
}
