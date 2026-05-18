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
    private JobHandle mainJobHandle;
    private bool jobActive;


    private void Awake()
    {
        tileMesh = QuadMesh.Instance;

        mazeRenderer = new MazeRenderer(tileMesh, material, tileStateColors);

        UpdateScheduler.RegisterUpdate(OnUpdate);
        StartNewMazeGeneration();
    }

    private void OnUpdate()
    {
        if (!mainJobHandle.IsCompleted || !jobActive) return;

        mainJobHandle.Complete();
        jobActive = false;

        mazeRenderer.UpdateMazeData(gridSize, colorIds);
    }

    /// <summary>
    /// Start all jobs required to generate a new maze (in the background)
    /// </summary>
    [InspectorButton("Generate Maze")]
    private void StartNewMazeGeneration()
    {
        int mazeLength = gridSize.x * gridSize.y;

        //// if gridSize changed, recalculate the matrix grid
        //if (mazeRenderer.Matrices.NextBatch.Length != mazeLength)
        //{
        //    mazeRenderer.Matrices.EnsureCapacity(mazeLength);

        colorIds = new NativeArray<uint>(mazeLength, Allocator.Persistent);

        //    var mazeGridGeneratorJob = new GenerateGridJobParallelForBatched
        //    {
        //        MazeSize = gridSize,
        //        TileSize = 1f,
        //        MazeMatrices = mazeRenderer.Matrices.NextBatch,
        //    };

        //    mainJobHandle = mazeGridGeneratorJob.Schedule(mazeLength, mazeLength / GridGenThreadCount);
        //}

        // Actual maze generation that generates colorIds to represent the maze visually
        var testMazeGen = new TestTileGenJob()
        {
            ColorIds = colorIds,
            ColorCount = (uint)tileStateColors.Length,
            Random = new Unity.Mathematics.Random(12398520),
        };

        mainJobHandle = JobHandle.CombineDependencies(
            mainJobHandle,
            testMazeGen.Schedule()
            );
        jobActive = true;
    }


    private void OnDestroy()
    {
        UpdateScheduler.UnRegisterUpdate(OnUpdate);

        mainJobHandle.Complete();
        colorIds.Dispose();

        mazeRenderer.Dispose();
    }
}
