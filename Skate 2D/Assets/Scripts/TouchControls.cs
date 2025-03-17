using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The Touch Controls class is a singleton, scene persistent object, which detects user input on a touchscreen, calculates the diferent swipe directions and invokes touch input events.
/// </summary>
public class TouchControls : MonoBehaviour
{
    public int id {get;}
    public Camera cam;
    public static TouchControls Instance;
    [SerializeField,Range(1f,1000f)]private float deadzone = 10f;
    [SerializeField,Range(0.01f,1f)]private float minDiagonalThreshold = 0.4f;
    [SerializeField]private InputActionReference touchInput;
    /// <summary>
    /// The Touch Event will invoke when a touch input has been calculated. 
    /// It will return TouchEventArgs which hold information about how long the touch was, and its direction.
    /// </summary>
    public static event EventHandler<TouchEventArgs> touchEvent;
    /// <summary>
    /// The Touch Started event will invoke when any touch input has been detected and send the start touch position as Vector2.
    /// </summary>
    public static event EventHandler<Vector2> touchStarted;
    /// <summary>
    /// The Touch Ended event will invoke when an existing touch has stopped, it will send a Vector2 respresenting the final position.
    /// </summary>
    public static event EventHandler<Vector2> touchEnded;
    /// <summary>
    /// This event will invoke as a swipe is happening. Giving the user the new position of the current touch.
    /// </summary>
    // public static event EventHandler<Vector2> swipeInProgress;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    private float worldRadius;
    private float touchTime;

    void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        touchInput.action.Enable();
        touchTime = 0;
    }

    void Update()
    {
        if(touchStart != Vector2.zero)
        {
            touchTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Detects if there is a viable swipe, else it will invoke an a plain touch input.
    /// </summary>
    private void DetectSwipe()
    {
        
        Vector2 swipeDelta = touchEnd - touchStart;
        SwipeDirection swipeDirection = SwipeDirection.NONE;
        if(swipeDelta.magnitude > deadzone)
        {
            swipeDirection = CalculateSwipe(swipeDelta);
        }
        touchEvent?.Invoke(this, new TouchEventArgs(touchTime,touchStart,touchEnd,swipeDirection));
    }

    /// <summary>
    /// Calculates the direction of the swipe, if there is one.
    /// </summary>
    /// <param name="delta">The current swipe</param>
    /// <returns>Returns NONE if there is no swipe direction based on the variables tweaked</returns>
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

    /// <summary>
    /// Function called user touches the screen
    /// </summary>
    /// <param name="context">The input action context</param>
    private void OnTouchInputBegan(InputAction.CallbackContext context)
    {
        // Debug.Log("Touch Began");
        touchStart = Mouse.current.position.ReadValue();
        if(Touchscreen.current != null)
        {
            touchStart = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        touchStarted?.Invoke(this, touchStart);
    }

    /// <summary>
    /// Function invoked when user lifts their finger from the screen
    /// </summary>
    /// <param name="context"></param>
    private void OnTouchInputEnded(InputAction.CallbackContext context)
    {
        // Debug.Log("Touch Ended");
        touchEnd = Mouse.current.position.ReadValue();
        if(Touchscreen.current != null)
        {
            touchEnd = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        touchEnded?.Invoke(this, touchEnd);
        DetectSwipe();
        touchStart = Vector2.zero;
        touchTime = 0;
    }

    private void OnTouchInputInProgress(InputAction.CallbackContext context)
    {
        if(touchStart == Vector2.zero)
        {
            return;
        }
        Debug.Log(context.action.ReadValue<Vector2>());
    }

    void OnEnable()
    {
        // Debug.Log($"Enabling touch functionality on TouchControls {this.GetInstanceID()}");
        touchInput.action.started += OnTouchInputBegan;
        touchInput.action.canceled += OnTouchInputEnded;
    }

    void OnDisable()
    {
        // Debug.Log($"Disabling touch functionality on TouchControls {this.GetInstanceID()}");
        touchInput.action.started -= OnTouchInputBegan;
        touchInput.action.canceled -= OnTouchInputEnded;
    }

    /// <summary>
    /// Draws a debug gizmos for debugging purposes
    /// </summary>
    void OnDrawGizmos()
    {
        if(touchStart == Vector2.zero || cam == null) {return;}
        Vector3 start = cam.ScreenToWorldPoint(touchStart);
        start.z = 0;
        worldRadius = deadzone * (2 * cam.orthographicSize / Screen.height);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start,worldRadius);
    }
}
