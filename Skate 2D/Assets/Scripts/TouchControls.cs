using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TouchControls : MonoBehaviour
{
    public Camera cam;
    public static TouchControls Instance;
    [SerializeField,Range(1f,1000f)]private float deadzone = 10f;
    [SerializeField,Range(0.01f,1f)]private float minDiagonalThreshold = 0.4f;
    [SerializeField]private InputActionReference touchInput;
    public static event EventHandler<TouchEventArgs> tapEvent;
    public static event EventHandler<TouchEventArgs> swipeUpEvent;
    public static event EventHandler<TouchEventArgs> swipeDownEvent;
    public static event EventHandler<TouchEventArgs> swipeRightEvent;
    public static event EventHandler<TouchEventArgs> swipeLeftEvent;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    private float worldRadius;
    private float touchTime;


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

    void Start()
    {
        touchTime = 0;
    }

    void Update()
    {
        if(touchStart != Vector2.zero)
        {
            touchTime += Time.deltaTime;
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
                swipeRightEvent.Invoke(this, new TouchEventArgs(touchTime));
                break;

                case SwipeDirection.LEFT:
                swipeLeftEvent.Invoke(this, new TouchEventArgs(touchTime));
                break;

                case SwipeDirection.DOWN:
                swipeDownEvent.Invoke(this, new TouchEventArgs(touchTime));
                break;

                case SwipeDirection.UP:
                swipeUpEvent.Invoke(this, new TouchEventArgs(touchTime));
                break;
            }
        }else
        {
            tapEvent.Invoke(this, new TouchEventArgs(touchTime));
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

    private void OnTouchInputBegan(InputAction.CallbackContext context)
    {
        touchStart = Touchscreen.current.primaryTouch.position.ReadValue();
    }

    private void OnTouchInputEnded(InputAction.CallbackContext context)
    {
        touchEnd = Touchscreen.current.primaryTouch.position.ReadValue();
        DetectSwipe();
        touchStart = Vector2.zero;
        touchTime = 0;
    }

    void OnEnable()
    {
        touchInput.action.Enable();
        touchInput.action.started += OnTouchInputBegan;
        touchInput.action.canceled += OnTouchInputEnded;
    }

    void OnDisable()
    {
        touchInput.action.Disable();
        touchInput.action.started -= OnTouchInputBegan;
        touchInput.action.canceled -= OnTouchInputEnded;
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
