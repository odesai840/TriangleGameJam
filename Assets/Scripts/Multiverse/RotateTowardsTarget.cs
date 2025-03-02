using UnityEngine;

public class RotateTowardsTarget : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 200f; // Adjust rotation speed

    private void Start()
    {
        if (!GameSettings.GameWon())
            Destroy(this.gameObject);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Get direction to target
        Vector2 direction = target.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        targetAngle -= 90;

        // Smoothly rotate toward target
        float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}