using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class MultiplierManager : MonoBehaviour
{
    [SerializeField] private GameObject multiplierUiPrefab;
    [SerializeField]private List<GameObject> multiplierEmblems;
    private int rowCount;
    private int columnCount;

    void Start()
    {
        multiplierEmblems = new List<GameObject>();
        rowCount = 0;
        columnCount = 0;
    }

    private void OnPickUpAcquired(object sender, PickUpAcquiredEventArgs e)
    {
        RectTransform current = Instantiate(multiplierUiPrefab, transform).GetComponent<RectTransform>();
        float newX = 50 + (100 * columnCount);
        float newY = -50 - (100 * rowCount);

        columnCount++;
        current.anchoredPosition = new Vector2(newX, newY);

        if (current.anchoredPosition.x > gameObject.GetComponent<RectTransform>().rect.width)
        {
            rowCount++;
            newX = multiplierEmblems[0].GetComponent<RectTransform>().anchoredPosition.x;
            newY = -50 - (100 * rowCount);
            current.anchoredPosition = new Vector2(newX, newY);
            columnCount = 1;
        }


        multiplierEmblems.Add(current.gameObject);

    }

    void OnEnable()
    {
        PickUp.PickUpAcquired += OnPickUpAcquired;
    }

    void OnDisable()
    {
        PickUp.PickUpAcquired -= OnPickUpAcquired;
    }
}
