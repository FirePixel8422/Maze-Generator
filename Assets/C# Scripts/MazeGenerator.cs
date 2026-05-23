using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


public class MazeGenerator : MonoBehaviour
{
    private Mesh tileMesh;
    [SerializeField] private Material material;
    [SerializeField] private Color[] tileStateColors;

    [SerializeField] private int2 gridSize;

    [Range(1, 12)]
    [SerializeField] private int gridGenMaxThreadCount = 1;
    public int GridGenThreadCount => math.min(JobsUtility.MaxJobThreadCount, gridGenMaxThreadCount);


    private MazeRenderer mazeRenderer;
    private NativeArray<uint> colorIds;
    private JobTrackerAsync jobTracker;



    private void Awake()
    {
        tileMesh = QuadMesh.Instance;

        mazeRenderer = new MazeRenderer(tileMesh, material, tileStateColors);
        jobTracker = new JobTrackerAsync(OnMazeGenerationComplete);

        StartNewMazeGeneration();
    }

    /// <summary>
    /// Schedule all maze generation jobs in the background
    /// </summary>
    [InspectorButton("Generate Maze")]
    private void StartNewMazeGeneration()
    {
        int mazeLength = gridSize.x * gridSize.y;
        colorIds = new NativeArray<uint>(mazeLength, Allocator.Persistent);

        // Actual maze generation that generates colorIds to represent the maze visually
        var testMazeGen = new TestTileGenJob()
        {
            ColorIds = colorIds,
            ColorCount = (uint)tileStateColors.Length,
            Random = new Unity.Mathematics.Random(12398520),
        };

        JobHandle handle = testMazeGen.Schedule();
        jobTracker.TrackJobHandle(handle);
    }


    private void OnMazeGenerationComplete()
    {
        mazeRenderer.UpdateMazeData(gridSize, colorIds);
    }


    private void OnDestroy()
    {
        jobTracker.Dispose();
        colorIds.Dispose();

        mazeRenderer.Dispose();
    }
}
