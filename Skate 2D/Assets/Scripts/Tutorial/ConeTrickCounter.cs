using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeTrickCounter : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<TutorialSkateboard>().IncreaseTrickCounter();
        }
    }
}
