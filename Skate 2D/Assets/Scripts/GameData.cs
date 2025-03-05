using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
