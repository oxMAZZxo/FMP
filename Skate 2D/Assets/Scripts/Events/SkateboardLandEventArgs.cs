using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardLandEventArgs : EventArgs
{
    /// <summary>
    /// The number of tricks in this combo
    /// </summary>
    public int comboCount {get;}
    /// <summary>
    /// The highest point the skateboard reached when performing a trick.
    /// </summary>
    public float jumpHeight {get;}
    /// <summary>
    /// The score to be rewarded to the player.
    /// </summary>
    public int score {get;}
    /// <summary>
    /// All the trick points added together
    /// </summary>
    public int trickPoints {get;}

    public SkateboardLandEventArgs(int newTrickPoints, int newComboCount, float newJumpHeight)
    {
        comboCount = newComboCount;
        trickPoints = newTrickPoints; 
        score = trickPoints * comboCount;
        jumpHeight = newJumpHeight;
    }
}
