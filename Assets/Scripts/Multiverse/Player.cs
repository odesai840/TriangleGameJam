using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float rotationSpeed = 180f;   // degrees per second
    public float acceleration = 5f;      // force for forward movement
    public float deceleration = 5f;      // force for deceleration

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Usually for top-down, we don't want gravity
        rb.gravityScale = 0f;
        // Optionally set constraints if needed:
        // rb.constraints = RigidbodyConstraints2D.FreezeRotation; // if you want to handle rotation manually
    }

    void Update()
    {
        // ROTATION
        // Left arrow => rotate left, Right arrow => rotate right
        float rotation = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation = rotationSpeed;  // rotate left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation = -rotationSpeed; // rotate right
        }

        // Apply rotation in degrees per second
        // (We'll do it in Update, then apply in FixedUpdate for precise physics)
        // For a simple approach, we can do rotation in Update if the object is not moving extremely fast
        float rotThisFrame = rotation * Time.deltaTime;
        transform.Rotate(0, 0, rotThisFrame);

        // ACCELERATION & DECELERATION
        // Up => accelerate forward, Down => decelerate
        // We'll store that as well, then apply in FixedUpdate
    }

    void FixedUpdate()
    {
        // For forward direction, we use transform.up in 2D
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rb.velocity += (Vector2)transform.up * (acceleration * Time.fixedDeltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            rb.velocity -= (Vector2)transform.up * (deceleration * Time.fixedDeltaTime);
        }
    }
}
