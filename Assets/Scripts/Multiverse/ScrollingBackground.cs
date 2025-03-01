using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("How strongly this layer parallax-scales camera movement.")]
    public float parallaxFactor = 0.5f;

    [Tooltip("If true, only scroll horizontally.")]
    public bool lockVertical = false;

    [Tooltip("Scales offset per-axis. If your texture is 512x256, set (512,256).")]
    public Vector2 textureScale = new Vector2(480f, 270f);

    private Renderer rend;
    private Transform mainCam;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mainCam = Camera.main.transform;

        // Alternatively, if your texture is assigned at runtime, you could do:
        // var tex = rend.material.mainTexture;
        // textureScale = new Vector2(tex.width, tex.height);
    }

    void LateUpdate()
    {
        // Get the camera position in 2D
        Vector2 camPos = mainCam.position;

        // Scale by parallax factor
        float offsetX = camPos.x * parallaxFactor;
        float offsetY = camPos.y * parallaxFactor;

        // If you only want horizontal
        if (lockVertical) offsetY = 0f;

        // Adjust offsets by textureScale to keep uniform speed in X/Y
        // i.e. if texture is wider than tall, we'll move the X offset more slowly
        offsetX /= textureScale.x;
        offsetY /= textureScale.y;

        // Use modulo 1 to wrap in [0..1] range (prevent large offset issues)
        offsetX %= 1f;
        if (offsetX < 0) offsetX += 1f; // ensure positive
        offsetY %= 1f;
        if (offsetY < 0) offsetY += 1f;

        // Set the material’s main texture offset
        rend.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}
