using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Timing Settings")]
    public float visibleDuration = 2.0f;
    public float invisibleDuration = 2.0f;
    public float initialDelay = 0f;

    [Header("Visual Warning")]
    public bool flashBeforeDisappearing = true;
    public float warningTime = 0.5f;
    public float flashRate = 10f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        
        // store original color for flashing
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // start the cycle coroutine
        StartCoroutine(PlatformCycle());
    }

    IEnumerator PlatformCycle()
    {
        // handle initial delay
        if (initialDelay > 0)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        while (true)
        {
            // make the platform visible
            SetPlatformVisible(true);
            
            // wait for some time
            if (flashBeforeDisappearing && warningTime < visibleDuration)
            {
                yield return new WaitForSeconds(visibleDuration - warningTime);
                
                // flash as warning
                yield return StartCoroutine(FlashPlatform());
            }
            else
            {
                // no flashing, just wait the full duration
                yield return new WaitForSeconds(visibleDuration);
            }
            
            // make the platform invisible
            SetPlatformVisible(false);
            
            // wait for some time
            yield return new WaitForSeconds(invisibleDuration);
        }
    }

    IEnumerator FlashPlatform()
    {
        if (spriteRenderer == null) yield break;

        float endTime = Time.time + warningTime;
        bool isVisible = true;
        float flashInterval = 1f / flashRate;

        // flash until warning time is over
        while (Time.time < endTime)
        {
            // toggle visibility
            isVisible = !isVisible;
            
            // change color based on visibility
            spriteRenderer.color = isVisible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            
            // wait for next flash
            yield return new WaitForSeconds(flashInterval);
        }
        
        // reset to original color
        spriteRenderer.color = originalColor;
    }

    void SetPlatformVisible(bool visible)
    {
        // enable/disable sprite renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
        
        // enable/disable collider
        if (platformCollider != null)
        {
            platformCollider.enabled = visible;
        }
    }
}
