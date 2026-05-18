using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Utility class responsible for rendering the maze in a fast and efficient way
/// </summary>
public class MazeRenderer
{
    private readonly Mesh tileMesh;

    private readonly MaterialPropertyBlock mbp;
    private readonly RenderParams renderParams;

    private readonly ComputeBuffer colorBuffer;

    private int gridWidth;
    private int gridHeight;

    private ComputeBuffer colorIdBuffer;


    public MazeRenderer(Mesh mesh, Material material, Color[] tileStateColors)
    {
        tileMesh = mesh;
        material.EnableKeyword("USE_COLOR_ID_BUFFER");

        // Convert UnityEngine.Color array to float4 array
        NativeArray<float4> colorData = new NativeArray<float4>(tileStateColors.Length, Allocator.Temp);
        for (int i = 0; i < tileStateColors.Length; i++)
        {
            Color color = tileStateColors[i];
            colorData[i] = new float4(color.r, color.g, color.b, color.a);
        }
        colorBuffer = new ComputeBuffer(colorData.Length, sizeof(float) * 4);
        colorBuffer.SetData(colorData);

        mbp = new MaterialPropertyBlock();
        mbp.SetBuffer("_ColorBuffer", colorBuffer);

        renderParams = new RenderParams(material)
        {
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
            receiveShadows = false,
            motionVectorMode = MotionVectorGenerationMode.ForceNoMotion,
            worldBounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue),
            matProps = mbp,
        };

        UpdateScheduler.RegisterLateUpdate(RenderMaze);

        colorData.Dispose();
    }
    private MazeRenderer() { }


    /// <summary>
    /// Update maze render data by updating colorBuffer and matrix array
    /// </summary>
    public void UpdateMazeData(int2 gridSize, NativeArray<uint> ColorIds)
    {
        gridWidth = gridSize.x;
        gridHeight = gridSize.y;

        mbp.SetFloat("_GridWidth", gridWidth);
        mbp.SetFloat("_GridHeight", gridHeight);

        if (colorIdBuffer == null || colorIdBuffer.count != ColorIds.Length)
        {
            colorIdBuffer?.Dispose();
            colorIdBuffer = new ComputeBuffer(ColorIds.Length, sizeof(uint));
        }
        colorIdBuffer.SetData(ColorIds);

        mbp.SetBuffer("_ColorIdBuffer", colorIdBuffer);
    }
    /// <summary>
    /// Render the maze walls and floor based on asociated matrix arrays
    /// </summary>
    private void RenderMaze()
    {
        if (gridWidth * gridHeight == 0) return;

        Graphics.RenderMeshPrimitives(renderParams, tileMesh, 0, gridWidth * gridHeight);
    }

    /// <summary>
    /// Cleanup native memmory usage and render callback
    /// </summary>
    public void Dispose()
    {
        UpdateScheduler.UnRegisterLateUpdate(RenderMaze);

        colorIdBuffer?.DisposeIfValid();
        colorBuffer?.DisposeIfValid();
    }
}
