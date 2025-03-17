using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGrindCheckPause : MonoBehaviour
{
    [SerializeField]private GameObject SwipeNowDisplay;
    [SerializeField]private TutorialSwipeVisualiser swipeVisualiser;
    private TutorialSkateboard skateboard;
    private int counter;
    private bool canTrigger;

    void OnTriggerStay2D(Collider2D collision)
    {
        if(!canTrigger) {return;}
        if(counter < 3 && !skateboard.isGrounded && collision.CompareTag("Grindable"))
        {
            canTrigger = false;
            counter++;
            Debug.Log("Pausing Skateboard");
            StartCoroutine(PauseSkateboard());
        }
    }

    void Start()
    {
        skateboard = GetComponent<TutorialSkateboard>();
        canTrigger = true;
    }

    IEnumerator PauseSkateboard()
    {
        yield return new WaitForSeconds(0.3f);
        skateboard.Pause(false);
        SwipeNowDisplay.SetActive(true);
    }

    private void OnSkateboardLanded(object sender, EventArgs e)
    {
        if(counter < 3)
        {
            canTrigger = true;
        }else
        {
            TutorialSkateboard.onLanded -= OnSkateboardLanded;
            Destroy(swipeVisualiser.gameObject);
        }
        SwipeNowDisplay.SetActive(false);
    }

    void OnEnable()
    {
        TutorialSkateboard.onLanded += OnSkateboardLanded;
    }

    void OnDisable()
    {
        TutorialSkateboard.onLanded -= OnSkateboardLanded;
    }
}
