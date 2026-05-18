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

        Vector3[] vertices = new Vector3[6]
        {
            // Triangle 1
            new Vector3(-0.5f, 0f, -0.5f), // bottom-left
            new Vector3(-0.5f, 0f,  0.5f), // top-left
            new Vector3( 0.5f, 0f, -0.5f), // bottom-right

            // Triangle 2
            new Vector3( 0.5f, 0f, -0.5f), // bottom-right
            new Vector3(-0.5f, 0f,  0.5f), // top-left
            new Vector3( 0.5f, 0f,  0.5f)  // top-right
        };

        int[] triangles = new int[6] { 0, 1, 2, 3, 4, 5 }; // one triangle per 3 unique vertices

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
