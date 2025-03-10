using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardTrickPerformedEventArgs : EventArgs
{
    public string trickName {get;}
    public bool isCombo {get;}
    public int comboCount {get;}

    public SkateboardTrickPerformedEventArgs(string newTrickName, int newComboCount,bool newCombo)
    {
        trickName = newTrickName;
        isCombo = newCombo;
        comboCount = newComboCount;
    }
}
