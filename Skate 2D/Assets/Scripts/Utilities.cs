using System;
using System.Reflection;
using UnityEngine;

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
        }else if(value < 1000000)
        {
            return GetDecimalPoint((float)value / 1000,1) + "K";
        }else if(value < 1000000000)
        {
            return GetDecimalPoint((float)value / 1000000,1) + "M";
        }

        return "***";
    }

        /// <param name="value">The number you wish to reduce to specific decimal points</param>
    /// <param name="decimalPoint">The decimal point you want to receive</param>
    /// <returns>Returns a string float number with the provided decimal points, if any</returns>
    private static string GetDecimalPoint(float value, int decimalPoint)
    {
        string temp = ""; temp += value.ToString()[0] + ".";
        for(int i = 2; i < value.ToString().Length; i ++)
        {
            if((i - 2) >= decimalPoint) {break;}
            temp += value.ToString()[i];
        }
        return temp;
    }
}
