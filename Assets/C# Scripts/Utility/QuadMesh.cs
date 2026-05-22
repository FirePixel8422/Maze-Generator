using UnityEngine;


/// <summary>
/// Static class generating a memmory efficient quad mesh instance.
/// </summary>
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

        mesh.SetVertices(new[]
        {
            new Vector3(-0.5f, 0f,  0.5f),
            new Vector3( 0.5f, 0f,  0.5f),
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f, -0.5f),
        });

        mesh.SetIndices(
            new ushort[]
            {
                0, 1, 2,
                1, 3, 2
            },
            MeshTopology.Triangles,
            0,
            false);

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.UploadMeshData(true);

        return mesh;
    }
}
