using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]private Camera cam;
    private AsyncOperation asyncLoad;
    private bool sceneReady;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        asyncLoad = SceneManager.LoadSceneAsync("ProceduralMap");
        asyncLoad.allowSceneActivation = false;
        sceneReady = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                sceneReady = true;
                break;
            }

            yield return null;
        }
        
    }

    private void StartGame()
    {
        asyncLoad.allowSceneActivation = true;
    }

    private void OnTouch(object sender, TouchEventArgs e)
    {
        bool UI = CheckForButton(e.startPosition);

        if(!UI && sceneReady)
        {
            StartGame();
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mousePos"></param>
    /// <returns>Returns true if an interactable UI element has been clicked</returns>
    private bool CheckForButton(Vector2 mousePos)
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = mousePos;

        List<RaycastResult> results = new List<RaycastResult>();

        raycaster.Raycast(pointerEventData, results);

        if(results.Count < 1) {return false;}

        return true;
	}

    void OnEnable()
    {
        TouchControls.touchEvent += OnTouch; 
    }

    void OnDisable()
    {
        TouchControls.touchEvent -= OnTouch; 
    }
}
