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
