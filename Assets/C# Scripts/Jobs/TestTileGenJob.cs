using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct TestTileGenJob : IJob
{
    [NoAlias][WriteOnly] public NativeArray<uint> ColorIds;
    [NoAlias][ReadOnly] public int ColorCount;
    [NoAlias][ReadOnly] public Random Random;

    [BurstCompile]
    public void Execute()
    {
        for (int i = 0; i < ColorIds.Length; i++)
        {
            ColorIds[i] = (uint)Random.NextInt(0, ColorCount);
        }
    }
}