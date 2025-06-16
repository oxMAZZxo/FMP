using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class handles logic for transitioning between menu, gameplay, and tutorial.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]private GameObject mainMenuPanel;
    [SerializeField]private GameObject gameplayPanel;
    [SerializeField]private GameObject settingsPanel;
    [SerializeField]private GameObject tutorialPanel;
    [SerializeField]private TextMeshProUGUI highScoreDisplay;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private AsyncOperation asyncLoad;
    private bool sceneLoadDone;
    public bool canGameStart = true;

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
        StartCoroutine(LoadTutorialScene());
    }


    /// <summary>
    /// Function executed through an event when the user touches the screen.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTouch(object sender, TouchEventArgs e)
    {
        if(!canGameStart) {return;}
        if(GameManager.Instance.gameHasStarted || settingsPanel.activeInHierarchy) {return;}
        GameObject objectHit;
        bool UI = CheckForUI(e.startPosition,out objectHit);

        if(!UI)
        {
            if(!GameData.Instance.tutorialCompleted)
            {
                tutorialPanel.SetActive(true);
                return;
            }
            GameManager.Instance.StartGame();
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mousePos"></param>
    /// <returns>Returns true if an interactable UI element has been clicked</returns>
    private bool CheckForUI(Vector2 mousePos, out GameObject uiObject)
    {
        uiObject = null;
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = mousePos;

        List<RaycastResult> results = new List<RaycastResult>();

        raycaster.Raycast(pointerEventData, results);
        
        if(results.Count < 1) {return false;}
        uiObject = results[0].gameObject;
        return true;
	}

    /// <summary>
    /// Resets the Menu to the Main Menu Panel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResetMenu(object sender, EventArgs e)
    {
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        OnMenuShown(sender, e);
    }

    private void OnMenuShown(object sender, EventArgs e)
    {
        highScoreDisplay.text = Utilities.PrettyNumberString(GameData.Instance.highScore);
    }

    /// <summary>
    /// Load the Tutorial Scene Asyncronously, for it to be ready in case the user wants to start it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadTutorialScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync("TutorialScene");
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

    public void StartTutorialMap()
    {
        asyncLoad.allowSceneActivation = true;
    }

    public void PlayClickSoundEffect()
    {
        AudioManager.Global.Play("ButtonClick");
    }

    public void OpenFeedbackForm()
    {
        Application.OpenURL("https://forms.gle/ucchkwAhkFpXmxGz9");
    }

    void OnEnable()
    {
        TouchControls.touchEvent += OnTouch; 
        GameManager.reset += ResetMenu;
        GameData.dataLoaded += OnMenuShown;
    }

    void OnDisable()
    {
        TouchControls.touchEvent -= OnTouch; 
        GameManager.reset -= ResetMenu;
        GameData.dataLoaded -= OnMenuShown;
    }
}
