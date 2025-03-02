using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float velocityPower = 0.9f;
    [SerializeField] private float frictionAmount = 0.2f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float fallGravityMultiplier = 1.9f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private bool preventWallSticking = true;
    [SerializeField] private int numberOfWallChecks = 3;
    [SerializeField] private PhysicsMaterial2D frictionMaterial;

    [Header("Gravity Reversal")]
    [SerializeField] private float gravityReverseForce = 8f;
    [SerializeField] private float gravityStrength = 1f;

    // component references
    private Rigidbody2D rb;
    private Collider2D col;

    // movement variables
    private float moveInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;
    private bool isGrounded;
    private bool jumpInputReleased;
    private bool isTouchingWall;
    private bool isSlidingDownWall;

    // gravity variables
    private bool isGravityReversed = false;
    private Transform playerTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerTransform = transform;
        
        // create a frictionless material if one wasn't assigned
        if (frictionMaterial == null)
        {
            frictionMaterial = new PhysicsMaterial2D("NoFriction");
            frictionMaterial.friction = 0f;
            frictionMaterial.bounciness = 0f;
        }
    }

    void Update()
    {
        // get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // ground check
        CheckGrounded();
        
        // wall check
        CheckWallCollision();
        
        // handle coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // jump buffer
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // jump logic
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce((isGravityReversed ? Vector2.down : Vector2.up) * jumpForce, ForceMode2D.Impulse);
            
            isJumping = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
        
        // variable jump height
        if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) && 
            ((rb.velocity.y > 0 && !isGravityReversed) || (rb.velocity.y < 0 && isGravityReversed)))
        {
            jumpInputReleased = true;
        }

        // gravity reversal logic
        HandleGravityReversal();
    }

    void FixedUpdate()
    {
        // apply horizontal movement with acceleration and deceleration
        ApplyMovement();
        
        // apply friction when not moving horizontally
        ApplyFriction();
        
        // apply variable jump height and increased fall gravity
        ApplyJumpModifiers();
    }

    private void CheckGrounded()
    {
        Vector2 checkPosition = groundCheck.position;
        
        // check for ground in the direction of gravity
        if (isGravityReversed)
        {
            // move the check position upward when gravity is reversed
            checkPosition = new Vector2(groundCheck.position.x, 
                                       transform.position.y + col.bounds.extents.y + 0.1f);
        }
        
        // check if player is grounded
        isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);
        
        // reset jump state when landing
        if (isGrounded && 
            ((rb.velocity.y <= 0 && !isGravityReversed) || (rb.velocity.y >= 0 && isGravityReversed)))
        {
            isJumping = false;
            jumpInputReleased = false;
        }
    }

    private void HandleGravityReversal()
    {
        // check for gravity reversal input (space key) and if player is grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // toggle gravity state
            isGravityReversed = !isGravityReversed;
            
            // apply an initial force for the reversal
            rb.velocity = new Vector2(rb.velocity.x, 0f); // reset vertical velocity
            rb.AddForce((isGravityReversed ? Vector2.up : Vector2.down) * gravityReverseForce, ForceMode2D.Impulse);
            
            // flip the player upside down when gravity is reversed
            FlipPlayerVertically();
        }
        
        // apply custom gravity
        rb.gravityScale = 0f; // disable built-in gravity
        Vector2 gravityForce = (isGravityReversed ? Vector2.up : Vector2.down) * gravityStrength;
        rb.AddForce(gravityForce, ForceMode2D.Force);
    }

    private void FlipPlayerVertically()
    {
        // rotate the player 180 degrees around the z-axis
        playerTransform.rotation = Quaternion.Euler(0f, 0f, isGravityReversed ? 180f : 0f);
    }

    private void ApplyMovement()
    {
        // calculate target speed
        float targetSpeed = moveInput * moveSpeed;
        
        // calculate difference between current and target speed
        float speedDifference = targetSpeed - rb.velocity.x;
        
        // calculate acceleration rate based on whether we're accelerating or decelerating
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        
        // apply acceleration with non-linear movement for better control
        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelerationRate, velocityPower) * Mathf.Sign(speedDifference);
        
        // don't apply movement force if pushing into a wall while falling
        if (preventWallSticking && isSlidingDownWall)
        {
            // if trying to move into the wall
            if ((moveInput > 0 && isTouchingWall) || (moveInput < 0 && isTouchingWall))
            {
                // don't apply force at all
                movement = 0;
            }
        }
        
        // apply final movement force to the rigidbody
        rb.AddForce(movement * Vector2.right);
    }

    private void ApplyFriction()
    {
        // apply friction when no horizontal input
        if (Mathf.Abs(moveInput) < 0.01f && isGrounded)
        {
            float friction = Mathf.Min(Mathf.Abs(rb.velocity.x), frictionAmount);
            friction *= Mathf.Sign(rb.velocity.x);
            
            rb.AddForce(Vector2.right * -friction, ForceMode2D.Impulse);
        }
    }

    private void ApplyJumpModifiers()
    {
        // apply jump cut when player releases jump button
        if (jumpInputReleased)
        {
            if ((!isGravityReversed && rb.velocity.y > 0) || (isGravityReversed && rb.velocity.y < 0))
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
                jumpInputReleased = false;
            }
        }
        
        // determine if falling based on gravity direction
        bool isFalling = (!isGravityReversed && rb.velocity.y < 0) || (isGravityReversed && rb.velocity.y > 0);
        
        // apply custom gravity management instead of using gravity scale
        if (isFalling)
        {
            Vector2 extraGravity = (isGravityReversed ? Vector2.up : Vector2.down) * (gravityStrength * (fallGravityMultiplier - 1f));
            rb.AddForce(extraGravity, ForceMode2D.Force);
        }
    }

    // check for wall collisions
    private void CheckWallCollision()
    {
        if (col == null) return;
        
        bool rightWallDetected = false;
        bool leftWallDetected = false;
        
        // get the bounds of the collider
        float colliderHeight = col.bounds.size.y;
        float colliderWidth = col.bounds.size.x;
        
        // distribute ray origins along the height of the collider
        Vector2[] rayOrigins = new Vector2[numberOfWallChecks];
        for (int i = 0; i < numberOfWallChecks; i++)
        {
            float heightFraction = (float)i / (numberOfWallChecks - 1);
            float yOffset = Mathf.Lerp(-colliderHeight / 2 + 0.1f, colliderHeight / 2 - 0.1f, heightFraction);
            rayOrigins[i] = new Vector2(transform.position.x, transform.position.y + yOffset);
        }
        
        // cast rays from each origin point
        foreach (Vector2 origin in rayOrigins)
        {
            // right side check
            RaycastHit2D rightWallHit = Physics2D.Raycast(
                origin,
                Vector2.right,
                colliderWidth / 2 + wallCheckDistance,
                groundLayer
            );
            
            // left side check
            RaycastHit2D leftWallHit = Physics2D.Raycast(
                origin,
                Vector2.left,
                colliderWidth / 2 + wallCheckDistance,
                groundLayer
            );
            
            // update detection flags
            if (rightWallHit.collider != null) rightWallDetected = true;
            if (leftWallHit.collider != null) leftWallDetected = true;
        }
        
        // update collision flags
        isTouchingWall = rightWallDetected || leftWallDetected;
        
        // check if sliding down a wall (touching wall, not grounded, and falling)
        bool isFalling = (!isGravityReversed && rb.velocity.y < 0) || (isGravityReversed && rb.velocity.y > 0);
        isSlidingDownWall = isTouchingWall && !isGrounded && isFalling;
        
        // manage physics material to prevent sticking
        if (preventWallSticking)
        {
            if (!isGrounded)
            {
                // when in air, use frictionless material
                col.sharedMaterial = frictionMaterial;
                
                // if sliding down wall, ensure velocity is not artificially limited
                if (isSlidingDownWall)
                {
                    float maxSlideSpeed = 15f;
                    if (isGravityReversed)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, maxSlideSpeed));
                    }
                    else
                    {
                        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxSlideSpeed));
                    }
                }
            }
        }
    }
    
    public void ResetGravity()
    {
        // Only reset if gravity is currently reversed
        if (isGravityReversed)
        {
            isGravityReversed = false;
            FlipPlayerVertically(); // Flip the player back to normal orientation
        
            // Reset vertical velocity to prevent any unexpected movement
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }
    
    // uncomment this function for debug stuff in scene view
    private void OnDrawGizmos()
    {
        // draw ground check gizmo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        
        // draw wall check gizmos
        if (col != null)
        {
            float colliderHeight = GetComponent<BoxCollider2D>().bounds.size.y;
            float colliderWidth = GetComponent<BoxCollider2D>().bounds.size.x;
            Gizmos.color = isTouchingWall ? Color.yellow : Color.white;
            
            int rayCount = numberOfWallChecks > 0 ? numberOfWallChecks : 3;
            for (int i = 0; i < rayCount; i++)
            {
                float heightFraction = (float)i / (rayCount - 1);
                float yOffset = Mathf.Lerp(-colliderHeight / 2 + 0.1f, colliderHeight / 2 - 0.1f, heightFraction);
                Vector2 origin = new Vector2(transform.position.x, transform.position.y + yOffset);
                
                Gizmos.DrawRay(origin, Vector2.right * (colliderWidth / 2 + wallCheckDistance));
                Gizmos.DrawRay(origin, Vector2.left * (colliderWidth / 2 + wallCheckDistance));
            }
        }
    }
}
