using Unity.Collections;
using UnityEngine;


/// <summary>
/// Utility container that tracks 2 matrix batches so 1 can always be safely used in a job
/// </summary>
public class MatrixBatch
{
    private NativeArray<Matrix4x4> batch1;
    private NativeArray<Matrix4x4> batch2;

    private int cBatchId;

    public ref NativeArray<Matrix4x4> CurrentBatch => ref (cBatchId == 0 ? ref batch1 : ref batch2);
    public ref NativeArray<Matrix4x4> NextBatch => ref (cBatchId == 0 ? ref batch2 : ref batch1);


    public MatrixBatch(int startBatchSize = 0)
    {
        batch1 = new NativeArray<Matrix4x4>(startBatchSize, Allocator.Persistent);
        batch2 = new NativeArray<Matrix4x4>(startBatchSize, Allocator.Persistent);
    }

    public void SwapBatches()
    {
        // Flip batches
        cBatchId ^= 1;
    }

    /// <summary>
    /// Ensure new next batch has enough space for next requested matrix grid job operation
    /// </summary>
    public void EnsureCapacity(int nextBatchSize)
    {
        if (nextBatchSize != NextBatch.Length)
        {
            NextBatch.Dispose();
            NextBatch = new NativeArray<Matrix4x4>(nextBatchSize, Allocator.Persistent);
        }
    }

    public void Dispose()
    {
        batch1.DisposeIfCreated();
        batch2.DisposeIfCreated();
    }
}
