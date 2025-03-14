using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEventArgs : EventArgs
{
    /// <summary>
    /// How long has the user been touching the screen
    /// </summary>
    public float touchTime {get;}
    /// <summary>
    /// The direction of the current swipe. A value of SwipeDirection.NONE indicates that no swipe could be calculated
    /// </summary>
    public SwipeDirection swipeDirection {get;}
    /// <summary>
    /// The start position of the swipe.
    /// </summary>
    public Vector2 startPosition {get;}
    /// <summary>
    /// The end position of the swipe.
    /// </summary>
    public Vector2 endPosition {get;}
    
    /// <summary>
    /// Instantiate empty TouchEventArgs
    /// </summary>
    public TouchEventArgs() : base()
    {
        touchTime = 0;
        swipeDirection = SwipeDirection.NONE;
    }

    /// <summary>
    /// Instantiate a TouchEventArgs with direction hold time
    /// </summary>
    /// <param name="time">The amount of time the swipe/touch was held</param>
    /// <param name="direction">The direction of the swipe</param>
    public TouchEventArgs(float time, Vector2 start, Vector2 end, SwipeDirection direction = SwipeDirection.NONE) : base()
    {
        touchTime = time;
        swipeDirection = direction;
        startPosition = start;
        endPosition = end;
    }
}

public enum SwipeDirection
{
    NONE,
    UP,
    DOWN,
    LEFT,
    RIGHT,
    UP_LEFT,
    UP_RIGHT,
    DOWN_LEFT,
    DOWN_RIGHT
}
