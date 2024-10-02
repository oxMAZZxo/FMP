using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchControls : MonoBehaviour
{
    [SerializeField,Range(1f,1000f)]private float deadzone = 10f;
    [SerializeField,Range(0.01f,1f)]private float minTravelMagnitude = 0.4f;
    public static event EventHandler swipeRightEvent;
    public static event EventHandler tapEvent;
    public static event EventHandler swipeDownEvent;
    public static event EventHandler swipeLeftEvent;
    private Vector2 touchStart;
    private Vector2 touchEnd;

    void Start()
    {

    }

    void Update()
    {
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            touchStart = Input.GetTouch(0).position;
        }

        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            touchEnd = Input.GetTouch(0).position;
            DetectSwipe();
            
        }
    }

    private void DetectSwipe()
    {
        Vector2 swipeDelta = touchEnd - touchStart;
        if(swipeDelta.magnitude > deadzone)
        {
            SwipeDirection swipeDirection = CalculateSwipe(swipeDelta);
            switch (swipeDirection)
            {
                case SwipeDirection.RIGHT: 
                swipeRightEvent.Invoke(this, new EventArgs());
                break;

                case SwipeDirection.LEFT:
                swipeLeftEvent.Invoke(this, new EventArgs());
                break;

                case SwipeDirection.DOWN:
                swipeDownEvent.Invoke(this, new EventArgs());
                break;
            }
        }else
        {
            tapEvent.Invoke(this, new EventArgs());
        }
    }


    private SwipeDirection CalculateSwipe(Vector2 delta)
    {

        delta.Normalize();
        if (delta.x < -minTravelMagnitude && delta.y < -minTravelMagnitude)
        {
            return SwipeDirection.DOWN_LEFT;
        }

        if (delta.x < -minTravelMagnitude && delta.y > minTravelMagnitude)
        {
            return SwipeDirection.UP_LEFT;
        }

        if (delta.x > minTravelMagnitude && delta.y < -minTravelMagnitude)
        {
            return SwipeDirection.DOWN_RIGHT;
        }

        if (delta.x > minTravelMagnitude && delta.y > minTravelMagnitude)
        {
            return SwipeDirection.UP_RIGHT;
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
        }
        else
        {
            return delta.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
        }
    }
}

public enum SwipeDirection
{
    TAP,
    UP,
    DOWN,
    LEFT,
    RIGHT,
    UP_LEFT,
    UP_RIGHT,
    DOWN_LEFT,
    DOWN_RIGHT
}
