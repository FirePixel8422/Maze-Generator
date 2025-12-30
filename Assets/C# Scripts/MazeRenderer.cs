using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Utility class Responsible for rendering the maze in a fast and efficient way
/// </summary>
public static class MazeRenderer
{
    private static Mesh tileMesh;

    private static MaterialPropertyBlock mbp;
    private static RenderParams renderParams;

    public static MatrixBatch Matrices;
    private static ComputeBuffer colorBuffer;
    private static ComputeBuffer colorIdBuffer;


    public static void Init(Mesh mesh, Material material, Color[] tileStateColors)
    {
        tileMesh = mesh;
        material.EnableKeyword("USE_COLOR_ID_BUFFER");

        mbp = new MaterialPropertyBlock();

        // Convert UnityEngine.Color array to float4 array
        NativeArray<float4> colorData = new NativeArray<float4>(tileStateColors.Length, Allocator.Temp);
        for (int i = 0; i < tileStateColors.Length; i++)
        {
            Color color = tileStateColors[i];
            colorData[i] = new float4(color.r, color.g, color.b, color.a);
        }
        colorBuffer = new ComputeBuffer(colorData.Length, sizeof(float) * 4);
        colorBuffer.SetData(colorData);
        mbp.SetBuffer("_ColorBuffer", colorBuffer);

        renderParams = new RenderParams(material)
        {
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
            receiveShadows = false,
            motionVectorMode = MotionVectorGenerationMode.ForceNoMotion,
            matProps = mbp
        };

        Matrices = new MatrixBatch();
        UpdateScheduler.RegisterLateUpdate(RenderMaze);
    }


    /// <summary>
    /// Update maze render data by updating colorBuffer and matrix array
    /// </summary>
    public static void UpdateMazeData(NativeArray<uint> ColorIds)
    {
        colorIdBuffer = new ComputeBuffer(ColorIds.Length, sizeof(uint));
        colorIdBuffer.SetData(ColorIds);

        mbp.SetBuffer("_ColorIdBuffer", colorIdBuffer);

        Matrices.SwapBatches();
    }
    /// <summary>
    /// Render the maze walls and floor based on asociated matrix arrays
    /// </summary>
    private static void RenderMaze()
    {
        if (Matrices.CurrentBatch.Length == 0) return;

        Graphics.RenderMeshInstanced(renderParams, tileMesh, 0, Matrices.CurrentBatch);
    }

    /// <summary>
    /// Cleanup native memmory usage and render callback
    /// </summary>
    public static void Dispose()
    {
        UpdateScheduler.UnRegisterLateUpdate(RenderMaze);

        Matrices.Dispose();
        colorIdBuffer.Dispose();
        colorBuffer.Dispose();
    }
}
