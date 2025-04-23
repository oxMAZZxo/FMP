using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardTrickPerformedEventArgs : EventArgs
{
    /// <summary>
    /// The trick that was performed.
    /// </summary>
    public string trickName {get;}
    /// <summary>
    /// Is this trick part of a combo
    /// </summary>
    public bool isCombo {get;}
    /// <summary>
    /// The number of tricks performed in this combo.
    /// </summary>
    public int comboCount {get;}
    public int potentialPoints {get;}

    public SkateboardTrickPerformedEventArgs(string newTrickName, int newComboCount,bool newCombo, int newPotentialPoints)
    {
        trickName = newTrickName;
        isCombo = newCombo;
        comboCount = newComboCount;
        potentialPoints = newPotentialPoints;
    }
}
