using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private AsyncOperation asyncLoad;
    private bool sceneReady;

    void Start()
    {
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

    private void OnTouch(object sender, EventArgs e){
        if(sceneReady) {StartGame();}
    }

    void OnEnable()
    {
        TouchControls.touchStarted += OnTouch;
    }

    void OnDisable()
    {
        TouchControls.touchStarted -= OnTouch;
    }
}
