using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        //If player comes in contact with the a ground trigger
        if(!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            //GameManager will create a new ground
            ProceduralMap.Instance.GenerateMap();
        }
    }

    public void Reset()
    {
        triggered = false;
    }

    private void GameReset(object sender, EventArgs e)
    {
        Reset();
    }

    void OnEnable()
    {
        GameManager.reset += GameReset;
    }

    void OnDisable()
    {
        GameManager.reset -= GameReset;
    }
}
