using System.Threading;
using System.Threading.Tasks;

namespace RazorBlade.Tests.Support;

public class StepSemaphore
{
    private readonly SemaphoreSlim _semaphoreStartOfStep = new(0);
    private readonly SemaphoreSlim _semaphoreEndOfStep = new(0);
    private int _isFirstStep;

    public async Task WaitForNextStepAsync()
    {
        if (Interlocked.Exchange(ref _isFirstStep, 1) != 0)
            _semaphoreEndOfStep.Release();

        var result = await _semaphoreStartOfStep.WaitAsync(10_000);
        result.ShouldBeTrue();
    }

    public Task NotifyEndOfLastStepAsync()
    {
        _semaphoreEndOfStep.Release();

        return Task.CompletedTask; // Just to make the API consistent
    }

    public async Task StartNextStepAndWaitForResultAsync()
    {
        _semaphoreStartOfStep.Release();

        var result = await _semaphoreEndOfStep.WaitAsync(10_000);
        result.ShouldBeTrue();
    }
}
