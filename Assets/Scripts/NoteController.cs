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
    public double previousAudioTimeInSeconds;

    private ParticleSystem particleSystem;
    private InputController inputController;

    public MidiNote midiNote;
    public int positionInSteps;

    public bool InTargetZone { get; private set; }
    private int NumFramesInTargetZone { get; set; }

    private double enterTargetZoneTimeInSeconds;
    private double exitTargetZoneTimeInSeconds;

    public Action OnHitEvent;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        if (particleSystem == null)
        {
            throw new Exception($"Unable to find child {nameof(ParticleSystem)} component");
        }

        inputController = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputController>();
        if (inputController == null)
        {
            throw new Exception($"Unable to find InputManager tagged {nameof(InputController)} component");

        }

        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (InTargetZone)
        {
            NumFramesInTargetZone++;
            CheckNoteHit();
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        // DSP time may remain the same over multiple calls to Update(), so this code tweens it
        double currentAudioTimeInSeconds = AudioSettings.dspTime;
        if (currentAudioTimeInSeconds == previousAudioTimeInSeconds)
        {
            currentAudioTimeInSeconds += Time.unscaledDeltaTime;
        }

        previousAudioTimeInSeconds = currentAudioTimeInSeconds;

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
        Debug.Log("Frames in target zone = " + NumFramesInTargetZone);

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
        OnHitEvent?.Invoke();

        particleSystem.Play();
        DestroyNote();
    }
}
