using System;
using System.Reflection;
using Cinemachine;
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
    [SerializeField]private TextMeshProUGUI addedScoreDisplay;
    [SerializeField,Range(1f,5f)]private float startVelocity = 1f;
    [SerializeField,Range(1f,10f)]private float maxVelocity = 1f;
    [SerializeField,Range(0.1f,0.2f)]private float velocityIncrementPerScoreAdded = 0.1f;
    [SerializeField,Range(1,15)]private int maxNumberOfIncrements = 15;
    [SerializeField,Range(10,50),Tooltip("This value will determine the amount of velocity incrementations based on the score added")]private int scoreIncrementValue = 10;
    private float currentVelocity;
    [SerializeField]private SkateboardController skateboardController;
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    public GameSpeed currentGameSpeed {get; private set;}
    private int noOfTricks;
    private int noOfCombos;
    [SerializeField]private bool startGame = false;
    private ScreenOrientation screenOrientation;
    private bool screenOrientationChanged;

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
        screenOrientation = Screen.orientation;
        Application.targetFrameRate = 144;
        currentVelocity = startVelocity;
        skateboardController.SetMinVelocity(currentVelocity);
        currentGameSpeed = GameSpeed.Slow;
        if(SystemInfo.deviceType == DeviceType.Desktop) {return;}
        if(screenOrientation == ScreenOrientation.LandscapeLeft || screenOrientation == ScreenOrientation.LandscapeRight)
        {
            virtualCamera.m_Lens.OrthographicSize = 2.3f;
        }else
        {
            virtualCamera.m_Lens.OrthographicSize = 5f;
        }
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
    }

    void FixedUpdate()
    {
        if(screenOrientation != Screen.orientation)
        {
            screenOrientationChanged = true;
            screenOrientation = Screen.orientation;
        }

        if(screenOrientationChanged)
        {
            screenOrientationChanged = false;
            if(screenOrientation == ScreenOrientation.LandscapeLeft || screenOrientation == ScreenOrientation.LandscapeRight)
            {
                virtualCamera.m_Lens.OrthographicSize = 2.3f;
            }else
            {
                virtualCamera.m_Lens.OrthographicSize = 5f;
            }
        }
    }

    public void AddScore(int value)
    {
        if(value == 0) {return;}
        DisplayPointsIncrement(value);
        if(currentVelocity < maxVelocity)
        {
            IncreaseSpeed(value);
        }
        score +=value;
        scoreDisplay.text = ScoreString(score);
    }

    private void IncreaseSpeed(int value)
    {
        int noOfIncrements = value / scoreIncrementValue;
        if(noOfIncrements > maxNumberOfIncrements) {noOfIncrements = maxNumberOfIncrements;}
        float speedToIncrement = velocityIncrementPerScoreAdded * noOfIncrements;
        currentVelocity += speedToIncrement;
        if(currentVelocity > maxVelocity) {currentVelocity = maxVelocity;}
        skateboardController.SetMinVelocity(currentVelocity);    
        CheckGameSpeed();    
    }

    
    public void ClearConsole()
    {
        Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
        MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }

    private void CheckGameSpeed()
    {
        if(currentVelocity > 3 && currentVelocity < 4)
        { 
            currentGameSpeed = GameSpeed.Medium;
            scoreIncrementValue = 50;
            velocityIncrementPerScoreAdded = 0.1f;
        }
        if(currentVelocity >= 4 && currentVelocity < 4.5)
        { 
            currentGameSpeed = GameSpeed.Fast;
            scoreIncrementValue = 100;
            velocityIncrementPerScoreAdded = 0.05f;
        }
        if(currentVelocity > 4.5)
        {
            currentGameSpeed = GameSpeed.Fast;
            scoreIncrementValue = 200;
            velocityIncrementPerScoreAdded = 0.015f;
        }
    }

    private void DisplayPointsIncrement(int value)
    {
        int x = UnityEngine.Random.Range(0 + 100, Screen.width - 100);
        int y = UnityEngine.Random.Range(0 + 400,Screen.height - 100);
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
    Fast,
    SuperFast
}
