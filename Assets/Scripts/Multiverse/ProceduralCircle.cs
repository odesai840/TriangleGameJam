using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCircle : MonoBehaviour
{
    [Header("Circle Settings")]
    [Tooltip("Number of segments around the circle. Increase for smoother detail.")]
    public int segments = 64;
    [Tooltip("Radius of the circle.")]
    public float radius = 1f;

    void Start()
    {
        // Generate the circle mesh and assign it to the MeshFilter.
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = GenerateCircleMesh(segments, radius);
        mf.mesh = mesh;
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
}
