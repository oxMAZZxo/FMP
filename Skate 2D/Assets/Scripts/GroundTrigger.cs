using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        //If player comes in contact with the a ground trigger
        if(other.CompareTag("Player"))
        {
            //GameManager will create a new ground
            ProceduralMap.Instance.CreateGround();
        }
    }
}
