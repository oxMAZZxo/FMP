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
    private float m_timeCounter;
    [Range(0.1f,1f)]public float m_refreshTime = 0.5f;
    private float m_frameCounter;
    private float m_lastFramerate;

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
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        if( m_timeCounter < m_refreshTime )
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter/m_timeCounter;
            framerateDisplay.text = "Average framerate: " + m_lastFramerate.ToString("F0");
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }

    public void AddScore(int value)
    {
        score +=value;
        scoreDisplay.text = score.ToString();
    }
}
