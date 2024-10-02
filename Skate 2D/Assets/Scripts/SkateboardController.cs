using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{
    private Animator animator;
    public TextMeshProUGUI text;

    void Start()
    {
        animator = GetComponent<Animator>();
        TouchControls.swipeRightEvent += Kickflip;
        TouchControls.swipeDownEvent += Shuvit;
        TouchControls.tapEvent += Ollie;
        TouchControls.swipeLeftEvent += Heelflip;
    }

    private void Kickflip(object sender, EventArgs e)
    {
        text.text = "Kickflip";
        animator.SetTrigger("kickflip");
    }

    private void Shuvit(object sender, EventArgs e)
    {
        text.text = "Shuvit";
        animator.SetTrigger("shuvit");
    }

    private void Ollie(object sender, EventArgs e)
    {
        text.text = "Ollie";
        animator.SetTrigger("ollie");
    }

    private void Heelflip(object sender, EventArgs e)
    {
        text.text = "Heelflip";
        animator.SetTrigger("heelflip");
    }
}
