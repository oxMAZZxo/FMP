using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialSwipeVisualiser : MonoBehaviour
{
    /// <summary>
    /// Reference of the tutorial skateboard
    /// </summary>
    [SerializeField]private TutorialSkateboard skateboard;
    /// <summary>
    /// Reference of the visualiser arrow
    /// </summary>
    [SerializeField]private GameObject arrow;
    /// <summary>
    /// The Z Rotations that the arrow needs to be in representing the available swipe directions for tricks.
    /// </summary>
    private int[] trickSwipeDirections = {0,90,180,225,270};
    /// <summary>
    /// The Z Rotations that the arrow needs to be in representing the available swipe directions for grinds.
    /// </summary>
    private int[] grindSwipeDirections = {90,180,270};
    /// <summary>
    /// The current index to use to take a Z rotation, on the trickDirections or GrindDirections;
    /// </summary>
    private int index = 0;
    public static event EventHandler validSwipe;

    void Start()
    {
        arrow.SetActive(true);
    }

    /// <summary>
    /// Activates the Visualiser on the canvas
    /// </summary>
    public void ShowVisualiser()
    {
        arrow.SetActive(true);
    }

    // When any input is made, we want to check whether that input was valid, for that current stage of the tutorial (part A or part B).
    private void OnSwipeInput(object sender, TouchEventArgs e)
    {
        if(e.swipeDirection == SwipeDirection.NONE) {return;}
        bool isValid;
        if(TutorialManager.Instance.partB && !skateboard.isGrounded) //If we are in part be and not on the ground (in the air when we tell the player to swipe)
        {
            // Check if the swipe made was valid
            isValid = CheckGrind(e.swipeDirection);
        }else // Else if not in Part B (meaning in Part A) 
        {
            // Check if the swipe made was valid
            isValid = CheckTrick(e.swipeDirection);
        }
        // If the swipe wasn't valid, stop
        if(!isValid) {return;}
        //increase index if valid
        index++;
        //if we are in part B, we only have 3 available directions
        if(TutorialManager.Instance.partB && index > 2)
        {
            //if the index is higher than 2, then reset to 0 to point at the first swipe direction for a grind
            index = 0;
        }
        //if index is bigger than 5, aka being in part A, then set it to 0. This code will never run in part B, because the grind directions are less than trick directions.
        if(index > 5) {index = 0;}
        //disable the arrow
        arrow.SetActive(false);
        validSwipe?.Invoke(this,EventArgs.Empty);
    }

    private IEnumerator UpdateArrow()
    {
        yield return new WaitForSeconds(0.2f);
        //Update the arrows Z rotation based on the whether this is part B is true. if true, get grind rotations, if false, get trick rotations
        arrow.transform.rotation = TutorialManager.Instance.partB ? GetGrindRotations() : GetTrickRotations();  
    }

    private void OnSkateboardLanded(object sender, EventArgs e)
    {
        //when skateboard lands, update the arrow rotation
        StartCoroutine(UpdateArrow());
        //and show or hide arrow depending on tutorial stage
        StartCoroutine(ShowOrHideArrowWithDelay());
    }

    private IEnumerator ShowOrHideArrowWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        //if part A, show arrow when player lands.
        if(TutorialManager.Instance.partA)
        {
            arrow.SetActive(true);
        }
        //if part B, disable it if not disabled.
        if(TutorialManager.Instance.partB)
        {
            arrow.SetActive(false);
        }
    }

    private bool CheckTrick(SwipeDirection swipeDirection)
    {
        //if the current index is 0, then it means that player needs to swipe up.
        //Therefore if both these conditions are valid, when this function is called,
        //And the current index is 0, and player swiped UP, it means the player made a valid swipe.
        //The Index represents the current direction displayed on the screen (that the player should perform)
        //The Swipe Direction is the current swipe direction the player performed.
        if(index == 0 && swipeDirection == SwipeDirection.UP)
        {
            return true;
        }

        if(index == 1 && swipeDirection == SwipeDirection.LEFT)
        {
            return true;
        }

        if(index == 2 && swipeDirection == SwipeDirection.DOWN)
        {
            return true;
        }

        if(index == 3 && swipeDirection == SwipeDirection.DOWN_RIGHT)
        {
            return true;
        }

        if(index == 4 && swipeDirection == SwipeDirection.RIGHT)
        {
            return true;
        }

        return false;
    }

    private bool CheckGrind(SwipeDirection swipeDirection)
    {
        //Same as Checking Tricks
        if(index == 0 && swipeDirection == SwipeDirection.LEFT)
        {
            return true;
        }

        if(index == 1 && swipeDirection == SwipeDirection.DOWN)
        {
            return true;
        }

        if(index == 2 && swipeDirection == SwipeDirection.RIGHT)
        {
            return true;
        }
        return false;
    }

    /// <returns>Returns a quaternion representing the rotation the arrow needs to be in depending on the current index for Grinds</returns>
    private Quaternion GetGrindRotations()
    {
        //Ensuring the index is not bigger than the amount of grind directions available
        if(index > grindSwipeDirections.Length - 1) {index = 0;}
        Debug.Log($"Getting next grind arrow rotation on index {index}");
        Quaternion rotation = Quaternion.Euler(0,0,grindSwipeDirections[index]);

        return rotation;
    }

    /// <returns>Returns a quaternion representing the rotation the arrow needs to be in depending on the current index for Tricks</returns>
    private Quaternion GetTrickRotations()
    {
        //same as as above
        if(index > trickSwipeDirections.Length - 1) {index = 0;}
        Debug.Log($"Getting next trick arrow rotation on index {index}");
        Quaternion rotation = Quaternion.Euler(0,0,trickSwipeDirections[index]);

        return rotation;
    }
    

    void OnEnable()
    {
        TouchControls.touchEvent += OnSwipeInput;
        TutorialSkateboard.landed += OnSkateboardLanded;
    }

    void OnDisable()
    {
        TouchControls.touchEvent -= OnSwipeInput;
        TutorialSkateboard.landed -= OnSkateboardLanded;
    }
}
