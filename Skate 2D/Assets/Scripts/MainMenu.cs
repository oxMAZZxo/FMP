using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartGame()
    {
        SceneManager.LoadScene("ProceduralMap");
    }

    private void OnTouch(object sender, EventArgs e){
        StartGame();
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
