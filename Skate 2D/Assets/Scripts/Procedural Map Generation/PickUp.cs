using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickUp : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int minMultiplier = 1;
    [SerializeField, Range(5, 10)] private int maxMultiplier = 5;
    private int currentMultiplier;
    [SerializeField] private bool isTrickRequired;
    [SerializeField] private string trickRequired;
    [SerializeField] private TextMeshPro multiplierDisplay;
    public static event EventHandler<PickUpAcquiredEventArgs> PickUpAcquired;
    private bool triggered;
    private SpriteRenderer spriteRenderer;

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !triggered)
        {
            SkateboardController skateboard = collision.GetComponentInParent<SkateboardController>();
            if (isTrickRequired && skateboard.CurrentTrickPerformed != trickRequired)
            {
                return;
            }
            triggered = true;
            PickUpAcquired?.Invoke(this, new PickUpAcquiredEventArgs(currentMultiplier));
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (multiplierDisplay == null)
        {
            Debug.LogError("The Multiplier display in the PickUp class of a pickup has not been assigned.", this);
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        AssignMultiplier();
    }

    public void AssignMultiplier()
    {
        currentMultiplier = UnityEngine.Random.Range(minMultiplier, maxMultiplier + 1);
        multiplierDisplay.text = $"x{currentMultiplier}";

        float t = (currentMultiplier - 1) / 9f;// Normalize multiplier to [0,1]
        // Linearly interpolate color from green (1) to red (10)
        multiplierDisplay.color = Color.Lerp(Color.grey, Color.white,t);
        spriteRenderer.color = Color.Lerp(Color.yellow, Color.red, t);

    }

    public void OnDisable()
    {
        triggered = false;
    }
}
