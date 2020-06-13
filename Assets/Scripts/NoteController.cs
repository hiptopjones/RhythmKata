using Assets.Data;
using MidiJack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    [SerializeField]
    private float speed = 20;

    [SerializeField]
    private float destroyDelayInSeconds = 2;

    public double noteTimeInSeconds;
    public double startAudioTimeInSeconds;

    private ParticleSystem particleSystem;
    private InputController inputController;

    public MidiNote midiNote;
    public int positionInSteps;

    public bool InTargetZone { get; private set; }

    private double enterTargetZoneTimeInSeconds;
    private double exitTargetZoneTimeInSeconds;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        if (particleSystem == null)
        {
            throw new Exception("No ParticleSystem component found ");
        }

        inputController = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputController>();
        if (inputController == null)
        {
            throw new Exception("No InputController component found");
        }

        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (InTargetZone)
        {
            CheckNoteHit();
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        double currentAudioTimeInSeconds = AudioSettings.dspTime;
        double currentPlaybackTimeInSeconds = currentAudioTimeInSeconds - startAudioTimeInSeconds;
        double offsetTimeInSeconds = currentPlaybackTimeInSeconds - noteTimeInSeconds;

        transform.position = new Vector3((float)(-speed * offsetTimeInSeconds), transform.position.y, transform.position.z);
    }

    public void OnTargetZoneEnter()
    {
        enterTargetZoneTimeInSeconds = Time.time;
        InTargetZone = true;
        CheckNoteHit();
    }

    public void OnTargetZoneExit()
    {
        exitTargetZoneTimeInSeconds = Time.time;
        Debug.Log("Time in target zone = " + (exitTargetZoneTimeInSeconds - enterTargetZoneTimeInSeconds));

        InTargetZone = false;
        Invoke(nameof(DestroyNote), destroyDelayInSeconds);
    }

    private void DestroyNote()
    {
        GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    private void CheckNoteHit()
    {
        if (inputController.IsInputCodeActive(midiNote))
        {
            OnNoteHit();
        }
    }

    public void OnNoteHit()
    {
        particleSystem.Play();
        DestroyNote();
    }
}
