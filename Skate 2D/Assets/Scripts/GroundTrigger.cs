using System.Collections;
using System.Collections.Generic;
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
}
