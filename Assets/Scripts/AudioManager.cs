using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField]
    private AudioClip blockMoveClip;
    [SerializeField]
    private AudioClip blockMergeClip;

    private AudioSource source;


    private void Awake() => Instance = this;

    private void Start() => source = GetComponent<AudioSource>();

    public void PlayMoveClip(float volume = 0.5f)
    {
        source.PlayOneShot(blockMoveClip, volume);
    }

    public void PlayMergeClip(float volume = 0.5f)
    {
        source.PlayOneShot(blockMergeClip, volume);
    }
}