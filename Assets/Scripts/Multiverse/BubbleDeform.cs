using UnityEngine;

public class BubbleDeform : MonoBehaviour
{
    public Material bubbleMaterial;      // Material using the wrinkly bubble shader.
    public float maxImpactStrength = 0.1f; // The maximum deformation strength upon collision.
    public float recoverySpeed = 1f;     // How quickly the impact effect decays.

    private float currentImpactStrength = 0f;

    void Start()
    {
        // Ensure initial impact strength is zero.
        currentImpactStrength = 0f;
        bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
    }

    void Update()
    {
        // Gradually reduce the impact strength to zero over time.
        if (currentImpactStrength > 0)
        {
            currentImpactStrength = Mathf.Lerp(currentImpactStrength, 0f, Time.deltaTime * recoverySpeed);
            bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Get the first collision contact point.
        Vector2 contactPoint = collision.GetContact(0).point;
        // Convert the contact point from world space to the bubble's local space.
        Vector2 localContact = transform.InverseTransformPoint(contactPoint);

        // Pass the local collision point to the shader.
        bubbleMaterial.SetVector("_ImpactPoint", new Vector4(localContact.x, localContact.y, 0, 0));

        // Set the current impact strength to maximum.
        currentImpactStrength = maxImpactStrength;
        bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
    }
}
