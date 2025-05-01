using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The GameData class holds the important game data that is saved from the players session.
/// This is only a temporary class as things are saved locally. However, once moved to GPGS, this will be switched with a GPGS Manager.
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData Instance {get; private set;}
    public float highScore {get; private set;}
    public float longestCombo {get; private set;}
    public float longestDistance {get; private set;}
    public bool tutorialCompleted {get; private set;}
    public ComboAnnouncement[] comboAnnouncements {get; private set;}
    public static event EventHandler dataLoaded;

    void Awake()
    {
        if(Instance != this && Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        string data = SaveSystem.LoadData(Application.persistentDataPath + ("/GameData.txt"));
        if(!string.IsNullOrEmpty(data) && !string.IsNullOrWhiteSpace(data))
        {
            DeconstructData(data);
            dataLoaded?.Invoke(this, EventArgs.Empty);
        }
        LoadComboAnnouncements();
    }

    private void LoadComboAnnouncements()
    {
        TextAsset comboTaglinesFile = Resources.Load<TextAsset>("Combo Taglines");
        if (comboTaglinesFile != null)
        {
            string[] lines = comboTaglinesFile.text.Split('\n');
            comboAnnouncements = new ComboAnnouncement[lines.Length];
            for(int i = 0; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');
                comboAnnouncements[i] = new ComboAnnouncement(Convert.ToInt16(data[0]), data[1], data[2], data[3], data[4], data[5], data[6]);
            }
            Debug.Log($"Combo Announcements Loaded Successfully!");
            // foreach(ComboAnnouncement current in comboAnnouncements)
            // {
            //     Debug.Log($"Combo Count: {current.comboCount}. Taglines: {current.taglines[0]}, {current.taglines[1]}, {current.taglines[2]}, {current.taglines[3]}, {current.taglines[4]}, {current.taglines[5]}");
            // }
        }
        else
        {
            Debug.LogError("Could not load combo taglines from resources.");
        }
    }

    /// <summary>
    /// Takes the given loaded data and decontructs it do store in the variables that are appropriate.
    /// </summary>
    /// <param name="data"></param>
    private void DeconstructData(string data)
    {
        string[] temp = data.Split(',');

        highScore = float.Parse(temp[0]);
        longestCombo = float.Parse(temp[1]);
        longestDistance = float.Parse(temp[2]);
        try{
            tutorialCompleted = Convert.ToBoolean(temp[3]);
        }catch(IndexOutOfRangeException)
        {
            tutorialCompleted = false;
        }
    }

    /// <summary>
    /// Sets player stats and if there is a change, it will be saved.
    /// </summary>
    /// <param name="newHighScore"></param>
    /// <param name="newLongestCombo"></param>
    /// <param name="newLongestDistance"></param>
    public void SetStats(float newHighScore, float newLongestCombo, float newLongestDistance)
    {
        bool change = false;
        if(newHighScore > highScore) {highScore = newHighScore; change = true;}
        if(newLongestCombo > longestCombo) {longestCombo = newLongestCombo; change = true;}
        if(newLongestDistance > longestDistance) {longestDistance = newLongestDistance; change = true;}
        if(change)
        {
            string message = SaveSystem.SaveData($"{highScore},{longestCombo},{longestDistance},{tutorialCompleted}",Application.persistentDataPath + ("/GameData.txt"));
            Debug.Log(message);
        }
    }

    public void SetTutorialCompleted(bool newValue)
    {
        if(newValue == tutorialCompleted) {return;}
        tutorialCompleted = true;
        string message = SaveSystem.SaveData($"{highScore},{longestCombo},{longestDistance},{tutorialCompleted}",Application.persistentDataPath + ("/GameData.txt"));
        Debug.Log(message);
    }


}
