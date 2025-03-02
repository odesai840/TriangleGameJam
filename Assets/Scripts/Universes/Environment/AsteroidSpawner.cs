using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float spawnIntervalMin = 1.0f;
    [SerializeField] private float spawnIntervalMax = 3.0f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
        Instantiate(asteroidPrefab, transform.position, Quaternion.Euler(0, 0, 180));
        StartCoroutine("SpawnRoutine");
    }
}
