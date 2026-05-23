using Fire_Pixel.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Utility class responsible for rendering the maze in a fast and efficient way
/// </summary>
public class MazeRenderer
{
    private readonly Mesh tileMesh;

    private readonly Material material;
    private readonly RenderParams renderParams;

    private readonly ComputeBuffer colorBuffer;

    private int gridLength;
    private ComputeBuffer colorIdBuffer;

    private MazeRenderer() { }
    public MazeRenderer(Mesh mesh, Material material, Color[] tileStateColors)
    {
        tileMesh = mesh;
        this.material = material;

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
        material.SetBuffer("_ColorBuffer", colorBuffer);

        renderParams = new RenderParams(material)
        {
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
            receiveShadows = false,
            motionVectorMode = MotionVectorGenerationMode.ForceNoMotion,
            worldBounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue),
        };

        CallbackScheduler.RegisterLateUpdate(RenderMaze);

        colorData.Dispose();
    }

    public void UpdateMazeData(int2 gridSize, NativeArray<uint> ColorIds)
    {
        gridLength = gridSize.x * gridSize.y;

        if (colorIdBuffer == null || colorIdBuffer.count != ColorIds.Length)
        {
            colorIdBuffer?.Dispose();
            colorIdBuffer = new ComputeBuffer(ColorIds.Length, sizeof(uint));
        }
        colorIdBuffer.SetData(ColorIds);

        material.SetFloat("_GridWidth", gridSize.x);
        material.SetFloat("_GridHeight", gridSize.y);
        material.SetBuffer("_ColorIdBuffer", colorIdBuffer);

    }

    private void RenderMaze()
    {
        if (gridLength == 0) return;

        Graphics.RenderMeshPrimitives(renderParams, tileMesh, 0, gridLength);
    }

    public void Dispose()
    {
        CallbackScheduler.UnRegisterLateUpdate(RenderMaze);

        colorIdBuffer?.DisposeIfValid();
        colorBuffer?.DisposeIfValid();
    }
}