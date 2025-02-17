using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]private Camera cam;
    [Header("UI Elements")]
    [SerializeField]private GameObject mainMenuPanel;
    [SerializeField]private GameObject gameplayPanel;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    private void OnTouch(object sender, TouchEventArgs e)
    {
        if(GameManager.Instance.gameHasStarted) {return;}
        GameObject objectHit;
        bool UI = CheckForButton(e.startPosition,out objectHit);

        if(!UI)
        {
            GameManager.Instance.StartGame();
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
        }else
        {
            Debug.Log($"{objectHit.name}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mousePos"></param>
    /// <returns>Returns true if an interactable UI element has been clicked</returns>
    private bool CheckForButton(Vector2 mousePos, out GameObject uiObject)
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
    }

    void OnEnable()
    {
        TouchControls.touchEvent += OnTouch; 
        GameManager.reset += ResetMenu;
    }

    void OnDisable()
    {
        TouchControls.touchEvent -= OnTouch; 
        GameManager.reset -= ResetMenu;
    }
}
