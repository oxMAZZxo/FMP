using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public int score {get; private set;}
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI framerateDisplay;
    private float timeCounter;
    [Range(0.1f,1f)]public float refreshTime = 0.5f;
    private float frameCounter;
    private float lastFramerate;

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
        Application.targetFrameRate = 144;
    }

    void Update()
    {
        if( timeCounter < refreshTime )
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            lastFramerate = (float)frameCounter/timeCounter;
            framerateDisplay.text = "Average framerate: " + lastFramerate.ToString("F0");
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }

    public void AddScore(int value)
    {
        score +=value;
        scoreDisplay.text = score.ToString();
    }
}
