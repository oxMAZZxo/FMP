using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardLandEventArgs : EventArgs
{
    public int comboCount {get;}
    public float jumpHeight {get;}
    public int score {get;}

    public SkateboardLandEventArgs(int newScore, int newComboCount, float newJumpHeight)
    {
        score = newScore;
        comboCount = newComboCount;
        jumpHeight = newJumpHeight;
    }
}
