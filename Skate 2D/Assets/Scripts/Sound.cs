using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Represents a sound effect that can be played.
/// </summary>
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0.01f,1f)]public float volume; 
    [Range(0.1f,2f)]public float pitch;
    [Range(0f,1f),Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D")]public float spatialBlend;
    [HideInInspector]public AudioSource source;
    public bool loop;
    [Tooltip("If unchecked, it will be assumed this sound is a sound effect.")]public bool isMusic;
    public bool playOnAwake;
    [Tooltip("Enable this if you are going to be playing this sound in conjuction to other sounds and want to avoid lag spikes!")]public bool preLoadAudioData;
}