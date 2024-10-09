using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.XR;

public class TouchControls : MonoBehaviour
{

    public Camera cam;
    public static TouchControls Instance;
    [SerializeField,Range(1f,1000f)]private float deadzone = 10f;
    [SerializeField,Range(0.01f,1f)]private float minDiagonalThreshold = 0.4f;
    public static event EventHandler tapEvent;
    public static event EventHandler swipeRightEvent;
    public static event EventHandler swipeDownEvent;
    public static event EventHandler swipeLeftEvent;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    private float worldRadius;

    void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
        }

        if(Input.GetMouseButtonUp(0))
        {
            touchEnd = Input.mousePosition;
            DetectSwipe();
            touchStart = Vector2.zero;
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
            Debug.Log(swipeDirection.ToString());
        }else
        {
            tapEvent.Invoke(this, new EventArgs());
        }
    }


    private SwipeDirection CalculateSwipe(Vector2 delta)
    {
        delta.Normalize();
        if (delta.x < -minDiagonalThreshold && delta.y < -minDiagonalThreshold)
        {
            return SwipeDirection.DOWN_LEFT;
        }

        if (delta.x < -minDiagonalThreshold && delta.y > minDiagonalThreshold)
        {
            return SwipeDirection.UP_LEFT;
        }

        if (delta.x > minDiagonalThreshold && delta.y < -minDiagonalThreshold)
        {
            return SwipeDirection.DOWN_RIGHT;
        }

        if (delta.x > minDiagonalThreshold && delta.y > minDiagonalThreshold)
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

    void OnDrawGizmos()
    {
        if(touchStart == Vector2.zero) {return;}
        Vector3 start = cam.ScreenToWorldPoint(touchStart);
        start.z = 0;
        worldRadius = deadzone * (2 * cam.orthographicSize / Screen.height);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start,worldRadius);
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
