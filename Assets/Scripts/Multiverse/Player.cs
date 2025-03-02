using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPhysicsController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float rotationAccel = 50f;   // How quickly you accelerate angular velocity (left/right).
    public float rotationDrag = 10f;    // Angular drag factor (slows rotation).
    public float maxAngularSpeed = 180f; // Degrees/sec, clamp for stability.

    public float forwardAccel = 5f;     // Forward linear acceleration
    public float backwardAccel = 3f;    // Backward linear acceleration
    public float linearDrag = 1f;       // Drag that slows velocity
    public float maxSpeed = 10f;        // Limit the linear speed

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // top-down, no gravity
        // We can also handle drag manually, so you might set:
        // rb.drag = 0f; 
        // rb.angularDrag = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Universe")
        {
            print("UNIVERSE");
        }
    }

    void FixedUpdate()
    {
        // 1) READ INPUT
        // Arrow keys
        bool leftArrow = Input.GetKey(KeyCode.LeftArrow);
        bool rightArrow = Input.GetKey(KeyCode.RightArrow);
        bool upArrow = Input.GetKey(KeyCode.UpArrow);
        bool downArrow = Input.GetKey(KeyCode.DownArrow);
        
        // WASD keys
        bool aKey = Input.GetKey(KeyCode.A);
        bool dKey = Input.GetKey(KeyCode.D);
        bool wKey = Input.GetKey(KeyCode.W);
        bool sKey = Input.GetKey(KeyCode.S);

        // Combine inputs from both control schemes
        bool left = leftArrow || aKey;
        bool right = rightArrow || dKey;
        bool forward = upArrow || wKey;
        bool backward = downArrow || sKey;

        // 2) ANGULAR VELOCITY
        float turnInput = 0f;
        if (left) turnInput += 1f;
        if (right) turnInput -= 1f;

        // Apply an angular acceleration in deg/sec^2
        // Then we convert to deg/sec
        float currentAngVel = rb.angularVelocity; // in deg/sec

        // accelerate
        currentAngVel += turnInput * rotationAccel * Time.fixedDeltaTime;

        // apply some rotation drag
        if (Mathf.Abs(currentAngVel) > 0.01f)
        {
            float dragSign = (currentAngVel > 0) ? -1f : 1f;
            currentAngVel += dragSign * rotationDrag * Time.fixedDeltaTime;
            // This automatically pulls angular velocity toward 0
            // If we want no auto-slow, set rotationDrag = 0
        }

        // clamp to a max
        currentAngVel = Mathf.Clamp(currentAngVel, -maxAngularSpeed, maxAngularSpeed);

        // assign back
        rb.angularVelocity = currentAngVel;

        // 3) LINEAR VELOCITY
        Vector2 velocity = rb.velocity;

        // forward direction is transform.up in 2D
        if (forward)
        {
            velocity += (Vector2)transform.up * (forwardAccel * Time.fixedDeltaTime);
        }
        else if (backward)
        {
            velocity -= (Vector2)transform.up * (backwardAccel * Time.fixedDeltaTime);
        }

        // apply linear drag
        if (velocity.magnitude > 0.01f)
        {
            Vector2 dragDir = -velocity.normalized;
            velocity += dragDir * linearDrag * Time.fixedDeltaTime;
            // Pulls velocity toward zero over time
            // If you want to rely on rb.drag, you can set that instead
        }

        // clamp speed
        float speed = velocity.magnitude;
        if (speed > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        // set final
        rb.velocity = velocity;
    }

    void Update()
    {
        // Because rotation is handled by angularVelocity,
        // we do NOT set transform.rotation in Update.
        // The Rigidbody2D updates the object's rotation automatically
        // if you haven't frozen Z rotation in constraints.
    }
}
