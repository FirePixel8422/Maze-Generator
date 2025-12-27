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

    [SerializeField] private int2 mazeSize;

    [Range(1, 12)]
    [SerializeField] private int gridGenMaxThreadCount = 1;
    public int GridGenThreadCount => math.min(JobsUtility.MaxJobThreadCount, gridGenMaxThreadCount);


    private NativeArray<uint> colorIds;
    private JobHandle mainJobHandle;



    private void Awake()
    {
        tileMesh = QuadMesh.Instance;

        MazeRenderer.Init(tileMesh, material, tileStateColors);

        UpdateScheduler.RegisterUpdate(OnUpdate);
        StartNewMazeGeneration();
    }

    private void OnUpdate()
    {
        if (mainJobHandle.IsCompleted == false) return;

        mainJobHandle.Complete();
        mainJobHandle = new JobHandle();

        MazeRenderer.UpdateMazeData(colorIds);

        StartNewMazeGeneration();
    }

    /// <summary>
    /// Start all jobs required to generate a new maze (in the background)
    /// </summary>
    private void StartNewMazeGeneration()
    {
        int mazeLength = mazeSize.x * mazeSize.y;

        // if gridSize changed, recalculate the matrix grid
        if (true || MazeRenderer.Matrices.NextBatch.Length != mazeLength)
        {
            MazeRenderer.Matrices.EnsureCapacity(mazeLength);

            colorIds = new NativeArray<uint>(mazeLength, Allocator.Persistent);

            var mazeGridGeneratorJob = new GenerateGridJobParallelForBatched
            {
                MazeSize = mazeSize,
                TileSize = 1f,
                MazeMatrices = MazeRenderer.Matrices.NextBatch,
            };

            mainJobHandle = mazeGridGeneratorJob.Schedule(mazeLength, mazeLength / GridGenThreadCount);
        }

        // Actual maze generation that generates colorIds to represent the maze visually
        var testMazeGen = new TestTileGenJob()
        {
            ColorIds = colorIds,
            Random = new Unity.Mathematics.Random(12398520),
        };

        mainJobHandle = JobHandle.CombineDependencies(
            mainJobHandle,
            testMazeGen.Schedule()
            );
    }


    private void OnDestroy()
    {
        UpdateScheduler.UnRegisterUpdate(OnUpdate);

        mainJobHandle.Complete();
        colorIds.Dispose();

        MazeRenderer.Dispose();
    }
}
