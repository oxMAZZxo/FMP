using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
[RequireComponent(typeof(AudioManager))]
public class SkateboardController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private AudioManager audioManager;
    [Header("Movement & Tricks")]
    [SerializeField, Range(0.01f, 1f)] private float movementSmoothing = 1f;
    private float minVelocity = 3f;
    [SerializeField, Range(0.1f, 1f)] private float minimumJumpForce = 1f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField, Range(0.01f, 1f)] private float groundedCheckRadius = 0.2f;
    [Header("Grinds")]
    [SerializeField] private LayerMask whatIsGrindable;
    [SerializeField] private Vector2 grindableCheckBoxSize;
    [SerializeField] private Vector2 isGrindingCheckBoxSize;
    [Header("SFX")]
    [SerializeField] private GameObject backWheelSparks;
    [SerializeField] private GameObject frontWheelSparks;
    [SerializeField] private GameObject frontSmokeParticles;
    [SerializeField] private GameObject backSmokeParticles;
    [SerializeField] private GameObject rollingSmokeParticles;
    [SerializeField] private TrailRenderer grindingTrail;
    [Header("Visualisation")]
    public bool drawGizmos;
    private bool isGrounded;
    private float currentTouchTime;
    private bool isGrinding;
    private int potentialPoints; //this are points that will be awarded if a trick is landed
    private bool isStopped;
    private bool hasStarted;
    private bool performedTrick;
    private bool isCombo;
    private int comboCounter = 1;
    private int longestCombo;
    private float distanceTravelled;
    private Vector2 m_Velocity;
    private bool reverseOut;
    private double oldPosX;
    private float grindingTime;
    private int pointsToBeAdded;
    private bool wasPaused;
    private float unPausedCounter;
    private bool disablingGrind;
    private float jumpHeight;
    public static event EventHandler<SkateboardTrickPerformedEventArgs> trickPerformed;
    public static event EventHandler<SkateboardLandEventArgs> skateboardLanded;
    private float preJumpYPosition;
    private Gradient originalGrindTrailGradient;
    public string CurrentTrickPerformed { get; private set; }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        audioManager = GetComponent<AudioManager>();
        originalGrindTrailGradient = grindingTrail.colorGradient;
    }

    void FixedUpdate()
    {
        if (isStopped || !GameManager.Instance.gameHasStarted || GameManager.Instance.isGamePaused) { return; }

        if (!hasStarted && transform.position.x > 0.01f)
        {
            hasStarted = true;
            EnableInput();
            audioManager.Play("Rolling");
        }

        if (isGrounded || isGrinding)
        {
            Vector3 targetVelocity = new Vector2(minVelocity * 50 * Time.fixedDeltaTime, rb.velocity.y);
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);
        }

        if (transform.position.x > distanceTravelled) { distanceTravelled = transform.position.x; }

        //While the player is performing a grind, we want to make sure to enable physics
        //as the skateboard is reaching the end of the grindable obstacle
        if (isGrinding)
        {
            if (CheckIsGrinding() == false)
            {
                DisableGrinding();
            }
            grindingTime += Time.fixedDeltaTime;
        }
        CheckGrounded();

        if (!isGrounded && !isGrinding)
        {
            Quaternion rot = transform.rotation;
            Vector3 euler = rot.eulerAngles;

            // Ensure angles are within the range (-180, 180) for correct clamping
            if (euler.z > 180) euler.z -= 360;

            euler.z = Mathf.Clamp(euler.z, -10f, 10f); // Adjust limits as needed

            transform.rotation = Quaternion.Euler(euler);
            float currentHeight = transform.position.y - preJumpYPosition;
            if (currentHeight > jumpHeight) { jumpHeight = currentHeight; }
            audioManager.Stop("Rolling");
        }
        oldPosX = transform.position.x;
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isGamePaused && wasPaused)
        {
            unPausedCounter += Time.deltaTime;
            if (unPausedCounter > 0.5f)
            {
                wasPaused = false;
                unPausedCounter = 0;
            }
        }

        if (!isStopped && !wasPaused && hasStarted && (transform.position.x - oldPosX) == 0)
        {
            isStopped = true;
            audioManager.Stop("Rolling");
            GameManager.Instance.SessionEnded(longestCombo, distanceTravelled);
            DisableInput();
        }
    }

    public void Pause()
    {
        rb.simulated = false;
        DisableInput();
        wasPaused = true;

        if (isGrounded)
        {
            audioManager.Stop("Rolling");
        }
        if (isGrinding)
        {
            audioManager.Stop("Mid Grind");
        }
        if (!isGrinding && !isGrounded)
        {
            audioManager.Stop("Wheel Spinning");
        }
    }

    public void Resume()
    {
        rb.simulated = true;
        EnableInput();

        if (isGrounded)
        {
            audioManager.Play("Rolling");
        }
        if (isGrinding)
        {
            audioManager.Play("Mid Grind");
        }
        if (!isGrinding && !isGrounded)
        {
            audioManager.Play("Wheel Spinning");
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
                    skateboardLanded?.Invoke(this, new SkateboardLandEventArgs(potentialPoints, comboCounter, jumpHeight));
                    OnLand();
                }
            }
        }
    }

    private void OnLand()
    {
        if (!hasStarted) { return; }
        CurrentTrickPerformed = "";
        animator.SetBool("reverseOut", false);
        reverseOut = false;
        frontSmokeParticles.SetActive(true);
        backSmokeParticles.SetActive(true);
        rollingSmokeParticles.SetActive(true);
        performedTrick = false;
        if (isCombo && comboCounter > 3)
        {
            AudioManager.Global.Play("OnLandComboSFX");
        }
        isCombo = false;
        if (comboCounter > longestCombo) { longestCombo = comboCounter; }
        potentialPoints = 0;
        comboCounter = 1;
        jumpHeight = 0;
        rb.constraints = RigidbodyConstraints2D.None;
        audioManager.Play("Landed");
        audioManager.Play("Rolling");
        audioManager.Stop("Wheel Spinning");
        grindingTrail.colorGradient = originalGrindTrailGradient;
    }

    public void Jump()
    {
        performedTrick = true;
        float jumpForce = minimumJumpForce * (100 + (100 * currentTouchTime));
        if (!isGrinding && !isGrounded) { jumpForce += 25; }
        if (isGrinding)
        {
            jumpForce += 15;
            DisableGrinding();
        }
        if (isGrounded) { audioManager.Stop("Rolling"); }
        rollingSmokeParticles.SetActive(false);
        audioManager.Play("Tail Snap");
        audioManager.Play("Wheel Spinning");
        preJumpYPosition = transform.position.y;
        rb.AddForce(new Vector2(0f, jumpForce));
    }

    private void OnInputAction(object sender, TouchEventArgs e)
    {
        currentTouchTime = e.touchTime + 1;
        if (currentTouchTime > 2f) { currentTouchTime = 2f; }
        if (e.swipeDirection == SwipeDirection.NONE)
        {
            return;
        }

        bool isPerformingTrick;
        string trickPerformed;

        isPerformingTrick = CalculateAction(e.swipeDirection, out trickPerformed);
        if (!isPerformingTrick) { return; }
        StartCoroutine(DisplayTrick(trickPerformed));
    }

    /// <summary>
    /// Calculates if an action can be performed
    /// </summary>
    /// <param name="swipeDirection">The current swipe direction</param>
    /// <param name="trickPerformed">An out parameter which will return the action being performed</param>
    /// <returns>True if an action is being performed</returns>
    private bool CalculateAction(SwipeDirection swipeDirection, out string trickPerformed)
    {
        trickPerformed = "";
        bool isPerformingTrick = false;
        if (!isGrounded && !isGrinding)
        {
            //I check if I can perform a grind
            Collider2D other;
            bool grindable = CheckGrindable(out other, GetGrindCheckPosition(swipeDirection)); //check if player is above a grindable obstacle
            if (!grindable || swipeDirection != SwipeDirection.DOWN && swipeDirection != SwipeDirection.LEFT && swipeDirection != SwipeDirection.RIGHT) { return false; }
            trickPerformed = PerformGrind(swipeDirection, other);
            isPerformingTrick = true;
            grindingTrail.transform.position = backWheelSparks.transform.position;
            if (reverseOut) { grindingTrail.transform.position = frontWheelSparks.transform.position; }
            grindingTrail.emitting = true;
        }
        else //else if one of those conditions is true
        {
            //player performs a trick
            trickPerformed = ShowTrickAnimation(swipeDirection);//perfrom a trick
            if (trickPerformed == "")
            {
                return false;
            }
            isPerformingTrick = true;
            backWheelSparks.SetActive(false);
            frontWheelSparks.SetActive(false);
            if (isGrinding) // if they are grinding before the trick
            {
                animator.SetBool("isGrinding", false); //disable the grind animations
                //so that the trick animation can play
            }
        }

        return isPerformingTrick;
    }



    private IEnumerator DisplayTrick(string trickPerformedOutput)
    {
        if (performedTrick)
        {
            isCombo = true;
            comboCounter++;
        }
        bool isGrind = false;
        if (trickPerformedOutput == "Nose Grind" || trickPerformedOutput == "5-0" || trickPerformedOutput == "50-50") { isGrind = true; }
        if (isGrind) { GrindingSparksFX(); }
        yield return new WaitForSeconds(0.12f);
        if (disablingGrind)
        {
            disablingGrind = false;
            Debug.Log("Grind was invalid");
            yield break;
        }
        CurrentTrickPerformed = trickPerformedOutput;
        trickPerformed?.Invoke(this, new SkateboardTrickPerformedEventArgs(trickPerformedOutput, comboCounter, isCombo, potentialPoints, isGrind));
    }

    private void GrindingSparksFX()
    {
        // Get the gradient from the TrailRenderer
        Gradient gradient = grindingTrail.colorGradient;

        // Copy the existing keys
        GradientColorKey[] colorKeys = gradient.colorKeys;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        // Modify the first color key
        colorKeys[0].color = new Color(colorKeys[0].color.r, colorKeys[0].color.g - 0.075f, 0);
        colorKeys[0].time = 0f; // make sure it stays at the start

        // Create and apply a new gradient
        Gradient newGradient = new Gradient();
        newGradient.SetKeys(colorKeys, alphaKeys);

        // Assign it back to the TrailRenderer
        grindingTrail.colorGradient = newGradient;
    }

    /// <summary>
    /// Triggers a trick animations with the provided swipe direction.
    /// </summary>
    /// <param name="swipeDirection">The swipe direction</param>
    /// <returns>Returns the trick performed</returns>
    private string ShowTrickAnimation(SwipeDirection swipeDirection)
    {
        string trickOutput = "";
        switch (swipeDirection)
        {
            case SwipeDirection.UP:
                animator.SetTrigger("ollie");
                potentialPoints += 1;
                trickOutput = "Ollie";
                break;

            case SwipeDirection.DOWN:
                animator.SetTrigger("shuvit");
                potentialPoints += 2;
                trickOutput = "Shuvit";
                break;

            case SwipeDirection.RIGHT:
                animator.SetTrigger("kickflip");
                potentialPoints += 5;
                trickOutput = "Kickflip";
                break;

            case SwipeDirection.LEFT:
                animator.SetTrigger("heelflip");
                potentialPoints += 5;
                trickOutput = "Heelflip";
                break;

            case SwipeDirection.DOWN_RIGHT:
                animator.SetTrigger("treflip");
                potentialPoints += 10;
                trickOutput = "360 Kickflip";
                break;

        }

        if (trickOutput != "" && animator.GetBool("reverseOut"))
        {
            trickOutput = "Nollie " + trickOutput;
        }
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
        float skateboardBottomBounds = myCollider.bounds.center.y - myCollider.bounds.extents.y;
        float obstacleTopBounds = obstacle.bounds.center.y + obstacle.bounds.extents.y;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        transform.position = new Vector2(transform.position.x, transform.position.y + obstacleTopBounds - skateboardBottomBounds);
        isGrinding = true;
        rb.rotation = 0;
        Physics2D.SyncTransforms();
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
        animator.SetBool("reverseOut", false);
        reverseOut = false;
        switch (swipeDirection)
        {
            case SwipeDirection.DOWN:
                animator.SetTrigger("50-50");
                pointsToBeAdded = 5;
                backWheelSparks.SetActive(true);
                frontWheelSparks.SetActive(true);
                trickOutput = "50-50";
                break;

            case SwipeDirection.LEFT:
                animator.SetTrigger("5-0 Grind");
                backWheelSparks.SetActive(true);
                pointsToBeAdded = 10;
                trickOutput = "5-0";
                break;

            case SwipeDirection.RIGHT:
                animator.SetTrigger("Nose Grind");
                pointsToBeAdded = 10;
                frontWheelSparks.SetActive(true);
                trickOutput = "Nose Grind";
                animator.SetBool("reverseOut", true);
                reverseOut = true;
                break;
        }
        potentialPoints += pointsToBeAdded;
        animator.SetBool("isGrinding", true);
        audioManager.Play("Start Grind");
        audioManager.Play("Mid Grind");
        return trickOutput;
    }

    private Vector2 GetGrindCheckPosition(SwipeDirection swipeDirection)
    {
        if (swipeDirection == SwipeDirection.RIGHT || swipeDirection == SwipeDirection.DOWN_RIGHT || swipeDirection == SwipeDirection.UP_RIGHT)
        {
            return frontWheelSparks.transform.position;
        }
        if (swipeDirection == SwipeDirection.LEFT || swipeDirection == SwipeDirection.DOWN_LEFT || swipeDirection == SwipeDirection.UP_LEFT)
        {
            return backWheelSparks.transform.position;
        }
        return groundCheck.position;
    }

    /// <summary>
    /// Returns true if the obstacle below the skateboard is 'grindable'
    /// </summary>
    /// <param name="outCollider">Out parameter which is the first obstacle that is in the grind radius .</param>
    /// <returns></returns>
    private bool CheckGrindable(out Collider2D outCollider, Vector2 grindCheckPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(grindCheckPosition, grindableCheckBoxSize, whatIsGrindable);
        foreach (Collider2D collider in colliders)
        {
            outCollider = collider;
            if (collider.CompareTag("Grindable"))
            {
                return true;
            }
        }
        outCollider = null;
        return false;
    }

    private bool CheckIsGrinding()
    {
        Vector2 grindCheckPosition = backWheelSparks.transform.position;

        if (reverseOut)
        {
            grindCheckPosition = frontWheelSparks.transform.position;
        }

        Collider2D[] colliders = Physics2D.OverlapBoxAll(grindCheckPosition, isGrindingCheckBoxSize, whatIsGrindable);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Grindable"))
            {
                audioManager.Stop("Wheel Spinning");
                return true;
            }
        }

        return false;
    }

    private void DisableGrinding()
    {
        if (grindingTime < 0.1)
        {
            disablingGrind = true;
            RemoveGrind();
        }
        audioManager.Stop("Mid Grind");
        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = 1;
        isGrinding = false;
        animator.SetBool("isGrinding", false);
        backWheelSparks.SetActive(false);
        frontWheelSparks.SetActive(false);
        grindingTime = 0;
        grindingTrail.emitting = false;
    }

    private void RemoveGrind()
    {
        potentialPoints -= pointsToBeAdded;
        comboCounter -= 1;
        GameManager.Instance.DecrementNumberOfTricks();
    }

    private void OnTouchStarted(object sender, Vector2 start)
    {
        // Debug.Log("Touch Started");
        animator.SetBool("isHoldingTouch", true);
    }

    private void OnTouchEnded(object sender, Vector2 end)
    {
        // Debug.Log("Touch Ended");
        animator.SetBool("isHoldingTouch", false);
    }

    public void SetMinVelocity(float value)
    {
        minVelocity = value;
    }

    private void EnableInput()
    {
        TouchControls.touchEvent += OnInputAction;
        TouchControls.touchStarted += OnTouchStarted;
        TouchControls.touchEnded += OnTouchEnded;
    }

    private void DisableInput()
    {
        TouchControls.touchEvent -= OnInputAction;
        TouchControls.touchStarted -= OnTouchStarted;
        TouchControls.touchEnded -= OnTouchEnded;
    }

    public void Reset()
    {
        comboCounter = 1;
        longestCombo = 0;
        performedTrick = false;
        isStopped = false;
        hasStarted = false;
        wasPaused = false;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
        DisableInput();
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) { return; }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundedCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(backWheelSparks.transform.position, grindableCheckBoxSize);
        Gizmos.DrawWireCube(frontWheelSparks.transform.position, grindableCheckBoxSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(backWheelSparks.transform.position, isGrindingCheckBoxSize);
        Gizmos.DrawWireCube(frontWheelSparks.transform.position, isGrindingCheckBoxSize);
    }
}