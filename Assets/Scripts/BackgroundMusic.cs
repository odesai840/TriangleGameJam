using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField]
    public AudioClip musicClip; // Assign your music file in the Inspector
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;  // Enables looping
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f; // Adjust volume as needed
        audioSource.Play();
    }
}
