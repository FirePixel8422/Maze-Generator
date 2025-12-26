using UnityEngine;

public static class QuadMesh
{
    private static Mesh _mesh;

    public static ref readonly Mesh Instance => ref _mesh;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        _mesh = GenerateQuad();
    }

    private static Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Quad_1x1_Up";

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, 0f, -0.5f), // bottom-left
            new Vector3(0.5f, 0f, -0.5f),  // bottom-right
            new Vector3(-0.5f, 0f, 0.5f),  // top-left
            new Vector3(0.5f, 0f, 0.5f)    // top-right
        };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
