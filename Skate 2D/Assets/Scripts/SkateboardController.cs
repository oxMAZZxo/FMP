using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class SkateboardController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [SerializeField,Range(1f,100f)]private float minMovementSpeed = 50f;
    [SerializeField,Range(0.1f,5f)]private float minVelocity = 3f;
    [SerializeField,Range(1f,200f)]private float maxMovementSpeed = 150f;
    [SerializeField,Range(0.1f,10f)]private float maxVelocity = 5f;
    [SerializeField,Range(0.1f,1f)]private float minimumJumpForce = 1f;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask whatIsGround;
    [SerializeField,Range(0.01f,1f)]private float groundedCheckRadius = 0.2f; 
    [SerializeField]private LayerMask whatIsGrindable;
    [SerializeField,Range(0.01f,1f)]private float grindableCheckRadius = 0.3f;
    [SerializeField]private GameObject backWheelSparks;
    [SerializeField]private GameObject frontWheelSparks;
    [SerializeField]private Slider jumpForceSlider;
    private bool isGrounded;
    private float currentTouchTime;
    private bool isGrinding;
    private int potentialPoints; //this are points that will be awarded if a trick is landed
    private bool isCharging;
    private bool isStopped;
    private bool hasStarted;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        jumpForceSlider.maxValue = 1.5f;
    }

    void FixedUpdate()
    {

        if(isStopped) {return;}

        if(Mathf.Abs(rb.velocity.x) > minVelocity) { hasStarted = true; }

        if(Mathf.Abs(rb.velocity.x) < minVelocity)
        {
            rb.AddForce(transform.right * minMovementSpeed * 10 * Time.fixedDeltaTime);
        }

        if(hasStarted && rb.velocity.x < 0.3f)
        {
            isStopped = true;
            GameManager.Instance.SessionEnded();
            GameOver();
        }

        //While the player is performing a grind, we want to make sure to enable physics
        //as the skateboard is reaching the end of the grindable obstacle
        if(isGrinding) 
        {
            if(CheckIsGrinding() == false)
            {
                rb.constraints = RigidbodyConstraints2D.None;
                rb.gravityScale = 1;
                isGrinding = false;
                animator.SetBool("isGrinding",false);
                backWheelSparks.SetActive(false);
                frontWheelSparks.SetActive(false);
            }

        }else
        {
            CheckGrounded();
        }

        if(isCharging){
            jumpForceSlider.value += Time.fixedDeltaTime;
        }else
        {
            jumpForceSlider.value = 0;
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
				if (!wasGrounded) //meaning you just landed
				{
                    GameManager.Instance.AddScore(potentialPoints);
                    potentialPoints = 0;
                }
			}
		}
    }

    public void Jump()
    {
        // if (isGrounded || isGrinding)
		// {
            //if the code reaches this point of execution
            //it is assumed that the player is perfoming a trick
            //therefore we reset gravity if they were performing a grind
            if(isGrinding)
            {
                rb.constraints = RigidbodyConstraints2D.None;
                rb.gravityScale = 1;
                isGrinding = false; //which means that they would no longer be doing a grind
            }
            // Debug.Log($"Current touch time: {currentTouchTime}");
            // Debug.Log($"Jump force will be: {minimumJumpForce * (100 + (100 * currentTouchTime))}");
			rb.AddForce(new Vector2(0f, minimumJumpForce * (100 + (100 * currentTouchTime))));
        // }
    }

    private void Move()
    {
        if(isGrounded && Mathf.Abs(rb.velocity.x) < maxVelocity)
        {
            rb.AddForce(transform.right * maxMovementSpeed);
        }
    }

    private void Action(object sender, TouchEventArgs e)
    {
        currentTouchTime = e.touchTime + 1;
        if(currentTouchTime > 2.5f) {currentTouchTime = 2.5f;}
        if(e.swipeDirection == SwipeDirection.NONE)
        {
            // Move();
            return;
        }

        //if the player is not on the ground and isn't grinding
        //meaning they are in the air, and they perform an action
        if(!isGrounded && !isGrinding)
        {
            CheckCanGrind(e.swipeDirection); //Check If the player can grind
        }else //else if one of those conditions is true
        {
            backWheelSparks.SetActive(false);
            frontWheelSparks.SetActive(false);
            ShowTrickAnimation(e.swipeDirection);//perfrom a trick
            if(isGrinding) // if they are grinding before the trick
            {
                animator.SetBool("isGrinding",false); //disable the grind animations
                //so that the trick animation can play
            }
        }
        
    }

    private void ShowTrickAnimation(SwipeDirection swipeDirection)
    {
        switch(swipeDirection)
        {
            case SwipeDirection.UP:
            animator.SetTrigger("ollie");
            potentialPoints +=1;
            break;

            case SwipeDirection.DOWN:
            animator.SetTrigger("shuvit");
            potentialPoints +=2;
            break;

            case SwipeDirection.RIGHT:
            animator.SetTrigger("kickflip");
            potentialPoints +=5;
            break;

            case SwipeDirection.LEFT:
            animator.SetTrigger("heelflip");
            potentialPoints +=5;
            break;

        }
        GameManager.Instance.IncrementNumberOfTricks();        
    }

    private void CheckCanGrind(SwipeDirection swipeDirection)
    {
        Collider2D other;
        bool grindable = CheckGrindable(out other); //check if player is above a grindable obstacle
        if(!grindable || swipeDirection != SwipeDirection.DOWN && swipeDirection != SwipeDirection.LEFT && swipeDirection != SwipeDirection.RIGHT) {return;}

        //positions the player above the grindable obstacle
        float distanceToMove = other.bounds.max.y - GetComponent<Collider2D>().bounds.min.y;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        transform.position = new Vector2(transform.position.x,transform.position.y + distanceToMove);
        isGrinding = true;    
        rb.rotation = 0;
        //show grind animation
        ShowGrindAnimation(swipeDirection);         
    }

    private void ShowGrindAnimation(SwipeDirection swipeDirection)
    {
        switch (swipeDirection)
        {
            case SwipeDirection.DOWN:
            animator.SetTrigger("50-50");
            potentialPoints +=5;
            backWheelSparks.SetActive(true);
            frontWheelSparks.SetActive(true);
            break;

            case SwipeDirection.LEFT:
            animator.SetTrigger("5-0 Grind");
            backWheelSparks.SetActive(true);
            potentialPoints +=10;
            break;

            case SwipeDirection.RIGHT:
            animator.SetTrigger("Nose Grind");
            potentialPoints +=10;
            frontWheelSparks.SetActive(true);
            break;
        }

        animator.SetBool("isGrinding",true);   
        GameManager.Instance.IncrementNumberOfTricks();        

    }

    private bool CheckGrindable(out Collider2D outCollider)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, grindableCheckRadius, whatIsGrindable);
        
		foreach(Collider2D collider in colliders)
        {
            outCollider = collider;
            if(collider.CompareTag("Grindable"))
            {
                return true;
            }
        }
        outCollider = null;
        return false;
    }

    private bool CheckIsGrinding()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, grindableCheckRadius, whatIsGrindable);
        
		foreach(Collider2D collider in colliders)
        {
            if(collider.CompareTag("Grindable"))
            {
                return true;
            }
        }
        
        return false;
    }

    private void OnTouchStarted(object sender, EventArgs e)
    {
        // Debug.Log("Touch Started");
        isCharging = true;
    }

    private void OnTouchEnded(object sender, EventArgs e)
    {
        // Debug.Log("Touch Ended");
        isCharging = false;
    }

    void OnEnable()
    {
        // Debug.Log("Enabling Skateboard Controller");
        TouchControls.touchEvent += Action;
        TouchControls.touchStarted += OnTouchStarted;
        TouchControls.touchEnded += OnTouchEnded;
    }

    void OnDisable()
    {
        // Debug.Log("Disabling Skateboard Controller");
        GameOver();
    }

    void GameOver()
    {
        TouchControls.touchEvent -= Action;
        TouchControls.touchStarted -= OnTouchStarted;
        TouchControls.touchEnded -= OnTouchEnded;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position,groundedCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position,grindableCheckRadius);
    }
}
