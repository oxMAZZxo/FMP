using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Global;
    public bool isGlobal = false;
    public Sound[] sounds;
    private Dictionary<string, Sound> playableSounds;
    
    void Awake()
    {
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.playOnAwake = false;
            sound.source.loop = sound.loop;
            sound.source.spatialBlend = sound.spatialBlend;
        }
        // CreateDictionary();
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

    private void CreateDictionary()
    {
        playableSounds = new Dictionary<string, Sound>();

        foreach(Sound sound in sounds)
        {
            playableSounds.Add(sound.name,sound);
        }
    }

    /// <summary>
    /// Plays the sound with the provided name, if it exists and not already playing
    /// </summary>
    /// <param name="name"></param>
    public void Play(string name)
    {
        foreach(Sound currentSound in sounds)
        {
            if(currentSound == null || currentSound.name != name) { continue; }
            if(currentSound.source.isPlaying && !Global)
            {
                // Debug.LogWarning("Sound is already playing");
                return;
            }
            currentSound.source.Play();
            break;
        }
    }

    /// <summary>
    /// Stops the sound with given name if its playing
    /// </summary>
    /// <param name="name">The name of the sound to stop playing</param>
    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if(sound == null)
        {
            Debug.LogWarning("Sound with name '" + name + "' does not exist");
            return;
        }
        if(sound.source.isPlaying)
        {
            sound.source.Stop();
        }
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
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if(sound == null)
        {
            Debug.LogWarning("Sound with name '" + name + "' does not exist");
            return false;
        }
        if(sound.source.isPlaying) {return true;}
        return false;
    }
}