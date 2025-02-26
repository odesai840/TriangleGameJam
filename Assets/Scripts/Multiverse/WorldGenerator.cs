using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Chunk & World Settings")]
    public int chunkSize = 100;           // World units per chunk (square chunks)
    public int viewDistanceInChunks = 2;  // How many chunks away from the player's chunk to load

    [Header("Planet Generation Settings")]
    public float minPlanetRadius = 1f;    // Minimum planet radius
    public float maxPlanetRadius = 5f;    // Maximum planet radius
    public int maxPlanetsPerChunk = 3;    // Maximum number of planets per chunk
    public int maxPlacementAttempts = 10; // How many times to try placing a planet before giving up

    [Header("Planet Prefab")]
    public GameObject planetPrefab;       // Prefab for the planet (should be a circle)

    // Dictionary to track generated chunks. Key is the chunk coordinate; Value is list of planet GameObjects in that chunk.
    private Dictionary<Vector2Int, List<GameObject>> loadedChunks = new Dictionary<Vector2Int, List<GameObject>>();

    private void Start()
    {
        //put this in main menu
        GameSettings.InitializeSeed();
    }

    void Update()
    {
        // Determine the player's current chunk.
        Vector2 playerPos = GetPlayerPosition();
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.y / chunkSize)
        );

        // Build a set of chunk coordinates that should be active.
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int x = playerChunk.x - viewDistanceInChunks; x <= playerChunk.x + viewDistanceInChunks; x++)
        {
            for (int y = playerChunk.y - viewDistanceInChunks; y <= playerChunk.y + viewDistanceInChunks; y++)
            {
                neededChunks.Add(new Vector2Int(x, y));
            }
        }

        // Remove chunks that are no longer needed.
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in loadedChunks.Keys)
        {
            if (!neededChunks.Contains(chunk))
            {
                chunksToRemove.Add(chunk);
            }
        }
        foreach (var chunk in chunksToRemove)
        {
            foreach (GameObject planet in loadedChunks[chunk])
            {
                Destroy(planet);
            }
            loadedChunks.Remove(chunk);
        }

        // Generate and load chunks that are needed but not yet loaded.
        foreach (Vector2Int chunk in neededChunks)
        {
            if (!loadedChunks.ContainsKey(chunk))
            {
                List<PlanetData> planetDatas = GeneratePlanetsForChunk(chunk);
                List<GameObject> planetObjects = InstantiatePlanets(planetDatas);
                loadedChunks.Add(chunk, planetObjects);
            }
        }
    }

    // Returns the player's position. Assumes a GameObject tagged "Player" exists.
    Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.Find("Player");
        return (player != null) ? player.transform.position : Vector2.zero;
    }

    // Deterministically generate a list of PlanetData for a given chunk.
    List<PlanetData> GeneratePlanetsForChunk(Vector2Int chunkCoord)
    {
        List<PlanetData> planets = new List<PlanetData>();

        // Create a seed based solely on the chunk coordinates.
        int seed = GameSettings.GlobalSeed ^ (chunkCoord.x * 73856093) ^ (chunkCoord.y * 19349663);
        System.Random rand = new System.Random(seed);

        int planetCount = rand.Next(0, maxPlanetsPerChunk + 1);

        for (int i = 0; i < planetCount; i++)
        {
            bool placed = false;
            int attempts = 0;
            while (!placed && attempts < maxPlacementAttempts)
            {
                // Random radius between min and max.
                float radius = Mathf.Lerp(minPlanetRadius, maxPlanetRadius, (float)rand.NextDouble());
                // Random position within the chunk.
                float posX = (float)rand.NextDouble() * chunkSize;
                float posY = (float)rand.NextDouble() * chunkSize;
                Vector2 candidatePos = new Vector2(chunkCoord.x * chunkSize + posX, chunkCoord.y * chunkSize + posY);

                // Check against already placed planets in this chunk.
                bool overlaps = false;
                foreach (PlanetData p in planets)
                {
                    if (Vector2.Distance(p.position, candidatePos) < (p.radius + radius))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    planets.Add(new PlanetData(candidatePos, radius, chunkCoord));
                    placed = true;
                }
                attempts++;
            }
        }
        return planets;
    }

    // Instantiate planet GameObjects from a list of PlanetData.
    List<GameObject> InstantiatePlanets(List<PlanetData> planetDatas)
    {
        List<GameObject> planetObjects = new List<GameObject>();
        foreach (PlanetData data in planetDatas)
        {
            // Instantiate the planet prefab at the specified position.
            GameObject planet = Instantiate(planetPrefab, data.position, Quaternion.identity);
            // Assuming the prefab is a unit circle, scale it according to the planet's radius.
            //planet.transform.localScale = Vector3.one * data.radius * 2f;
            planet.GetComponent<ProceduralCircle>().Init(data.radius);
            planet.GetComponent<ProceduralCircle>().ChunkCoordDebug = data.chunkCoord.ToString();
            planetObjects.Add(planet);
        }
        return planetObjects;
    }
}

// Simple class to store planet data.
public class PlanetData
{
    public Vector2 position;
    public float radius;
    public Vector2Int chunkCoord; //still need to delete

    public PlanetData(Vector2 pos, float r, Vector2Int coord)
    {
        chunkCoord = coord;
        position = pos;
        radius = r;
    }
}
