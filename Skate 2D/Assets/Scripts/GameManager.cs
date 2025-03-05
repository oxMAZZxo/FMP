using System;
using System.Collections;
using System.Reflection;
using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public int score {get; private set;}
    public bool isGamePaused {get; private set;}
    public TextMeshProUGUI framerateDisplay;
    [Header("UI Elements")]
    [SerializeField]private TextMeshProUGUI scoreDisplay;
    [SerializeField]private TextMeshProUGUI distanceTravelledDisplay;
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
    [Header("Combo Rush")]
    [SerializeField]private GameSpeed comboRushGameSpeed;
    [SerializeField,Range(0,100)]private float comboRushActivateChance;
    [SerializeField,Range(10,60)]private float comboRushActivationCooldown;
    [SerializeField,Range(5f,30f),Tooltip("How long can the combo rush last")]private float comboRushDuration;
    [SerializeField]private GameObject comboRushDisplay;
    private bool canActivateComboRush;
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
    public static event EventHandler gamespeedChanged;

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
            virtualCamera.m_Lens.OrthographicSize = 1.2f;
        }else
        {
            virtualCamera.m_Lens.OrthographicSize = 5f;
        }
        gameHasStarted = false;
        canActivateComboRush = true;
    }

    public void StartGame()
    {
        if(gameHasStarted) {return;}
        StartCameraAnimations();

        currentVelocity = startVelocity;
        skateboardController.SetMinVelocity(currentVelocity);
        gameHasStarted = true;
        currentGameSpeed = GameSpeed.Slow;
        skateboardStartX = skateboardController.gameObject.transform.position.x;
        skateboardOldX = skateboardStartX;
    }

    private void StartCameraAnimations()
    {
        StartCoroutine(VM_ZoomOutAnim());
        StartCoroutine(VM_TrackedObjectOffsetAnim());
    }

    private IEnumerator VM_ZoomOutAnim()
    {
        while(virtualCamera.m_Lens.OrthographicSize < 2.3f)
        {
            virtualCamera.m_Lens.OrthographicSize += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator VM_TrackedObjectOffsetAnim()
    {
        CinemachineFramingTransposer framingTransposer = virtualCamera.GetComponentInChildren<CinemachineFramingTransposer>();
        Vector3 newOffset = framingTransposer.m_TrackedObjectOffset;
        while(framingTransposer.m_TrackedObjectOffset.y < 0.22f)
        {
            newOffset.y += Time.deltaTime;
            framingTransposer.m_TrackedObjectOffset = newOffset;
            yield return new WaitForEndOfFrame();
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
        if(!gameHasStarted || isGamePaused) {return;}
        if(currentVelocity >= maxVelocity) {return;}
        if(counter >= incrementationInterval)
        {
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

        if(!gameHasStarted || isGamePaused) {return;}

        if(skateboardOldX < skateboardController.gameObject.transform.position.x)
        {
            float distanceTravelled = skateboardController.gameObject.transform.position.x - skateboardStartX;
            distanceTravelledDisplay.text = distanceTravelled.ToString("F0");
        }

        skateboardOldX = skateboardController.gameObject.transform.position.x;
    }

    /// <summary>
    /// Adds the provided points to the player's overall score, and may increase the game speed depending on the number of points provided
    /// </summary>
    /// <param name="value"></param>
    public void AddScore(int value)
    {
        if(value == 0) {return;}
        DisplayPointsIncrement(value);
        if(currentVelocity < maxVelocity)
        {
            IncreaseSpeed(value);
        }
        score +=value;
        scoreDisplay.text = Utilities.PrettyNumberString(score);
    }

    /// <summary>
    /// Increases game speed based on the given points
    /// </summary>
    /// <param name="value"></param>
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

    /// <summary>
    /// Updates currentGameSpeed based on current Velocity
    /// </summary>
    private void CheckGameSpeed()
    {
        GameSpeed oldGameSpeed = currentGameSpeed;
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
        if(oldGameSpeed != currentGameSpeed)
        {
            Debug.Log($"Game speed changed {currentGameSpeed.ToString()}");
            gamespeedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void CalculateComboRush()
    {
        if(canActivateComboRush && currentGameSpeed >= comboRushGameSpeed && UnityEngine.Random.Range(0,100) <= comboRushActivateChance)
        {
            canActivateComboRush = false;
            comboRushDisplay.SetActive(true);
            ProceduralMap.Instance.StartComboRush(comboRushDuration);
            StartCoroutine(ComboRushCooldown());
        }
    }

    private IEnumerator ComboRushCooldown()
    {
        Debug.Log("Combo rush cooldown started");
        yield return new WaitForSeconds(comboRushActivationCooldown);
        Debug.Log("Combo rush cooldown ended");
        canActivateComboRush = true;
    }

    /// <summary>
    /// Displays the current points added to the players score
    /// </summary>
    /// <param name="value"></param>
    private void DisplayPointsIncrement(int value)
    {
        int x = UnityEngine.Random.Range(0 + 200, Screen.width - 200);
        int y = UnityEngine.Random.Range(0 + 400,Screen.height - 100);
        addedScoreDisplay.text = "+" + Utilities.PrettyNumberString(value);
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
        StopCoroutine(ComboRushCooldown());
        canActivateComboRush = true;
        scoreDisplayFinal.text += score.ToString();
        noOfTricksDisplay.text += noOfTricks.ToString();
        noOfCombosDisplay.text += noOfCombos.ToString();
        longestComboDisplay.text += longestCombo.ToString();
        distanceTravelledDisplayFinal.text += distanceTravelled.ToString("F1");
        gameOverPanel.SetActive(true);
        gameHasStarted = false;
        GameData.Instance.SetStats(score,longestCombo,distanceTravelled);
    }

    /// <summary>
    /// Resets the game to default positions and values
    /// </summary>
    public void Reset()
    {
        StopCoroutine(ComboRushCooldown());
        canActivateComboRush = true;
        gameHasStarted = false;
        isGamePaused = false;
        distanceTravelledDisplay.text = "0";
        scoreDisplay.text = "0";
        score = 0;
        noOfTricks = 0;
        noOfCombos = 0;
        currentVelocity = startVelocity;
        currentGameSpeed = GameSpeed.Slow;
        scoreIncrementValue = 30;
        velocityIncrementPerScoreAdded = 0.125f;
        Vector3 newPos = new Vector3(0,0.735f,0);
        CinemachineFramingTransposer framingTransposer = virtualCamera.GetComponentInChildren<CinemachineFramingTransposer>();
        framingTransposer.m_TrackedObjectOffset.y = 0;
        virtualCamera.m_Lens.OrthographicSize = 1.2f;
        skateboardController.transform.position = newPos;
        reset?.Invoke(this, EventArgs.Empty);
        virtualCamera.enabled = false;
        Invoke("DisableGameOverPanel",0.3f);
        skateboardController.Reset();
    }

    private void DisableGameOverPanel()
    {
        virtualCamera.enabled = true;
        gameOverPanel.SetActive(false);
        distanceTravelledDisplayFinal.text = "Distance: ";
        scoreDisplayFinal.text = "Score: ";
        noOfTricksDisplay.text = "Tricks: ";
        noOfCombosDisplay.text = "Combos: ";
        longestComboDisplay.text = "Longest: ";
    }

    public void PauseGame()
    {
        isGamePaused = true;
        skateboardController.Pause();
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        skateboardController.Resume();
    }
}

public enum GameSpeed{
    Slow,
    Medium,
    Fast,
    SuperFast
}
