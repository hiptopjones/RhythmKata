using Assets.Data;
using MidiJack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private ulong frameIndex = 0;
    private Dictionary<MidiNote, ulong> noteOnToFrameIndex = new Dictionary<MidiNote, ulong>();
    private Dictionary<MidiNote, ulong> noteOffToFrameIndex = new Dictionary<MidiNote, ulong>();

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

    public bool IsInputCodeActive(MidiNote midiNote)
    {
        return IsMidiNoteActive(midiNote);
    }

    private bool IsMidiNoteActive(MidiNote midiNote)
    {
        ulong noteOnFrameIndex;
        if (!noteOnToFrameIndex.TryGetValue(midiNote, out noteOnFrameIndex))
        {
            return false;
        }

        ulong noteOffFrameIndex;
        if (!noteOffToFrameIndex.TryGetValue(midiNote, out noteOffFrameIndex))
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

    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        MidiNote midiNote = (MidiNote)note;

        Debug.Log($"{frameIndex} - NoteOn: {channel}, {midiNote} ({note}), {velocity}");
        noteOnToFrameIndex[midiNote] = frameIndex;
    }

    private void NoteOff(MidiChannel channel, int note)
    {
        MidiNote midiNote = (MidiNote)note;

        Debug.Log($"{frameIndex} - NoteOff: {channel}, {midiNote} ({note})");
        noteOffToFrameIndex[midiNote] = frameIndex;
    }
}
