using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField]
    private AudioClip blockMoveClip;
    [SerializeField]
    private AudioClip blockMergeClip;
    [SerializeField]
    private AudioClip[] bgMusics;

    private AudioSource source;

    private int currentMusicIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            currentMusicIndex = Random.Range(0, bgMusics.Length);
            PlayBackgroundMusic();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlayBackgroundMusic()
    {
        source.clip = bgMusics[currentMusicIndex];
        source.Play();
        Invoke(nameof(PlayBackgroundMusic), bgMusics[currentMusicIndex].length);
        currentMusicIndex = (currentMusicIndex + 1) % bgMusics.Length;
    }

    public void PlayMoveClip(float volume = 0.5f)
    {
        source.PlayOneShot(blockMoveClip, volume);
    }

    public void PlayMergeClip(float volume = 0.5f)
    {
        source.PlayOneShot(blockMergeClip, volume);
    }
}