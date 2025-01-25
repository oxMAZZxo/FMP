using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public int score {get; private set;}
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI framerateDisplay;
    [SerializeField]private GameObject gameOverPanel;
    [SerializeField]private TextMeshProUGUI scoreDisplayFinal;
    [SerializeField]private TextMeshProUGUI noOfTricksDisplay;
    [SerializeField]private TextMeshProUGUI noOfCombosDisplay;
    [SerializeField]private TextMeshProUGUI longestComboDisplay;
    [SerializeField]private TextMeshProUGUI distanceTravelledDisplay;

    private int noOfTricks;
    private int noOfCombos;

    #region Framerate Variables
    private float timeCounter;
    [Range(0.1f,1f)]public float refreshTime = 0.5f;
    private float frameCounter;
    private float lastFramerate;
    #endregion

    void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Application.targetFrameRate = 144;
    }

    void Update()
    {
        #region FrameRateCounter
        if( timeCounter < refreshTime )
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            lastFramerate = (float)frameCounter/timeCounter;
            framerateDisplay.text = "Average framerate: " + lastFramerate.ToString("F0");
            frameCounter = 0;
            timeCounter = 0.0f;
        }
        #endregion
    
    }

    public void AddScore(int value)
    {
        score +=value;
        scoreDisplay.text = ScoreString(score);
    }

    public void IncrementNumberOfTricks()
    {
        noOfTricks ++;
    }

    public void IncrementNumberOfCombos()
    {
        noOfCombos++;
    }

    public void SessionEnded(int longestCombo, float distanceTravelled)
    {
        Debug.Log($"Session ended with a score of {score} and {noOfTricks} performed tricks");
        scoreDisplayFinal.text += score.ToString();
        noOfTricksDisplay.text += noOfTricks.ToString();
        noOfCombosDisplay.text += noOfCombos.ToString();
        longestComboDisplay.text += longestCombo.ToString();
        distanceTravelledDisplay.text += distanceTravelled.ToString("F1");
        gameOverPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private string ScoreString(int value) 
    {
        if(value < 1000)
        {
            return value.ToString();
        }else if(value < 1000000)
        {
            return ((float)value / 1000).ToString("F1") + "K";
        }else if(value < 1000000000)
        {
            return ((float)value / 1000000).ToString("F1") + "M";
        }

        return "***";
    }
}
