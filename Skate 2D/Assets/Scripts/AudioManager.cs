using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Global {get; private set;}
    [SerializeField]private bool isGlobal = false;
    [SerializeField]private Sound[] sounds;
    [SerializeField]private AudioMixerGroup soundEffectMixer;
    [SerializeField]private AudioMixerGroup musicMixer;
    private Dictionary<string, int> soundIndexes;
    
    void Awake()
    {
        CreateSounds();
        if(!isGlobal) {return;}
        if(Global != this && Global != null)
        {
            Destroy(gameObject);
        }else
        {
            Global = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void CreateSounds()
    {
        soundIndexes = new Dictionary<string, int>();
        for(int i = 0; i < sounds.Length; i++)
        {
            sounds[i].source = gameObject.AddComponent<AudioSource>();
            sounds[i].source.clip = sounds[i].clip;
            sounds[i].source.volume = sounds[i].volume;
            sounds[i].source.pitch = sounds[i].pitch;
            sounds[i].source.playOnAwake = false;
            sounds[i].source.loop = sounds[i].loop;
            sounds[i].source.spatialBlend = sounds[i].spatialBlend;
            sounds[i].source.playOnAwake = sounds[i].playOnAwake;
            sounds[i].source.outputAudioMixerGroup = soundEffectMixer;
            if(sounds[i].preLoadAudioData)
            {
                sounds[i].clip.LoadAudioData();
            }
            if(sounds[i].isMusic)
            {
                sounds[i].source.outputAudioMixerGroup = musicMixer;
            }
            soundIndexes.Add(sounds[i].name,i);
        }
    }

    /// <summary>
    /// Plays the sound with the provided name, if it exists and not already playing
    /// </summary>
    /// <param name="name"></param>
    public void Play(string name)
    {
        Sound sound = FindSound(name);
        if(sound == null) {return;}
        if(sound.source.isPlaying && !Global)
        {
            Debug.LogWarning("Sound is already playing");
            return;
        }
        sound.source.Play();
    }

    public void Play(string name, float alterPitch)
    {
        Sound sound = FindSound(name);
        if(sound == null) {return;}
        if(sound.source.isPlaying && !Global)
        {
            Debug.LogWarning("Sound is already playing");
            return;
        }
        sound.source.pitch += alterPitch;
        sound.source.Play();
    }

    public void ResetPitch(string name)
    {
        Sound sound = FindSound(name);
        if(sound == null) {return;}
        sound.source.pitch = sound.pitch;
    }

    private Sound FindSound(string name)
    {
        if(!soundIndexes.ContainsKey(name)) {return null;}
        int soundIndex = soundIndexes[name];
        return sounds[soundIndex];
    }

    /// <summary>
    /// Stops the sound with given name if its playing
    /// </summary>
    /// <param name="name">The name of the sound to stop playing</param>
    public void Stop(string name)
    {
        Sound sound = FindSound(name);
        if(sound == null) {return;}
        sound.source.Stop();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if any sound is playing</returns>
    public bool IsSoundPlaying()
    {
        foreach(Sound sound in sounds)
        {
            if(sound.source.isPlaying) {return true;}
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">The name of the sound to check</param>
    /// <returns>True if the sound with the given name is playing</returns>
    public bool IsSoundPlaying(string name)
    {
        Sound sound = FindSound(name);
        if(sound == null) {return false;}
        if(sound.source.isPlaying && !Global)
        {
            return true;
        }
        return false;
    }
}