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
    [SerializeField] private int gridGenMaxThreadCount;
    public int GridGenThreadCount => math.min(JobsUtility.MaxJobThreadCount, gridGenMaxThreadCount);


    private MazeRenderer mazeRenderer;
    private NativeArray<uint> colorIds;
    private JobHandle mainJobHandle;



    private void Awake()
    {
        tileMesh = QuadMesh.Instance;
        mazeRenderer = new MazeRenderer(tileMesh, material, tileStateColors);

        StartNewMazeGeneration();
    }

    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    private void OnUpdate()
    {
        if (mainJobHandle.IsCompleted == false) return;

        mainJobHandle.Complete();
        mainJobHandle = new JobHandle();

        mazeRenderer.UpdateMazeData(colorIds);

        StartNewMazeGeneration();
    }

    /// <summary>
    /// Start all jobs required to generate a new maze (in the background)
    /// </summary>
    private void StartNewMazeGeneration()
    {
        int mazeLength = mazeSize.x * mazeSize.y;

        // if gridSize changed, recalculate the matrix grid
        if (mazeRenderer.Matrices.NextBatch.Length != mazeLength)
        {
            mazeRenderer.Matrices.EnsureCapacity(mazeLength);

            colorIds = new NativeArray<uint>(mazeLength, Allocator.Persistent);

            var mazeGridGeneratorJob = new GenerateGridJobParallelForBatched
            {
                MazeSize = mazeSize,
                TileSize = 1f,
                MazeMatrices = mazeRenderer.Matrices.NextBatch,
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
        mazeRenderer.Dispose();
    }
}
