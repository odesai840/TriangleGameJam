using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public float targetProgress = 1f;
    public float lerpSpeed = 10f;

    void Update()
    {
        if (ScreenDissolveFeature.Instance != null)
        {
            // Lerp the dissolve progress toward the target
            ScreenDissolveFeature.Instance.progress = Mathf.Lerp(ScreenDissolveFeature.Instance.progress, targetProgress, Time.deltaTime * lerpSpeed);
        }
    }
}
