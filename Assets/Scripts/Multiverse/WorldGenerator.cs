using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance; // Singleton reference

    [Header("Chunk & World Settings")]
    public int chunkSize = 100;
    public int viewDistanceInChunks = 2;

    [Header("Planet Generation Settings")]
    public float minPlanetRadius = 1f;
    public float maxPlanetRadius = 5f;
    public int maxPlanetsPerChunk = 3;
    public int maxPlacementAttempts = 2;
    public int planetsPerFrame = 2;  // How many planets to spawn per frame
    public int homeDistance = 3;

    public float noForegroundRadius;

    public Vector2 homePos;

    public Texture[] universeColors;
    public bool isBackground;

    [Header("Planet Prefab")]
    public GameObject planetPrefab;

    // Tracks the loaded chunk GameObjects (planets, residues, etc.)
    private Dictionary<Vector2Int, List<GameObject>> loadedChunks = new Dictionary<Vector2Int, List<GameObject>>();
    // Tracks active coroutines that are loading planets for specific chunks
    private Dictionary<Vector2Int, Coroutine> activeChunkCoroutines = new Dictionary<Vector2Int, Coroutine>();

    private void Awake()
    {
        Instance = this;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Background"), true);
    }

    private void Start()
    {
        GameSettings.Initialize(); //do this in main menu

        if (! isBackground)
            PlaceHome();
    }

    Vector2 GetRandomPointOnCircle(float r, System.Random rand)
    {
        float angle = GetRandomFloat(rand, 0f, Mathf.PI * 2); // Random angle in radians
        float x = r * Mathf.Cos(angle);
        float y = r * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    void Update()
    {
        Vector2 playerPos = GetPlayerPosition();
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.y / chunkSize)
        );

        // Which chunks should be active based on the player's position?
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int x = playerChunk.x - viewDistanceInChunks; x <= playerChunk.x + viewDistanceInChunks; x++)
        {
            for (int y = playerChunk.y - viewDistanceInChunks; y <= playerChunk.y + viewDistanceInChunks; y++)
            {
                neededChunks.Add(new Vector2Int(x, y));
            }
        }

        // Remove chunks we no longer need
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
            // Safely stop the loading coroutine if it exists (and is non-null)
            if (activeChunkCoroutines.TryGetValue(chunk, out Coroutine routine))
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
                activeChunkCoroutines.Remove(chunk);
            }

            // Destroy all planets in this chunk
            if (loadedChunks.ContainsKey(chunk))
            {
                foreach (GameObject planet in loadedChunks[chunk])
                {
                    Destroy(planet);
                }
                loadedChunks.Remove(chunk);
            }
        }


        // Generate and load chunks we need but haven't yet loaded
        foreach (Vector2Int chunk in neededChunks)
        {
            if (!loadedChunks.ContainsKey(chunk))
            {
                // Create planet data deterministically
                List<PlanetData> planetDatas = GeneratePlanetsForChunk(chunk);
                // Asynchronously load the planets
                LoadPlanetsAsync(planetDatas, chunk);
            }
        }
    }

    // Public method to start a coroutine for loading a chunk's planets
    public void LoadPlanetsAsync(List<PlanetData> planetDatas, Vector2Int chunkCoord)
    {
        // If we're already loading this chunk, do nothing
        if (activeChunkCoroutines.ContainsKey(chunkCoord))
            return;

        // Start the coroutine and store it in our dictionary
        Coroutine loadCoro = StartCoroutine(InstantiatePlanetsCoroutine(planetDatas, chunkCoord));
        activeChunkCoroutines[chunkCoord] = loadCoro;
    }

    // The actual coroutine that spawns planets gradually
    private IEnumerator InstantiatePlanetsCoroutine(List<PlanetData> planetDatas, Vector2Int chunkCoord)
    {
        // Ensure we have a list ready for this chunk
        loadedChunks[chunkCoord] = new List<GameObject>();

        for (int i = 0; i < planetDatas.Count; i++)
        {
            // If the chunk was removed while we're loading, stop immediately
            if (!loadedChunks.ContainsKey(chunkCoord))
            {
                activeChunkCoroutines.Remove(chunkCoord);
                yield break;
            }

            PlanetData data = planetDatas[i];

            GameObject planet = InstantiatePlanet(data);

            // Keep track of this planet
            loadedChunks[chunkCoord].Add(planet);

            // Spread out spawning: only spawn "planetsPerFrame" each frame
            if ((i + 1) % planetsPerFrame == 0)
            {
                yield return null;
            }
        }

        // Once we're done, remove from active coroutines
        activeChunkCoroutines.Remove(chunkCoord);
    }

    GameObject InstantiatePlanet(PlanetData data)
    {
        // Instantiate planet prefab
        GameObject planet = Instantiate(planetPrefab, data.position, Quaternion.identity);
        if (isBackground)
        {
            planet.layer = LayerMask.NameToLayer("Background");
            planet.transform.position = new Vector3(planet.transform.position.x, planet.transform.position.y, 3);
        } else
        {
            planet.layer = LayerMask.NameToLayer("Default");
        }
        // Example init: radius, chunkCoord, etc.
        planet.GetComponent<ProceduralCircle>().Init(data.radius, data.universeType);
        planet.GetComponent<BubbleDeform>().Init(isBackground);
        planet.GetComponent<ProceduralCircle>().chunkCoord = data.chunkCoord;
        planet.GetComponent<ProceduralCircle>().rotation = data.rot;
        return planet;
    }

    void PlaceHome()
    {
        System.Random rand = new System.Random(GameSettings.GlobalSeed);
        PlanetData home = new PlanetData(GetRandomPointOnCircle(homeDistance, rand), maxPlanetRadius, new Vector2Int(), 4, GetRandomFloat(rand, -.01f,.01f));
        GameObject homeObject = InstantiatePlanet(home);
        homePos = homeObject.transform.position;
    }

    public void RegisterResidueInChunk(Vector2Int chunk, GameObject residue)
    {
        if (!loadedChunks.ContainsKey(chunk))
        {
            loadedChunks[chunk] = new List<GameObject>();
        }
        loadedChunks[chunk].Add(residue);
    }

    float GetRandomFloat(System.Random rand, float min, float max)
    {
        return (float)(rand.NextDouble() * (max - min) + min);
    }

    // Find the player's position
    Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.Find("Player");
        return (player != null) ? player.transform.position : Vector2.zero;
    }

    // Deterministically generate data for a given chunk
    List<PlanetData> GeneratePlanetsForChunk(Vector2Int chunkCoord)
    {
        List<PlanetData> planets = new List<PlanetData>();

        int seed = GameSettings.GlobalSeed ^ (chunkCoord.x * 73856093) ^ (chunkCoord.y * 19349663) ^ (isBackground ? 83492791 : 0);
        System.Random rand = new System.Random(seed);

        int planetCount = rand.Next(0, maxPlanetsPerChunk + 1);

        for (int i = 0; i < planetCount; i++)
        {
            bool placed = false;
            int attempts = 0;
            while (!placed && attempts < maxPlacementAttempts)
            {
                int universeType = rand.Next(0, 3);
                if (GameSettings.gotUniverse[universeType])
                    universeType = 3;
                float rotation = GetRandomFloat(rand, -.01f, .01f);
                float radius = Mathf.Lerp(minPlanetRadius, maxPlanetRadius, (float)rand.NextDouble());
                float posX = (float)rand.NextDouble() * chunkSize;
                float posY = (float)rand.NextDouble() * chunkSize;
                Vector2 candidatePos = new Vector2(
                    chunkCoord.x * chunkSize + posX,
                    chunkCoord.y * chunkSize + posY
                );

                // Basic overlap check among planets in this chunk
                bool overlaps = false;

                // 1) If it's foreground (isBackground == false),
                //    skip if inside the "noForegroundRadius" around (0,0).
                if (!isBackground)
                {
                    // Check if the candidate center is too close to origin
                    float distFromOrigin = candidatePos.magnitude;
                    if (distFromOrigin < (noForegroundRadius + radius))
                    {
                        overlaps = true;
                    }
                }

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
                    planets.Add(new PlanetData(candidatePos, radius, chunkCoord, universeType, rotation));
                    placed = true;
                }
                attempts++;
            }
        }
        return planets;
    }
}

// Simple data class for planet info
public class PlanetData
{
    public Vector2 position;
    public float radius;
    public Vector2Int chunkCoord;
    public int universeType;
    public float rot;

    public PlanetData(Vector2 pos, float r, Vector2Int coord, int type, float rotation)
    {
        position = pos;
        radius = r;
        chunkCoord = coord;
        universeType = type;
        rot = rotation;
    }
}
