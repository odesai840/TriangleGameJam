using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLives : MonoBehaviour
{
    [SerializeField] private float maxLives = 3.0f;

    private GravityController gravityController;
    private Collider2D col;
    private Vector3 playerStartPos;
    private float currentLives;
    
    void Start()
    {
        gameObject.TryGetComponent<GravityController>(out gravityController);
        playerStartPos = transform.position;
        currentLives = maxLives;
        Debug.Log("Current Lives: " + currentLives);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Hazard"))
        {
            currentLives--;
            if (gravityController != null)
            {
                gravityController.ResetGravity();
            }
            transform.position = playerStartPos;
            Debug.Log("Current Lives: " + currentLives);
            if (currentLives <= 0)
            {
                SceneManager.LoadScene("Multiverse");
            }
        }
    }

    // public function to get current lives
    public float GetCurrentLives()
    {
        return currentLives;
    }
}
