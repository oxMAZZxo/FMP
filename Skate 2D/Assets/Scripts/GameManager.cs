using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
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
    [SerializeField]private TextMeshProUGUI addedScoreDisplay;
    [SerializeField,Range(1f,5f)]private float startVelocity = 1f;
    [SerializeField,Range(0.01f,1f)]private float velocityIncrementPerInterval = 0.01f;
    [SerializeField,Range(1f,60)]private float velocityIncrementInterval = 60f;
    private float currentVelocity;
    [SerializeField]private SkateboardController skateboardController;
    public GameSpeed currentGameSpeed {get; private set;}
    private int noOfTricks;
    private int noOfCombos;
    private float counter;
    public bool startGame = false;

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
        if(!startGame) 
        {
            skateboardController.SetMinVelocity(0);
            return;
        }
        Debug.Log($"Screen width and height: {Screen.width} x {Screen.height}");
        Application.targetFrameRate = 144;
        currentVelocity = startVelocity;
        skateboardController.SetMinVelocity(currentVelocity);
        currentGameSpeed = GameSpeed.Slow;
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
            framerateDisplay.text = lastFramerate.ToString("F0");
            frameCounter = 0;
            timeCounter = 0.0f;
        }
        #endregion
        
        if(counter >= velocityIncrementInterval)
        {
            counter = 0;
            currentVelocity += velocityIncrementPerInterval;
            skateboardController.SetMinVelocity(currentVelocity);
        }
        if(currentVelocity > 3 && currentVelocity < 4){ currentGameSpeed = GameSpeed.Medium;}
        if(currentVelocity >= 4){ currentGameSpeed = GameSpeed.Fast;}
        counter += Time.deltaTime;
    }

    public void AddScore(int value)
    {
        if(value == 0) {return;}
        DisplayPointsIncrement(value);
        score +=value;
        scoreDisplay.text = ScoreString(score);
    }

    private void DisplayPointsIncrement(int value)
    {
        int x = Random.Range(0 + 100, Screen.width - 100);
        int y = Random.Range(0 + 400,Screen.height - 100);
        addedScoreDisplay.text = "+" + ScoreString(value);
        addedScoreDisplay.gameObject.transform.position = new Vector3(x,y);
        addedScoreDisplay.gameObject.SetActive(true);
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

public enum GameSpeed{
    Slow,
    Medium,
    Fast
}
