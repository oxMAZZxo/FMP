using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance {get; private set;}
    public bool partA{get; private set;}
    public bool partB{get; private set;}
    public bool partC{get; private set;}
    public bool partD{get; private set;}
    public bool shouldRoll {get; private set;}
    public bool isPaused {get; private set;}
    [SerializeField]private GameObject tutorialPanel;
    [SerializeField]private GameObject tutorialFinishedPanel;
    [SerializeField]private GameObject inOutPanel;
    [SerializeField]private GameObject partAPanel;
    [SerializeField]private GameObject partBPanel;
    [SerializeField]private GameObject partCPanel;
    [SerializeField]private GameObject partDPanel;
    [SerializeField]private GameObject trickCounterDisplay;
    [SerializeField]private GameObject wellDoneDisplay;
    [SerializeField]private GameObject jumpBar;
    [SerializeField]private GameObject pauseButton;
    [SerializeField]private Transform skateboard;
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    [SerializeField]private GameObject[] grindables;
    [SerializeField]private GameObject[] unavoidables;
    private TutorialSkateboard tutorialSkateboard;

    private AsyncOperation asyncLoad;
    private bool sceneLoadDone;

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
        tutorialSkateboard = skateboard.GetComponent<TutorialSkateboard>();
        partA = true;
        partB = false;
        partC = false;
        partD = false;
        partAPanel.SetActive(true);
        partBPanel.SetActive(false);
        partCPanel.SetActive(false);
        partDPanel.SetActive(false);
        StartCoroutine(LoadGameScene());
    }

    /// <summary>
    /// If the tutorial has already been completed, allow the player to pause the tutorial.
    /// </summary>
    public void CheckToEnablePauseButton()
    {
        if(!GameData.Instance.tutorialCompleted){return;}
        pauseButton.SetActive(true);
    }

    public void StartPartB()
    {
        partA = false;
        partB = true;
        StartCoroutine(SwitchPanel(partAPanel,partBPanel));
    }

    public void StartPartC()
    {
        partB = false;
        partC = true;        
        TextMeshProUGUI trickCounter = trickCounterDisplay.GetComponent<TextMeshProUGUI>();
        trickCounter.text = "0/3";
        StartCoroutine(SwitchPanel(partBPanel,partCPanel));
    }

    public void StartPartD()
    {
        partC = false;
        partD = true;
        TextMeshProUGUI trickCounter = trickCounterDisplay.GetComponent<TextMeshProUGUI>();
        trickCounter.text = "0/5";
        StartCoroutine(SwitchPanel(partCPanel,partDPanel));
        StartCoroutine(ActivePartDObjects());
    }

    public void TutorialFinished()
    {
        GameData.Instance.SetTutorialCompleted(true);
        StartCoroutine(SwitchPanel(tutorialPanel,tutorialFinishedPanel));    
    }

    private IEnumerator ActivePartDObjects()
    {
        yield return new WaitForSeconds(1.5f);
        jumpBar.SetActive(true);
        SwitchObstacles();
    }

    private IEnumerator SwitchPanel(GameObject from, GameObject to)
    {
        shouldRoll = false;
        from.SetActive(false);
        trickCounterDisplay.SetActive(false);
        wellDoneDisplay.SetActive(true);
        yield return new WaitForSeconds(1f);
        inOutPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        pauseButton.SetActive(false);
        virtualCamera.enabled = false;
        wellDoneDisplay.SetActive(false);
        skateboard.position = new Vector2(0,skateboard.position.y + 0.2f);
        tutorialSkateboard.DisableInput();
        to.SetActive(true);
        virtualCamera.enabled = true;
    }

    public void PlayerRoll(bool value)
    {
        shouldRoll = value;
    }

    private void SwitchObstacles()
    {
        foreach(GameObject current in grindables)
        {
            current.SetActive(false);
        }
        foreach(GameObject current in unavoidables)
        {
            current.SetActive(true);
        }
    }

    public void Pause()
    {
        isPaused = true;
        tutorialSkateboard.Pause();
    }

    public void Resume()
    {
        isPaused = false;
        tutorialSkateboard.Resume();
    }

    private IEnumerator LoadGameScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync("Game");
        asyncLoad.allowSceneActivation = false;
        sceneLoadDone = false;
        //wait until the asynchronous scene fully loads
        while (!sceneLoadDone)
        {
            //scene has loaded as much as possible,
            // the last 10% can't be multi-threaded
            if (asyncLoad.progress >= 0.9f)
            {
                sceneLoadDone = true;
            }
            yield return null;
        }
    }

    public void StartGameScene()
    {
        asyncLoad.allowSceneActivation = true;
    }

    public void PlayClickSoundEffect()
    {
        AudioManager.Global.Play("ButtonClick");
    }
}
