using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0.01f,1f)]public float volume; 
    [Range(0.5f,2f)]public float pitch;
    [Range(0f,1f),Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D")]public float spatialBlend;
    [HideInInspector]public AudioSource source;
    public bool loop;
}