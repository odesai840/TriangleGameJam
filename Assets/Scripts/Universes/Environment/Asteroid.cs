using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Asteroid : MonoBehaviour
{
    [SerializeField] float lifeTime = 5f;
    [SerializeField] private float minGravityScale = 1.0f;
    [SerializeField] private float maxGravityScale = 3.0f;
    [SerializeField] private float minScaleFactor = 1.0f;
    [SerializeField] private float maxScaleFactor = 3.0f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.transform.localScale *= Random.Range(minScaleFactor, maxScaleFactor);
        rb.gravityScale = Random.Range(minGravityScale, maxGravityScale);
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
