using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]private AudioMixer mainMixer;
    [SerializeField]private Slider masterSlider;
    [SerializeField]private Slider musicSlider;
    [SerializeField]private Slider sfxSlider;

    void Start()
    {
        SaveSystem.LoadData(Application.persistentDataPath + ("/SettingsData.txt"));
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
    }
}
