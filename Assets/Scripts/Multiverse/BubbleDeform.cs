using UnityEngine;

public class BubbleDeform : MonoBehaviour
{
    public Material bubbleMaterial;      // Material assigned in the Inspector.
    public float maxImpactStrength = 0.1f; // Maximum inward deformation.
    public float maxImpactWiggle = 0.05f;  // Maximum extra wiggle amount upon impact.
    public float recoverySpeed = 1f;       // Speed at which both effects decay.

    private float currentImpactStrength = 0f;
    private float currentImpactWiggle = 0f;

    void Start()
    {
        // Create a unique instance of the material for this bubble.
        bubbleMaterial = Instantiate(bubbleMaterial);
        // Assign the new instance to the SpriteRenderer.
        GetComponent<MeshRenderer>().material = bubbleMaterial;

        // Ensure initial impact values are zero.
        currentImpactStrength = 0f;
        currentImpactWiggle = 0f;
        bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
        bubbleMaterial.SetFloat("_ImpactWiggleAmount", currentImpactWiggle);

        bubbleMaterial.mainTexture = WorldGenerator.Instance.universeColors[GetComponent<ProceduralCircle>().universeType];
    }

    void Update()
    {
        // Gradually decay the impact effects over time.
        if (currentImpactStrength > 0)
        {
            currentImpactStrength = Mathf.Lerp(currentImpactStrength, 0f, Time.deltaTime * recoverySpeed);
            bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
        }
        if (currentImpactWiggle > 0)
        {
            currentImpactWiggle = Mathf.Lerp(currentImpactWiggle, 0f, Time.deltaTime * recoverySpeed);
            bubbleMaterial.SetFloat("_ImpactWiggleAmount", currentImpactWiggle);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Get the collision contact point.
        Vector2 contactPoint = collision.GetContact(0).point;
        // Convert the contact point from world space to local space.
        Vector2 localContact = transform.InverseTransformPoint(contactPoint);
        // Pass the local collision point to the shader.
        bubbleMaterial.SetVector("_ImpactPoint", new Vector4(localContact.x, localContact.y, 0, 0));

        // Set the maximum values for impact strength and extra wiggle.
        currentImpactStrength = maxImpactStrength;
        currentImpactWiggle = maxImpactWiggle;
        bubbleMaterial.SetFloat("_ImpactStrength", currentImpactStrength);
        bubbleMaterial.SetFloat("_ImpactWiggleAmount", currentImpactWiggle);
    }
}
