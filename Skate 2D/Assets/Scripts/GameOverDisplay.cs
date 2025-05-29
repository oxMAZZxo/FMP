using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI scoreDisplayFinal;
    [SerializeField]private TextMeshProUGUI noOfTricksDisplay;
    [SerializeField]private TextMeshProUGUI noOfCombosDisplay;
    [SerializeField]private TextMeshProUGUI longestComboDisplay;
    [SerializeField]private TextMeshProUGUI distanceTravelledDisplay;
    private int score;
    private int noOfTricks;
    private int noOfCombos;
    private int longestCombo;
    private float distanceTravelled;

    public void StartIncrementAnimation()
    {
        StartCoroutine(ScoreIncrementation());
        StartCoroutine(NoOfTricksIncrementation());
        StartCoroutine(NoOfCombosIncrementation());
        StartCoroutine(LongestComboIncrementation());
        StartCoroutine(DistanceTravelledIncrementation());
    }

    private IEnumerator ScoreIncrementation()
    {
        int addition = 1;
        if(score > 100) {addition = 50;}
        if(score > 1000) {addition = 100;}
        if(score > 10000) {addition = 250;}
        for(int i = 0; i <= score; i+= addition)
        {
            yield return new WaitForSeconds(0.01f);
            if(i > score)
            {
                scoreDisplayFinal.text = $"Score: {score}";
                break;
            }
            scoreDisplayFinal.text = $"Score: {i}";
        }
    }

    private IEnumerator NoOfTricksIncrementation()
    {
        int addition = 1;
        if (noOfTricks > 1000) { addition = 10; }
        if (noOfTricks > 10000) { addition = 50; }
        for (int i = 0; i <= noOfTricks; i += addition)
        {
            yield return new WaitForSeconds(0.001f);
            if (i > noOfTricks)
            {
                noOfTricksDisplay.text = $"Tricks: {noOfTricks}";
                break;
            }
            noOfTricksDisplay.text = $"Tricks: {i}";
        }
    }

    private IEnumerator NoOfCombosIncrementation()
    {
        int addition = 1;
        if(noOfCombos > 1000) {addition = 10;}
        if(noOfCombos > 10000) {addition = 50;}
        for(int i = 0; i <= noOfCombos; i+= addition)
        {
            yield return new WaitForSeconds(0.01f);
            if(i > noOfCombos)
            {
                noOfCombosDisplay.text = $"Combos: {noOfCombos}";
                break;
            }
            noOfCombosDisplay.text = $"Combos: {i}";
        }
    }

    private IEnumerator LongestComboIncrementation()
    {
        int addition = 1;
        if(longestCombo > 1000) {addition = 10;}
        if(longestCombo > 10000) {addition = 50;}
        for(int i = 0; i <= longestCombo; i+= addition)
        {
            yield return new WaitForSeconds(0.1f);
            if(i > longestCombo)
            {
                longestComboDisplay.text = $"Longest Combo: {longestCombo}";
                break;
            }
            longestComboDisplay.text = $"Longest Combo: {i}";
        }
    }

    private IEnumerator DistanceTravelledIncrementation()
    {
        int addition = 1;
        if(distanceTravelled > 100) {addition = 3;}
        if(distanceTravelled > 1000) {addition = 10;}
        if(distanceTravelled > 10000) {addition = 50;}
        for(int i = 0; i <= distanceTravelled; i+= addition)
        {
            yield return new WaitForSeconds(0.01f);
            if(i > distanceTravelled)
            {
                distanceTravelledDisplay.text = $"Distance: {distanceTravelled}";
                break;
            }
            distanceTravelledDisplay.text = $"Distance: {i}";
        }
    }

    public void SetValues(int preMultiplierScore, int newNoOfTricks, int newNoOfCombos, int newLongestCombo, float newDistanceTravelled)
    {
        score = preMultiplierScore;
        noOfTricks = newNoOfTricks;
        noOfCombos = newNoOfCombos;
        longestCombo = newLongestCombo;
        distanceTravelled = newDistanceTravelled;
    }

    void OnDisable()
    {
        score = 0;
        noOfTricks = 0;
        noOfCombos = 0;
        longestCombo = 0;
        distanceTravelled = 0;
    }
}
