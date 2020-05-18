﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float speed;
    public bool inTargetZone;
    public float destroyDelayInSeconds = 2f;

    public float noteTimeInSeconds;
    public float startAudioTimeInSeconds;

    // Debug data
    public int noteType;
    public int positionInSteps;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        double currentAudioTimeInSeconds = AudioSettings.dspTime;
        double currentPlaybackTimeInSeconds = currentAudioTimeInSeconds - startAudioTimeInSeconds;
        double offsetTimeInSeconds = currentPlaybackTimeInSeconds - noteTimeInSeconds;

        transform.position = new Vector3((float)(-speed * offsetTimeInSeconds), transform.position.y, transform.position.z);
    }

    public void OnTriggerEnter(Collider other)
    {
        inTargetZone = true;
    }

    public void OnTriggerExit(Collider other)
    {
        inTargetZone = false;
        Destroy(gameObject, destroyDelayInSeconds);
    }
}