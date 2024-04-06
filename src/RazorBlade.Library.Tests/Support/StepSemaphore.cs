using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RazorBlade.Tests.Support;

public class StepSemaphore
{
    private readonly SemaphoreSlim _semaphoreStartOfStep = new(0);
    private readonly SemaphoreSlim _semaphoreEndOfStep = new(0);

    private bool _hasWorker;
    private bool _hasController;
    private bool _isFirstStep = true;

    public IWorker CreateWorker()
    {
        if (_hasWorker)
            throw new InvalidOperationException("A worker has already been created");

        _hasWorker = true;
        return new Worker(this);
    }

    public IController CreateController()
    {
        if (_hasController)
            throw new InvalidOperationException("A controller has already been created");

        _hasController = true;
        return new Controller(this);
    }

    private async Task WaitForNextStepAsync()
    {
        if (!_isFirstStep)
            _semaphoreEndOfStep.Release();

        _isFirstStep = false;
        await WaitWithTimeout(_semaphoreStartOfStep);
    }

    private void NotifyEndOfLastStep()
        => _semaphoreEndOfStep.Release();

    private async Task StartNextStepAndWaitForResultAsync()
    {
        _semaphoreStartOfStep.Release();
        await WaitWithTimeout(_semaphoreEndOfStep);
    }

    private static async Task WaitWithTimeout(SemaphoreSlim semaphore)
        => (await semaphore.WaitAsync(10_000)).ShouldBeTrue();

    public interface IWorker : IDisposable
    {
        Task WaitForNextStepAsync();
    }

    public interface IController
    {
        Task StartNextStepAndWaitForResultAsync();
    }

    private class Worker(StepSemaphore parent) : IWorker
    {
        public Task WaitForNextStepAsync()
            => parent.WaitForNextStepAsync();

        public void Dispose()
            => parent.NotifyEndOfLastStep();
    }

    private class Controller(StepSemaphore parent) : IController
    {
        public Task StartNextStepAndWaitForResultAsync()
            => parent.StartNextStepAndWaitForResultAsync();
    }
}
