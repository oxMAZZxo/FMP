using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialSwipeVisualiser : MonoBehaviour
{
    [SerializeField]private TutorialSkateboard skateboard;
    [SerializeField]private GameObject arrowVisualiser;
    private int[] trickDirections = {0,90,180,225,270};
    private int[] grindDirections = {90,180,270};
    private int index = 0;

    void Start()
    {
        arrowVisualiser.SetActive(true);
    }

    public void ShowArrow()
    {
        arrowVisualiser.SetActive(true);
    }

    private void OnSwipeInput(object sender, TouchEventArgs e)
    {
        bool swipeValid;
        if(TutorialManager.Instance.partB && !skateboard.isGrounded)
        {
            swipeValid = CheckGrind(e.swipeDirection);
        }else
        {
            swipeValid = CheckTrick(e.swipeDirection);
        }
        if(!swipeValid) {return;}
        index++;
        if(TutorialManager.Instance.partB && index > 2)
        {
            index = 0;
        }
        if(index > 5) {index = 0;}
        arrowVisualiser.SetActive(false);
    }

    private IEnumerator UpdateArrow()
    {
        yield return new WaitForSeconds(0.2f);
        arrowVisualiser.transform.rotation = TutorialManager.Instance.partB ? GetGrindRotations() : GetTrickRotations();  
    }

    private void OnSkateboardLanded(object sender, EventArgs e)
    {
        StartCoroutine(UpdateArrow());
        StartCoroutine(ShowOrHideArrowWithDelay());
    }

    private IEnumerator ShowOrHideArrowWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        if(TutorialManager.Instance.partA)
        {
            arrowVisualiser.SetActive(true);
        }
        if(TutorialManager.Instance.partB)
        {
            arrowVisualiser.SetActive(false);
        }
    }

    private bool CheckTrick(SwipeDirection swipeDirection)
    {
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
        Debug.Log($"Checking Grind... {Environment.NewLine} Current index: {index} Swipe Direction: {swipeDirection.ToString()}");
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


    private Quaternion GetGrindRotations()
    {
        if(index > grindDirections.Length - 1) {index = 0;}
        Debug.Log($"Getting next grind arrow rotation on index {index}");
        Quaternion rotation = Quaternion.Euler(0,0,grindDirections[index]);

        return rotation;
    }

    private Quaternion GetTrickRotations()
    {
        if(index > trickDirections.Length - 1) {index = 0;}
        Debug.Log($"Getting next trick arrow rotation on index {index}");
        Quaternion rotation = Quaternion.Euler(0,0,trickDirections[index]);

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
