using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class SkateboardController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    [SerializeField,Range(1f,100f)]private float minMovementSpeed = 50f;
    private float minVelocity = 3f;
    [SerializeField,Range(0.1f,1f)]private float minimumJumpForce = 1f;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask whatIsGround;
    [SerializeField,Range(0.01f,1f)]private float groundedCheckRadius = 0.2f; 
    [SerializeField]private LayerMask whatIsGrindable;
    [SerializeField,Range(0.01f,1f)]private float grindableCheckRadius = 0.3f;
    [SerializeField]private GameObject backWheelSparks;
    [SerializeField]private GameObject frontWheelSparks;
    [SerializeField]private GameObject frontSmokeParticles;
    [SerializeField]private GameObject backSmokeParticles;
    [SerializeField]private Slider jumpForceSlider;
    [SerializeField]private TextMeshProUGUI comboDisplay;
    [SerializeField]private TextMeshProUGUI comboCounterDisplay;
    private bool isGrounded;
    private float currentTouchTime;
    private bool isGrinding;
    private int potentialPoints; //this are points that will be awarded if a trick is landed
    private bool isCharging;
    private bool isStopped;
    private bool hasStarted;
    private bool performedTrick;
    private bool isCombo;
    private int comboCounter = 1;
    private int longestCombo;
    private float distanceTravelled;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        jumpForceSlider.maxValue = 1f;
    }

    void FixedUpdate()
    {
        if(isStopped) {return;}

        if(Mathf.Abs(rb.velocity.x) > minVelocity) { hasStarted = true; }

        if((isGrounded || isGrinding) && Mathf.Abs(rb.velocity.x) < minVelocity)
        {
            rb.AddForce(transform.right * minMovementSpeed * 10 * Time.fixedDeltaTime);
        }

        if(transform.position.x > distanceTravelled) {distanceTravelled = transform.position.x;}

        if(hasStarted && rb.velocity.x < 0.3f)
        {
            isStopped = true;
            GameManager.Instance.SessionEnded(longestCombo,distanceTravelled);
            GameOver();
        }

        //While the player is performing a grind, we want to make sure to enable physics
        //as the skateboard is reaching the end of the grindable obstacle
        if(isGrinding) 
        {
            if(CheckIsGrinding() == false)
            {
                DisableGrinding();
            }
        }
        CheckGrounded();
        

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
                    animator.SetBool("reverseOut", false);
                    frontSmokeParticles.SetActive(true);
                    backSmokeParticles.SetActive(true);
                    performedTrick = false;
                    if(isCombo){
                        GameManager.Instance.IncrementNumberOfCombos();
                    }
                    isCombo = false;
                    GameManager.Instance.AddScore(potentialPoints * comboCounter);
                    if(comboCounter > longestCombo) { longestCombo = comboCounter;}
                    potentialPoints = 0;
                    comboCounter = 1;
                    comboDisplay.gameObject.SetActive(false);
                    comboCounterDisplay.gameObject.SetActive(false);
                    comboDisplay.text = "";
                    comboCounterDisplay.text = "";
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
            performedTrick = true;
            // Debug.Log("Performing Trick");
            if(isGrinding)
            {
                DisableGrinding();
            }
            // Debug.Log($"Current touch time: {currentTouchTime}");
            // Debug.Log($"Jump force will be: {minimumJumpForce * (100 + (100 * currentTouchTime))}");
			rb.AddForce(new Vector2(0f, minimumJumpForce * (100 + (100 * currentTouchTime))));
        // }
    }

    private void Action(object sender, TouchEventArgs e)
    {
        currentTouchTime = e.touchTime + 1;
        if(currentTouchTime > 2f) {currentTouchTime = 2f;}
        if(e.swipeDirection == SwipeDirection.NONE)
        {
            return;
        }

        bool isPerformingTrick;
        //if the player is not on the ground and isn't grinding
        //meaning they are in the air, and they perform an action
        string trickPerformed;
        if(!isGrounded && !isGrinding)
        {
            //I check if I can perform a grind
            Collider2D other;
            bool grindable = CheckGrindable(out other); //check if player is above a grindable obstacle
            if(!grindable || e.swipeDirection != SwipeDirection.DOWN && e.swipeDirection != SwipeDirection.LEFT && e.swipeDirection != SwipeDirection.RIGHT) {return;}
            trickPerformed = PerformGrind(e.swipeDirection,other);
            isPerformingTrick = true;
        }else //else if one of those conditions is true
        {
            //player performs a trick
            isPerformingTrick = true;
            backWheelSparks.SetActive(false);
            frontWheelSparks.SetActive(false);
            trickPerformed = ShowTrickAnimation(e.swipeDirection);//perfrom a trick
            if(isGrinding) // if they are grinding before the trick
            {
                animator.SetBool("isGrinding",false); //disable the grind animations
                //so that the trick animation can play
            }
        }

        if(performedTrick && isPerformingTrick)
        {
            isCombo = true;
            trickPerformed = " + " + trickPerformed;
            comboCounter++;
            comboCounterDisplay.text = comboCounter.ToString();
            comboDisplay.gameObject.SetActive(true);
            comboCounterDisplay.gameObject.SetActive(true);
        }
        comboDisplay.text += trickPerformed;        
    }

    /// <summary>
    /// Triggers a trick animations with the provided swipe direction.
    /// </summary>
    /// <param name="swipeDirection">The swipe direction</param>
    /// <returns>Returns the trick performed</returns>
    private string ShowTrickAnimation(SwipeDirection swipeDirection)
    {
        string trickOutput = "";
        switch(swipeDirection)
        {
            case SwipeDirection.UP:
            animator.SetTrigger("ollie");
            potentialPoints +=1;
            trickOutput = "Ollie";
            break;

            case SwipeDirection.DOWN:
            animator.SetTrigger("shuvit");
            potentialPoints +=2;
            trickOutput = "Shuvit";
            break;

            case SwipeDirection.RIGHT:
            animator.SetTrigger("kickflip");
            potentialPoints +=5;
            trickOutput = "Kickflip";
            break;

            case SwipeDirection.LEFT:
            animator.SetTrigger("heelflip");
            potentialPoints +=5;
            trickOutput = "Heelflip";
            break;

            case SwipeDirection.DOWN_RIGHT:
            animator.SetTrigger("treflip");
            potentialPoints += 10;
            trickOutput = "360 Kickflip";
            break;

        }
        
        if(animator.GetBool("reverseOut"))
        {
            trickOutput = "Nollie " + trickOutput;
        }
        GameManager.Instance.IncrementNumberOfTricks();     
        return trickOutput;
    }

    /// <summary>
    /// Performs a grind on a given obstacle.
    /// </summary>
    /// <param name="swipeDirection">The current swipe direction</param>
    /// <param name="obstacle">the obstacle to perform the grind on</param>
    /// <returns>Returns the grind performed</returns>
    private string PerformGrind(SwipeDirection swipeDirection, Collider2D obstacle)
    {
        //positions the player above the grindable obstacle
        float distanceToMove = obstacle.bounds.max.y - GetComponent<Collider2D>().bounds.min.y;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        transform.position = new Vector2(transform.position.x,transform.position.y + distanceToMove);
        isGrinding = true;    
        rb.rotation = 0;
        //show grind animation
        return ShowGrindAnimation(swipeDirection);       
    }

    /// <summary>
    /// Triggers a grind animation based on the swipe direction.
    /// </summary>
    /// <param name="swipeDirection">The swipe direction</param>
    private string ShowGrindAnimation(SwipeDirection swipeDirection)
    {
        string trickOutput = "";
        animator.SetBool("reverseOut",false);
        switch (swipeDirection)
        {
            case SwipeDirection.DOWN:
            animator.SetTrigger("50-50");
            potentialPoints +=5;
            backWheelSparks.SetActive(true);
            frontWheelSparks.SetActive(true);
            trickOutput = "50-50";
            break;

            case SwipeDirection.LEFT:
            animator.SetTrigger("5-0 Grind");
            backWheelSparks.SetActive(true);
            potentialPoints +=10;
            trickOutput = " 5-0";
            break;

            case SwipeDirection.RIGHT:
            animator.SetTrigger("Nose Grind");
            potentialPoints +=10;
            frontWheelSparks.SetActive(true);
            trickOutput = " Nose Grind";
            animator.SetBool("reverseOut", true);
            break;
        }
        animator.SetBool("isGrinding",true);   
        GameManager.Instance.IncrementNumberOfTricks();
        return trickOutput;
    }

    /// <summary>
    /// Returns true if the obstacle below the skateboard is 'grindable'
    /// </summary>
    /// <param name="outCollider">Out parameter which is the first obstacle that is in the grind radius .</param>
    /// <returns></returns>
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

    private void DisableGrinding()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = 1;
        isGrinding = false;
        animator.SetBool("isGrinding",false);
        backWheelSparks.SetActive(false);
        frontWheelSparks.SetActive(false);
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

    public void SetMinVelocity(float value)
    {
        minVelocity = value;
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