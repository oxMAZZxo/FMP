using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]private Camera cam;
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

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
        StartCoroutine(LoadTutorialScene());
    }

    private void OnTouch(object sender, TouchEventArgs e)
    {
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
