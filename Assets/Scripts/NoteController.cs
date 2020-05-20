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
    private double destroyDelayInSeconds = 2;

    public double noteTimeInSeconds;
    public double startAudioTimeInSeconds;

    private ParticleSystem particleSystem;
    private InputController inputController;

    public int noteType;
    public int positionInSteps;

    public bool InTargetZone { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        inputController = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputController>();

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
        InTargetZone = true;
        CheckNoteHit();
    }

    public void OnTargetZoneExit()
    {
        InTargetZone = false;
        Destroy(gameObject, (float)destroyDelayInSeconds);
    }

    private void CheckNoteHit()
    {
        if (inputController.IsInputCodeActive(noteType))
        {
            OnNoteHit();
        }
    }

    public void OnNoteHit()
    {
        // Hide the note
        GetComponent<MeshRenderer>().enabled = false;

        // Play the explosion
        particleSystem.Play();
    }
}
