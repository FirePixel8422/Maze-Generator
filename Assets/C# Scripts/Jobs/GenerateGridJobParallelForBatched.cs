using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;


/// <summary>
/// Generate a matrix position grid for the maze based on dimensions and tile size
/// </summary>
[BurstCompile]
public struct GenerateGridJobParallelForBatched : IJobParallelForBatch
{
    [NoAlias][ReadOnly] public int2 MazeSize;
    [NoAlias][ReadOnly] public float TileSize;

    [NativeDisableParallelForRestriction]
    [NoAlias] public NativeArray<Matrix4x4> MazeMatrices;


    [BurstCompile]
    public void Execute(int startIndex, int count)
    {
        int width = MazeSize.x;
        float3 centerOffset = new float3(
            (MazeSize.x * TileSize) * 0.5f - (TileSize * 0.5f),
            0,
            (MazeSize.y * TileSize) * 0.5f - (TileSize * 0.5f)
        );

        // Calculate start x and y based on startIndex
        int x = startIndex % width;
        int y = startIndex / width;

        int cIndex = startIndex;
        int2 gridPos;

        for (int i = 0; i < count; i++, cIndex++)
        {
            gridPos = new int2(x, y);

            MazeMatrices[cIndex] = Matrix4x4.TRS(
                new float3(gridPos.x * TileSize, 0, gridPos.y * TileSize) - centerOffset,
                Quaternion.identity,
                Vector3.one
                );

            // Update gridPositions for next index
            x++;
            if (x == width)
            {
                x = 0;
                y++;
            }
        }
    }
}
