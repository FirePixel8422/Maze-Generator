using Fire_Pixel.Utility;
using System;
using Unity.Jobs;


/// <summary>
/// Lightweight container class that tracks a jobs progress every frame and auto completes it after its done, after which it invokes an onComplete callback.
/// </summary>
public class JobTrackerAsync
{
    private Action onComplete;
    private JobHandle handle;
    private bool isJobActive;


    private JobTrackerAsync() { }
    public JobTrackerAsync(Action onCompleteAction)
    {
        onComplete = onCompleteAction;
        CallbackScheduler.RegisterUpdate(OnUpdate);
    }
    /// <summary>
    /// Force complete active job and dispose this container
    /// </summary>
    public void Dispose()
    {
        if (isJobActive)
        {
            handle.Complete();
        }
        isJobActive = false;
        onComplete = null;
        CallbackScheduler.UnRegisterUpdate(OnUpdate);
    }


    public void TrackJobHandle(JobHandle newHandle)
    {
        handle = newHandle;
        isJobActive = true;
    }

    private void OnUpdate()
    {
        if (!isJobActive || !handle.IsCompleted) return;

        handle.Complete();
        onComplete?.Invoke();
        isJobActive = false;
    }
}