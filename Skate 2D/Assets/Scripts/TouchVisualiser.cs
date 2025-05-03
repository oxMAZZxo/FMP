using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TouchVisualiser : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isTouching;


    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();  
        spriteRenderer = GetComponent<SpriteRenderer>();  
        spriteRenderer.enabled = false;
        trailRenderer.emitting = false;
    }

    void FixedUpdate()
    {
        if(!isTouching) {return;}
        transform.position = TouchControls.Instance.GetTouchToWorldPoint();
    }

    void OnTouchStarted(object sender, Vector2 position) {
        isTouching = true;
        spriteRenderer.enabled = true;
        trailRenderer.emitting = true;
    }

    void OnTouchEnded(object sender, Vector2 position)
    {
        isTouching = false;
        spriteRenderer.enabled = false;
        trailRenderer.emitting = false;
    }

    void OnEnable()
    {
        TouchControls.touchStarted += OnTouchStarted;
        TouchControls.touchEnded += OnTouchEnded;
    }

    void OnDisable()
    {
        TouchControls.touchStarted -= OnTouchStarted;
        TouchControls.touchEnded -= OnTouchEnded;
    }
}
