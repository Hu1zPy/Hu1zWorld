using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public AudioSource[] AudioSources;
    private AudioSource _sfxAudioSource;
    private AudioSource _bgmAudioSource;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        _sfxAudioSource = AudioSources[0];
        _bgmAudioSource = AudioSources[1];
        PlayBGM("mainbgm");
    }

    [Serializable]
    public struct MusicData
    {
        public string ID;
        public AudioClip Clip;
    }

    public MusicData[] musics;

    
    
    public void PlayClip(string key)
    {
        foreach (var clip in musics)
        {
            if (clip.ID.Equals(key))
            {
                if (_sfxAudioSource)
                {
                    _sfxAudioSource.clip = clip.Clip;
                    _sfxAudioSource.Play();
                }
            }
        }
    }

    public void PlayBGM(string key)
    {
        StopBGM();
        foreach (var clip in musics)
        {
            if (clip.ID.Equals(key))
            {
                if (_bgmAudioSource)
                {
                    _bgmAudioSource.clip = clip.Clip;
                    Debug.Log("正在播放BGM--"+ key);
                    _bgmAudioSource.Play();
                }
            }
        }
    }

    public void StopBGM()
    {
        if (_bgmAudioSource)
        {
            Debug.Log("正在关闭BGM");
            _bgmAudioSource.clip = null;
            _bgmAudioSource.Stop();
        }
    }
}
