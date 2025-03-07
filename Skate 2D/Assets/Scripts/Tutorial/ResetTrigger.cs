using System.Collections;
using Cinemachine;
using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    [SerializeField]private CinemachineVirtualCamera cinemachineVirtualCamera;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            cinemachineVirtualCamera.enabled = false;
            collision.gameObject.transform.position = new Vector2(0,collision.gameObject.transform.position.y);
            cinemachineVirtualCamera.enabled = true;
        }
    }

}
