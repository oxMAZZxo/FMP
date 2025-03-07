using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeResetTrigger : MonoBehaviour
{
    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            collision.transform.position = new Vector3(0,collision.transform.position.y);
        }
    }
}
