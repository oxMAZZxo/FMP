using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The Game Manager is a singleton that holds information and performs logic based on the current events that take place in the game.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The Game Manager Instance.
    /// </summary>
    public static GameManager Instance {get; private set;}
    /// <summary>
    /// The players current score
    /// </summary>
    public int score {get; private set;}
    /// <summary>
    /// Is the current game session running paused.
    /// </summary>
    public bool isGamePaused {get; private set;}
    [Header("UI Elements")]
    [SerializeField]private TextMeshProUGUI scoreDisplay;
    [SerializeField]private TextMeshProUGUI distanceTravelledDisplay;
    [SerializeField]private GameOverDisplay gameOverDisplay;
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
    [SerializeField]private Transform skateboardFollowTarget;
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    [SerializeField,Range(0.1f,2f)]private float cameraShakeTime;
    [SerializeField]private TextMeshProUGUI comboDisplay;
    [SerializeField]private TextMeshProUGUI comboCounterDisplay;
    [SerializeField]private TextMeshProUGUI addedScoreDisplay;
    [SerializeField]private TextMeshProUGUI comboAnnouncementDisplay;
    private Animator comboCounterDisplayAnimator;
    // [SerializeField]private TextMeshProUGUI multiplierDisplay;
    // [SerializeField]private TextMeshProUGUI potentialPointsDisplay;
    private CinemachineShake vmShake;
    [Header("Combo Rush")]
    [SerializeField]private GameSpeed comboRushGameSpeed;
    [SerializeField,Range(0,100)]private float comboRushActivateChance;
    [SerializeField,Range(10,60)]private float comboRushActivationCooldown;
    [SerializeField,Range(5f,30f),Tooltip("How long can the combo rush last")]private float comboRushDuration;
    [SerializeField]private GameObject comboRushDisplay;
    private bool canActivateComboRush;
    /// <summary>
    /// The speed that current session is at.
    /// </summary>
    public GameSpeed currentGameSpeed {get; private set;}
    private int noOfTricks;
    private int noOfCombos;
    private float skateboardStartX;
    private float skateboardOldX;
    /// <summary>
    /// Has the a game session started
    /// </summary>
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
        vmShake = virtualCamera.gameObject.GetComponent<CinemachineShake>();
        Application.targetFrameRate = 144;
        gameHasStarted = false;
        canActivateComboRush = true;
        comboCounterDisplayAnimator = comboCounterDisplay.gameObject.GetComponent<Animator>();
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
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
    }

    /// <summary>
    /// Slowly zooms out the camera.
    /// </summary>
    /// <returns></returns>
    private IEnumerator VM_ZoomOutAnim()
    {
        while(virtualCamera.m_Lens.FieldOfView < 30f)
        {
            virtualCamera.m_Lens.FieldOfView += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    void Update()
    {
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
    private void AddScore(int value)
    {
        if(value == 0) {return;}
        StartCoroutine(IncrementScoreAnimation(value));
        DisplayPointsIncrement(value);
        if(currentVelocity < maxVelocity)
        {
            IncreaseSpeed(value);
        }

    }

    private IEnumerator IncrementScoreAnimation(int value)
    {
        Debug.Log($"Value: {value}");
        int finalValue = score + value;
        Debug.Log($"Final Value: {finalValue}");
        int addition = 1;
        if(value > 100) {addition = 10;}
        if(value > 1000) {addition = 50;}
        if(value > 10000) {addition = 100;}
        for(int i = score; i <= finalValue; i+= addition)
        {
            yield return new WaitForSeconds(0.0001f);
            score = i;
            if(score > finalValue)
            {
                score = finalValue;
                scoreDisplay.text = score.ToString();
                break;
            }
            scoreDisplay.text = score.ToString();
        }
        // scoreDisplay.text = Utilities.PrettyNumberString(score);
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

    /// <summary>
    /// Calculates whether a combo rush should be activated.
    /// </summary>
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
        yield return new WaitForSeconds(comboRushActivationCooldown);
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

    /// <summary>
    /// Function that gets executed through an event when the skateboard controller performs a trick.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSkateboardTrickPerformed(object sender, SkateboardTrickPerformedEventArgs e)
    {
        if(e.isCombo)
        {
            comboCounterDisplayAnimator.SetTrigger("comboAdded");
            noOfCombos++;
        }
        noOfTricks ++;
        comboCounterDisplay.text = e.comboCount.ToString();
        // potentialPointsDisplay.text = e.potentialPoints.ToString();
        // comboDisplay.text += e.trickName;
        // comboDisplay.gameObject.SetActive(true);
        comboCounterDisplay.gameObject.SetActive(true);
        if(e.isCombo && e.comboCount > 1)
        {
            AudioManager.Global.Play("ComboSFX", 0.025f);
        }
    }

    public void DecrementNumberOfTricks()
    {
        noOfTricks --;
    }

    /// <summary>
    /// Function executed through an event when skateboard controller lands on the ground.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSkateboardLanded(object sender, SkateboardLandEventArgs e)
    {
        AddScore(e.score);
        // comboDisplay.gameObject.SetActive(false);
        comboCounterDisplay.gameObject.SetActive(false);
        comboCounterDisplay.text = "";
        // potentialPointsDisplay.text = "";
        // comboDisplay.text = "";
        AudioManager.Global.ResetPitch("ComboSFX");
        if(e.jumpHeight > 1)
        {
            vmShake.ShakeCamera(cameraShakeTime, e.jumpHeight);    
        }else if(e.comboCount > 2)
        {
            float cameraShakeMultiplier = ((e.comboCount -3) / 10f) + 1;
            vmShake.ShakeCamera(cameraShakeTime, cameraShakeMultiplier);
        }

        if(e.comboCount > 1)
        {
            ShowComboAnnouncement(e.comboCount);
        }
    }

    private void ShowComboAnnouncement(int comboCount)
    {
        int index = comboCount - 2;
        if(comboCount > (GameData.Instance.comboAnnouncements.Length - 1)) {index = GameData.Instance.comboAnnouncements.Length -1;}
        ComboAnnouncement current = GameData.Instance.comboAnnouncements[index];
        comboAnnouncementDisplay.text = current.taglines[UnityEngine.Random.Range(0,current.taglines.Length)];
        comboAnnouncementDisplay.gameObject.SetActive(true);
    }

    /// <summary>
    /// Ends the session.
    /// </summary>
    /// <param name="longestCombo">This sessions longest combo</param>
    /// <param name="distanceTravelled">This sessions distance travelled</param>
    public void SessionEnded(int longestCombo, float distanceTravelled)
    {
        StopCoroutine(ComboRushCooldown());
        canActivateComboRush = true;
        gameOverDisplay.SetValues(score,noOfTricks,noOfCombos,longestCombo,distanceTravelled);
        gameOverDisplay.gameObject.SetActive(true);
        gameHasStarted = false;
        GameData.Instance.SetStats(score,longestCombo,distanceTravelled);
    }

    /// <summary>
    /// Resets the game to default positions and values
    /// </summary>
    public void Reset()
    {
        StopAllCoroutines();
        
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
        virtualCamera.m_Lens.FieldOfView = 15f;
        skateboardController.transform.position = newPos;
        reset?.Invoke(this, EventArgs.Empty);
        virtualCamera.enabled = false;
        // comboDisplay.gameObject.SetActive(false);
        comboCounterDisplay.gameObject.SetActive(false);
        // comboDisplay.text = "";
        comboCounterDisplay.text = "";
        // potentialPointsDisplay.text = "";
        Invoke("DisableGameOverPanel",0.3f);
        skateboardController.Reset();
    }

    private void DisableGameOverPanel()
    {
        virtualCamera.enabled = true;
        gameOverDisplay.gameObject.SetActive(false);
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

    void OnEnable()
    {
        SkateboardController.trickPerformed += OnSkateboardTrickPerformed;
        SkateboardController.skateboardLanded += OnSkateboardLanded;
    }

    void OnDisable()
    {
        SkateboardController.trickPerformed -= OnSkateboardTrickPerformed;
        SkateboardController.skateboardLanded -= OnSkateboardLanded;
    }
}

public enum GameSpeed{
    Slow,
    Medium,
    Fast,
    SuperFast
}
