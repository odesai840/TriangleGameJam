using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCircle : MonoBehaviour
{
    [Header("Circle Settings")]
    [Tooltip("Number of segments around the circle. Increase for smoother detail.")]
    public int segments = 32;
    [Tooltip("Radius of the circle.")]
    public float radius = 1f;

    public string ChunkCoordDebug;

    public void Init(float r)
    {
        radius = r;
        segments = numSegments(r);
        GetComponent<CircleCollider2D>().radius = r;

        // Generate the circle mesh and assign it to the MeshFilter.
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = GenerateCircleMesh(segments, radius);
        mf.mesh = mesh;

        StartCoroutine(CheckOverlap());
    }

    int numSegments(float r)
    {
        return (int)((2f * Mathf.PI * r) / 0.196f);
    }

    Mesh GenerateCircleMesh(int segments, float radius)
    {
        Mesh mesh = new Mesh();

        // +1 for the center vertex.
        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uvs = new Vector2[segments + 1];
        int[] triangles = new int[segments * 3];

        // Center vertex at (0, 0, 0)
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, y, 0f);

            // Map x,y from [-radius, radius] to [0,1] UV space.
            uvs[i + 1] = new Vector2((x / (radius * 2)) + 0.5f, (y / (radius * 2)) + 0.5f);
        }

        // Create triangle fan: center vertex and two consecutive outer vertices form each triangle.
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;           // Center vertex.
            triangles[i * 3 + 1] = i + 1;     // Current outer vertex.
            // If we're at the last vertex, wrap around to vertex 1.
            triangles[i * 3 + 2] = (i < segments - 1) ? i + 2 : 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // Coroutine to check for overlapping planets after initialization.
    IEnumerator CheckOverlap()
    {
        // Wait one frame to allow physics to update.
        //yield return null;

        bool shouldDelete = false;

        // Use OverlapCircleAll to check for colliders overlapping this circle.
        // We assume the circle's center is at transform.position and use the same radius.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            // Ignore self.
            if (hit.gameObject == gameObject)
                continue;

            // Optionally, check that the hit object is tagged as "Planet".
            if (hit.gameObject.CompareTag("Universe"))
            {
                // If any other planet is overlapping, delete this planet.
                shouldDelete = true;
                break;
            }
        }

        if (shouldDelete)
        {
            yield return null;
            print("overlapped");
            Destroy(gameObject);
        }
    }
}
