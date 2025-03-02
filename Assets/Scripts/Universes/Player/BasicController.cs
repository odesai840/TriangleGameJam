using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
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
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
            isJumping = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
        
        // variable jump height
        if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) && rb.velocity.y > 0)
        {
            jumpInputReleased = true;
        }
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
        // check if player is grounded
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        
        // reset jump state when landing
        if (isGrounded && rb.velocity.y <= 0)
        {
            isJumping = false;
            jumpInputReleased = false;
        }
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
        if (jumpInputReleased && rb.velocity.y > 0)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
            jumpInputReleased = false;
        }
        
        // apply increased gravity when falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
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
        isSlidingDownWall = isTouchingWall && !isGrounded && rb.velocity.y < 0;
        
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
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -15f));
                }
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