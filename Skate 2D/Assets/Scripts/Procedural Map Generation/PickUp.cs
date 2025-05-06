using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField,Range(1,5)]private int minMultiplier = 1;
    [SerializeField,Range(5,10)]private int maxMultiplier = 5;
    public int currentMultiplier {get; private set;}
    [SerializeField]private bool isTrickRequired;
    [SerializeField]private string trickRequired;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {

        }
    }

    public void AssignMultiplier()
    {
        currentMultiplier = Random.Range(minMultiplier, maxMultiplier + 1);
    }
}
