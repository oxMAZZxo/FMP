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
    public TextMeshProUGUI framerateDisplay;
    [Header("UI Elements")]
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI distanceTravelledDisplay;
    [SerializeField]private GameObject gameOverPanel;
    [SerializeField]private TextMeshProUGUI scoreDisplayFinal;
    [SerializeField]private TextMeshProUGUI noOfTricksDisplay;
    [SerializeField]private TextMeshProUGUI noOfCombosDisplay;
    [SerializeField]private TextMeshProUGUI longestComboDisplay;
    [SerializeField]private TextMeshProUGUI distanceTravelledDisplayFinal;
    [SerializeField]private TextMeshProUGUI addedScoreDisplay;
    [Header("Start Game Velocity Fields")]
    [SerializeField,Range(1f,5f)]private float startVelocity = 1f;
    [SerializeField,Range(1f,10f)]private float maxVelocity = 1f;
    [SerializeField,Range(0.1f,0.2f)]private float velocityIncrementPerScoreAdded = 0.1f;
    [SerializeField,Range(1,15)]private int maxNumberOfIncrements = 15;
    [SerializeField,Range(10,50),Tooltip("This value will determine the amount of velocity incrementations based on the score added")]private int scoreIncrementValue = 10;
    [Header("Automatic Speed Incrementation Fields")]
    [SerializeField,Range(0.01f,0.5f)]private float velocityToAdd = 0.25f;
    [SerializeField,Range(0f,60f)]private float incrementationInterval = 30f;
    private float counter;
    private float currentVelocity;
    [Header("In Game")]
    [SerializeField]private SkateboardController skateboardController;
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    public GameSpeed currentGameSpeed {get; private set;}
    private int noOfTricks;
    private int noOfCombos;
    private ScreenOrientation screenOrientation;
    private bool screenOrientationChanged;

    #region Framerate Variables
    private float timeCounter;
    private float refreshTime = 0.5f;
    private float frameCounter;
    private float lastFramerate;
    #endregion

    private float skateboardStartX;
    private float skateboardOldX;
    public bool gameHasStarted {get; private set;}
    public static event EventHandler<EventArgs> reset;

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
        screenOrientation = Screen.orientation;
        Application.targetFrameRate = 144;
        
        if(SystemInfo.deviceType == DeviceType.Desktop) {return;}
        if(screenOrientation == ScreenOrientation.LandscapeLeft || screenOrientation == ScreenOrientation.LandscapeRight)
        {
            virtualCamera.m_Lens.OrthographicSize = 2.3f;
        }else
        {
            virtualCamera.m_Lens.OrthographicSize = 5f;
        }
        gameHasStarted = false;
    }

    public void StartGame()
    {
        if(gameHasStarted) {return;}
        currentVelocity = startVelocity;
        skateboardController.SetMinVelocity(currentVelocity);
        gameHasStarted = true;
        currentGameSpeed = GameSpeed.Slow;
        skateboardStartX = skateboardController.gameObject.transform.position.x;
        skateboardOldX = skateboardStartX;
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
        if(!gameHasStarted) {return;}
        if(currentVelocity >= maxVelocity) {return;}
        if(counter >= incrementationInterval)
        {
            Debug.Log($"Increasing Velocity by {velocityToAdd}");
            currentVelocity += velocityToAdd;
            skateboardController.SetMinVelocity(currentVelocity);
            CheckGameSpeed();
            counter = 0;
        }

        counter += Time.deltaTime;
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

        if(!gameHasStarted) {return;}

        if(skateboardOldX < skateboardController.gameObject.transform.position.x)
        {
            float distanceTravelled = skateboardController.gameObject.transform.position.x - skateboardStartX;
            distanceTravelledDisplay.text = distanceTravelled.ToString("F0");
        }

        skateboardOldX = skateboardController.gameObject.transform.position.x;
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
        scoreDisplay.text = PrettyNumberString(score);
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
        if(currentVelocity >= 4 && currentVelocity < 5)
        { 
            currentGameSpeed = GameSpeed.Fast;
            scoreIncrementValue = 100;
            velocityIncrementPerScoreAdded = 0.075f;
        }
        if(currentVelocity > 5)
        {
            currentGameSpeed = GameSpeed.SuperFast;
            scoreIncrementValue = 120;
            velocityIncrementPerScoreAdded = 0.05f;
        }
    }

    private void DisplayPointsIncrement(int value)
    {
        int x = UnityEngine.Random.Range(0 + 200, Screen.width - 200);
        int y = UnityEngine.Random.Range(0 + 400,Screen.height - 100);
        addedScoreDisplay.text = "+" + PrettyNumberString(value);
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
        distanceTravelledDisplayFinal.text += distanceTravelled.ToString("F1");
        gameOverPanel.SetActive(true);
    }

    private string PrettyNumberString(float value) 
    {
        if(value < 1000)
        {
            return value.ToString();
        }else if(value < 1000000)
        {
            return GetDecimalPoint((float)value / 1000,1) + "K";
        }else if(value < 1000000000)
        {
            return GetDecimalPoint((float)value / 1000000,1) + "M";
        }

        return "***";
    }
    
    private string GetDecimalPoint(float value, int decimalPoint)
    {
        string temp = ""; temp += value.ToString()[0] + ".";
        for(int i = 2; i < value.ToString().Length; i ++)
        {
            if((i - 2) >= decimalPoint) {break;}
            temp += value.ToString()[i];
        }
        return temp;
    }

    public void Reset()
    {
        Vector3 newPos = new Vector3(0,0.735f,0);
        skateboardController.transform.position = newPos;
        reset?.Invoke(this, EventArgs.Empty);
        Invoke("DisableGameOverPanel",0.3f);
    }

    private void DisableGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }
}

public enum GameSpeed{
    Slow,
    Medium,
    Fast,
    SuperFast
}
