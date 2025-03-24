using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// A class which handles logic to adjust volume based on user input.
/// </summary>
public class Settings : MonoBehaviour
{
    [SerializeField]private AudioMixer mainMixer;
    [SerializeField]private Slider masterSlider;
    [SerializeField]private Slider musicSlider;
    [SerializeField]private Slider sfxSlider;

    void Start()
    {
        string data = SaveSystem.LoadData(Application.persistentDataPath + ("/SettingsData.txt"));
        if(!string.IsNullOrEmpty(data) && !string.IsNullOrWhiteSpace(data))
        {
            DeconstructData(data);
        }
    }

    private void DeconstructData(string data)
    {
        string[] temp = data.Split(',');

        masterSlider.value = float.Parse(temp[0]);
        musicSlider.value = float.Parse(temp[1]);
        sfxSlider.value = float.Parse(temp[2]);
    }

    public void AdjustVolume(string parameter)
    {
        float volume = 0;
        switch (parameter)
        {
            case "MasterVolume":
                volume = masterSlider.value;
                break;
            case "MusicVolume":
                volume = musicSlider.value;
                break;
            case "SFXVolume":
                volume = sfxSlider.value;
                break;
        }
        if(volume < -10f) {volume = -80f;}
        mainMixer.SetFloat(parameter,volume);
        SaveSystem.SaveData($"{masterSlider.value},{musicSlider.value},{sfxSlider.value}",Application.persistentDataPath + ("/SettingsData.txt"));
    }
}
