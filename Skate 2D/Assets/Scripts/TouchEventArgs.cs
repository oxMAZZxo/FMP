using System;
using System.Collections;
using System.Collections.Generic;

public class TouchEventArgs : EventArgs
{
    public float touchTime {get; private set;}
    public SwipeDirection swipeDirection {get; private set;}
    public TouchEventArgs() : base()
    {
        touchTime = 0;
        swipeDirection = SwipeDirection.NONE;
    }

    public TouchEventArgs(float time,SwipeDirection direction = SwipeDirection.NONE) : base()
    {
        touchTime = time;
        swipeDirection = direction;
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
