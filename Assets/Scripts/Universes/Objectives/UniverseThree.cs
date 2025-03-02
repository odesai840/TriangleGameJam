using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniverseThree : MonoBehaviour
{
    [Header("Screen Dissolve")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        if (collision.gameObject.CompareTag("Player"))
        {
            GameSettings.LevelWon(3);
            StartCoroutine(FadeOutLevel());
        }
    }
    
    private IEnumerator FadeOutLevel()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            
            float currentValue = Mathf.Lerp(1f, 0f, curveValue);
            
            ScreenDissolveFeature.Instance.progress = currentValue;
            
            yield return null;
        }
        
        ScreenDissolveFeature.Instance.progress = 0f;
        SceneManager.LoadScene("Multiverse");
    }
}
