using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Animator))]
public class SkateboardController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [SerializeField,Range(1f,200f)]private float movementSpeed = 1f;
    [SerializeField,Range(0.1f,10f)]private float maxX_Velocity = 1f;
    [SerializeField,Range(0.5f,5f)]private float minimumJumpForce = 1f;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask whatIsGround;
    const float groundedCheckRadius = 0.2f; 
    private bool isGrounded;
    private float currentTouchTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        if(Mathf.Abs(rb.velocity.x) != 0)
        {
            Debug.Log($"X Velocity: {rb.velocity.x}");
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
		isGrounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedCheckRadius, whatIsGround);
        
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				isGrounded = true;
				if (!wasGrounded)
				{

                }
			}
		}
    }

    public void Jump()
    {
        if (isGrounded)
		{
			// Add a vertical force to the player.
			isGrounded = false;
			rb.AddForce(new Vector2(0f, minimumJumpForce * (100 + (10 * currentTouchTime))));
		}
    }

    private void Move(object sender, TouchEventArgs e)
    {
        if(Mathf.Abs(rb.velocity.x) < maxX_Velocity)
        {
            rb.AddForce(transform.right * movementSpeed);
        }
    }

    private void Kickflip(object sender, TouchEventArgs e)
    {
        if(!isGrounded) {return;}
        animator.SetTrigger("kickflip");
        currentTouchTime = e.touchTime;
    }

    private void Shuvit(object sender, TouchEventArgs e)
    {
        if(!isGrounded) {return;}
        animator.SetTrigger("shuvit");
        currentTouchTime = e.touchTime;
    }

    private void Ollie(object sender, TouchEventArgs e)
    {
        if(!isGrounded) {return;}
        animator.SetTrigger("ollie");
        currentTouchTime = e.touchTime;
    }

    private void Heelflip(object sender, TouchEventArgs e)
    {
        if(!isGrounded) {return;}
        animator.SetTrigger("heelflip");
        currentTouchTime = e.touchTime;
    }

    void OnEnable()
    {
        TouchControls.tapEvent += Move;
        TouchControls.swipeRightEvent += Kickflip;
        TouchControls.swipeDownEvent += Shuvit;
        TouchControls.swipeUpEvent += Ollie;
        TouchControls.swipeLeftEvent += Heelflip;
    }

    void OnDisable()
    {
        TouchControls.tapEvent -= Move;
        TouchControls.swipeRightEvent -= Kickflip;
        TouchControls.swipeDownEvent -= Shuvit;
        TouchControls.swipeUpEvent -= Ollie;
        TouchControls.swipeLeftEvent -= Heelflip;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position,groundedCheckRadius);
    }
}
