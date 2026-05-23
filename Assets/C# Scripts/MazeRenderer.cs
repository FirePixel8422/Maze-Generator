using Fire_Pixel.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Utility class responsible for rendering the maze in a fast and efficient way
/// </summary>
public class MazeRenderer
{
    private readonly Mesh quadMesh;
    private readonly Material material;

    private readonly Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    
    private readonly uint[] args;
    private readonly ComputeBuffer argsBuffer;

    private readonly ComputeBuffer colorBuffer;
    private ComputeBuffer colorIdBuffer;

    private int gridLength;


    private MazeRenderer() { }
    public MazeRenderer(Mesh quadMesh, Material material, Color[] tileStateColors)
    {
        this.quadMesh = quadMesh;
        this.material = material;

        args = new uint[5]
        {
            6,
            0,
            0,
            0,
            0
        };
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        NativeArray<float4> colorData = new NativeArray<float4>(tileStateColors.Length, Allocator.Temp);

        for (int i = 0; i < tileStateColors.Length; i++)
        {
            Color c = tileStateColors[i];
            colorData[i] = new float4(c.r, c.g, c.b, c.a);
        }

        colorBuffer = new ComputeBuffer(colorData.Length, sizeof(float) * 4);
        colorBuffer.SetData(colorData);

        material.EnableKeyword("USE_COLOR_ID_BUFFER");
        material.SetBuffer("_ColorBuffer", colorBuffer);

        colorData.Dispose();

        CallbackScheduler.RegisterLateUpdate(RenderMaze);
    }

    public void UpdateMazeData(int2 gridSize, NativeArray<uint> colorIds)
    {
        gridLength = gridSize.x * gridSize.y;

        if (colorIdBuffer == null || colorIdBuffer.count != colorIds.Length)
        {
            colorIdBuffer?.Dispose();
            colorIdBuffer = new ComputeBuffer(colorIds.Length, sizeof(uint));
        }
        colorIdBuffer.SetData(colorIds);

        args[1] = (uint)gridLength;
        argsBuffer.SetData(args);

        material.SetBuffer("_ColorIdBuffer", colorIdBuffer);
        material.SetFloat("_GridWidth", gridSize.x);
        material.SetFloat("_GridHeight", gridSize.y);
    }

    private void RenderMaze()
    {
        if (gridLength == 0 || argsBuffer == null) return;

        Graphics.DrawMeshInstancedIndirect(quadMesh, 0, material, bounds, argsBuffer);
    }

    public void Dispose()
    {
        CallbackScheduler.UnRegisterLateUpdate(RenderMaze);

        colorIdBuffer?.DisposeIfValid();
        colorBuffer?.DisposeIfValid();
        argsBuffer?.DisposeIfValid();
    }
}