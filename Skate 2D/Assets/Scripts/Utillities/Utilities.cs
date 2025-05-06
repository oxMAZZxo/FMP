using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// The Utilities class contains static functions which perform logic that can be used throughout a system.
/// </summary>
public class Utilities
{
    /// <summary>
    /// Clears Unity's Console
    /// </summary>
    public static void ClearConsole()
    {
        Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
        MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }

    /// <param name="value"></param>
    /// <returns>Returns a shortened version of a number with the appropriate suffix</returns>
    public static string PrettyNumberString(float value) 
    {
        if(value < 1000)
        {
            return value.ToString();
        }else if(value < 100000) //less than 100k
        {
            return GetDecimalPoint((float)value / 1000,1) + "K";
        }else if(value < 1000000) //less than 1m
        {
            return GetDecimalPoint((float)value / 1000,0) + "K";
        }else if(value < 100000000) //less than 100m
        {
            return GetDecimalPoint((float)value / 1000000,1) + "M";
        }else if(value < 1000000000) //less than 1b
        {
            return GetDecimalPoint((float)value / 1000000,0) + "M";
        }

        return "***";
    }

    /// <param name="value">The number you wish to reduce to specific decimal points</param>
    /// <param name="maxDecimalPoint">The decimal point you want to receive</param>
    /// <returns>Returns a string float number with the provided decimal points, if any</returns>
    public static string GetDecimalPoint(float value, int maxDecimalPoint)
    {
        string floatValue = "";
        int significantFigureCounter;
        //loop to find all the significant figures, before the decimal
        for(significantFigureCounter = 0; significantFigureCounter < value.ToString().Length; significantFigureCounter++)
        {
            if(value.ToString()[significantFigureCounter] == '.') {break;}
            floatValue += value.ToString()[significantFigureCounter];
        }
        if(maxDecimalPoint == 0)
        {
            return floatValue;
        }
        floatValue += '.';
        //loop to get all the decimal figures
        for(int j = significantFigureCounter +1; j < value.ToString().Length; j++)
        {
            if(j - (significantFigureCounter + 1) == maxDecimalPoint) //if the current counter - the offset we added is equal to the decimal point then we can terminate
            {
                break;
            }
            floatValue += value.ToString()[j];
        }
        return floatValue;
    }
}
