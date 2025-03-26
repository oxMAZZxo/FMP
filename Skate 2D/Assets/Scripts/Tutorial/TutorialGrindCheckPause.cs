using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialGrindCheckPause : MonoBehaviour
{
    /// <summary>
    /// A an element on the canvas telling the player to swipe now.
    /// </summary>
    [SerializeField]private GameObject SwipeNowDisplay;
    /// <summary>
    /// the swipe visualiser being used.
    /// </summary>
    [SerializeField]private TutorialSwipeVisualiser swipeVisualiser;
    /// <summary>
    /// The tutorial skateboard instance
    /// </summary>
    private TutorialSkateboard skateboard;
    private int counter;
    private bool canTrigger;

    void OnTriggerStay2D(Collider2D collision)
    {
        if(!canTrigger) {return;}//if we cannot trigger, it means we paused the skateboard already.
        //If the counter is less than 3 (we have paused the game less than 3 times), and the other collider has the 'Grindable' tag
        if(counter < 3 && !skateboard.isGrounded && collision.CompareTag("Grindable"))
        {
            //Ensure that the skateboard won't be paused more than once for every collider
            canTrigger = false;
            //Increase the counter to indicate we paused one more time.
            counter++;
            Debug.Log("Pausing Skateboard");
            //Puase the skateboard
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
        if(skateboard.isGrounded) {yield break;}
        //Pause the skateboard, but keep input enabled
        skateboard.Pause(false);
        //Show Swipe Now Display
        SwipeNowDisplay.SetActive(true);
        //Show visualiser
        swipeVisualiser.ShowVisualiser();
    }

    //When the skateboard lands, we want to re-enable the trigger as long as the skateboard has been paused less than 3 times.
    //Else, destroy the visualiser since the rest of the tutorial doesn't need it.
    private void OnSkateboardLanded(object sender, EventArgs e)
    {
        if(counter < 3)
        {
            canTrigger = true;
        }else
        {
            TutorialSkateboard.landed -= OnSkateboardLanded;
            Destroy(swipeVisualiser.gameObject);
        }
    }

    private void OnValidSwipeMade(object sender, EventArgs e)
    {
        //Disable Swipe Now Display, once a valid swipe is made.
        SwipeNowDisplay.SetActive(false);
    }

    void OnEnable()
    {
        TutorialSkateboard.landed += OnSkateboardLanded;
        TutorialSwipeVisualiser.validSwipe += OnValidSwipeMade;
    }

    void OnDisable()
    {
        TutorialSkateboard.landed -= OnSkateboardLanded;
        TutorialSwipeVisualiser.validSwipe -= OnValidSwipeMade;
    }
}
