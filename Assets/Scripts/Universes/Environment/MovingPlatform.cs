using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
     public enum MovementType
    {
        Horizontal,
        Vertical
    }

    public enum MovementBehavior
    {
        PingPong, // move back and forth
        Loop,     // teleport back to start when reaching end
        OneWay    // move only in one direction, then stop
    }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.Horizontal;
    public MovementBehavior movementBehavior = MovementBehavior.PingPong;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public float waitTimeAtEndpoints = 0.5f;

    [Header("Optional Settings")]
    public bool startAtEndpoint = false;
    public bool moveOnStart = true;
    public bool smoothMovement = true;
    
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 targetPosition;
    private bool movingToEnd = true;
    private bool isWaiting = false;
    private bool playerOnPlatform = false;
    private Transform playerTransform = null;
    private bool needToUnparent = false;

    private void Start()
    {
        // store the starting position
        startPosition = transform.position;
        
        // calculate the end position based on movement type
        if (movementType == MovementType.Horizontal)
        {
            endPosition = startPosition + new Vector3(moveDistance, 0f, 0f);
        }
        else
        {
            endPosition = startPosition + new Vector3(0f, moveDistance, 0f);
        }

        // if we're starting at the endpoint, swap positions
        if (startAtEndpoint)
        {
            Vector3 temp = startPosition;
            startPosition = endPosition;
            endPosition = temp;
            transform.position = startPosition;
            movingToEnd = true;
        }

        // set initial target
        targetPosition = movingToEnd ? endPosition : startPosition;
    }

    private void Update()
    {
        // handle safe unparenting
        if (needToUnparent && playerTransform != null)
        {
            playerTransform.SetParent(null);
            needToUnparent = false;
            playerTransform = null;
        }
        
        // don't move if waiting at an endpoint
        if (isWaiting)
            return;
            
        // don't move if moveOnStart is false and player is not on the platform
        if (!moveOnStart && !playerOnPlatform)
            return;

        // move towards the target
        if (smoothMovement)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            // calculate movement direction
            Vector3 direction = (targetPosition - transform.position).normalized;
            // apply movement
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            if (movementType == MovementType.Horizontal)
            {
                float xPos = transform.position.x;
                float targetX = targetPosition.x;
                
                if ((movingToEnd && xPos > targetX) || (!movingToEnd && xPos < targetX))
                {
                    transform.position = targetPosition;
                }
            }
            else
            {
                float yPos = transform.position.y;
                float targetY = targetPosition.y;
                
                if ((movingToEnd && yPos > targetY) || (!movingToEnd && yPos < targetY))
                {
                    transform.position = targetPosition;
                }
            }
        }

        // check if we've reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            HandleEndpointReached();
        }
    }

    private void HandleEndpointReached()
    {
        switch (movementBehavior)
        {
            case MovementBehavior.PingPong:
                // handle waiting at the endpoint
                if (waitTimeAtEndpoints > 0)
                {
                    StartCoroutine(WaitAtEndpoint());
                }
                else
                {
                    // switch direction
                    movingToEnd = !movingToEnd;
                    targetPosition = movingToEnd ? endPosition : startPosition;
                }
                break;

            case MovementBehavior.Loop:
                // when reaching the end, go back to start
                if (movingToEnd)
                {
                    if (waitTimeAtEndpoints > 0)
                    {
                        StartCoroutine(WaitAtEndpoint());
                    }
                    else
                    {
                        transform.position = startPosition;
                        targetPosition = endPosition;
                    }
                }
                break;

            case MovementBehavior.OneWay:
                // stop moving when we reach the end
                enabled = false;
                break;
        }
    }

    private IEnumerator WaitAtEndpoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtEndpoints);
        isWaiting = false;

        if (movementBehavior == MovementBehavior.PingPong)
        {
            // switch direction
            movingToEnd = !movingToEnd;
            targetPosition = movingToEnd ? endPosition : startPosition;
        }
        else if (movementBehavior == MovementBehavior.Loop)
        {
            // teleport back to start
            transform.position = startPosition;
            targetPosition = endPosition;
        }
    }

    // this function allows the platform to carry the player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is from above (player landing on platform)
        if (collision.gameObject.CompareTag("Player"))
        {
            // detect if player is above the platform (y-position check)
            float playerBottom = collision.collider.bounds.min.y;
            float platformTop = GetComponent<Collider2D>().bounds.max.y;
            
            if (playerBottom >= platformTop - 0.1f) // small tolerance for collision detection
            {
                collision.transform.SetParent(transform);
                playerTransform = collision.transform;
                playerOnPlatform = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // when player leaves the platform, unparent them
        if (collision.gameObject.CompareTag("Player"))
        {
            needToUnparent = true;
            playerOnPlatform = false;
        }
    }

    // uncomment this function for debug stuff in scene view
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end;

        if (movementType == MovementType.Horizontal)
        {
            end = start + new Vector3(moveDistance, 0f, 0f);
        }
        else
        {
            end = start + new Vector3(0f, moveDistance, 0f);
        }

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.2f);
    }

    // public methods to control the platform externally
    public void StartMoving()
    {
        enabled = true;
    }

    public void StopMoving()
    {
        enabled = false;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        movingToEnd = true;
        targetPosition = endPosition;
    }
}
