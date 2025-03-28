using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// A class which handles logic to adjust volume based on user input.
/// </summary>
public class Settings : MonoBehaviour
{
    [SerializeField]private AudioMixer mainMixer;
    [SerializeField]private Toggle sfxToggle;
    [SerializeField]private Toggle musicToggle;
    private bool sfxOn;
    private bool musicOn;

    void Start()
    {
        string data = SaveSystem.LoadData(Application.persistentDataPath + ("/SettingsData.txt"));
        if(!string.IsNullOrEmpty(data) && !string.IsNullOrWhiteSpace(data))
        {
            DeconstructData(data);
            SetSettings();
        }
    }

    private void DeconstructData(string data)
    {
        string[] temp = data.Split(',');

        musicOn = Convert.ToBoolean(temp[0]);
        sfxOn = Convert.ToBoolean(temp[1]);
    }

    private void SetSettings()
    {
        sfxToggle.SetIsOnWithoutNotify(sfxOn);
        musicToggle.SetIsOnWithoutNotify(musicOn);
        SetSFX();
        SetMusic();
    }

    public void SetSFX() 
    {
        if(!sfxToggle.isOn)
        {
            mainMixer.SetFloat("SFXVolume",-80f);
        }else
        {
            mainMixer.SetFloat("SFXVolume",0f);
        }
        if(sfxOn != sfxToggle.isOn)
        {
            sfxOn = sfxToggle.isOn;
            SaveSystem.SaveData($"{musicOn},{sfxOn}",Application.persistentDataPath + ("/SettingsData.txt"));
            Debug.Log($"There was a change in sfx sound, saving......");
        }

    }

    public void SetMusic() 
    {
        if(!musicToggle.isOn)
        {
            mainMixer.SetFloat("MusicVolume",-80f);
        }else
        {
            mainMixer.SetFloat("MusicVolume",0f);
        }
        if(musicOn != musicToggle.isOn)
        {
            musicOn = musicToggle.isOn;
            SaveSystem.SaveData($"{musicOn},{sfxOn}",Application.persistentDataPath + ("/SettingsData.txt"));
            Debug.Log($"There was a change in music sound, saving....");
        }
    }
}
