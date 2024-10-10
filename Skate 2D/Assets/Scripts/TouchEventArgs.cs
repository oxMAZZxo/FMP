using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEventArgs : EventArgs
{
    public float touchTime {get; private set;}
    public TouchEventArgs() : base()
    {
        touchTime = 0;
    }

    public TouchEventArgs(float time) : base()
    {
        touchTime = time;
    }
}
