using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host;

[SuppressMessage(
    "Design",
    "CA1001:Types that own disposable fields should be disposable",
    Justification = "Lives for entire process lifetime"
)]
internal sealed partial class CoroutineMonitor
{
    /// <summary>
    /// Queue of Task objects representing completed plugin callbacks. This is posted to by the awaiters of the Task
    /// objects when they are completed.
    /// <remarks>
    /// This must be thread-safe as Tasks may complete on any thread.
    /// </remarks>
    /// </summary>
    private readonly ConcurrentQueue<TaskContext> completedTaskQueue;

    /// <summary>
    /// Queue of Task objects representing completed plugin callbacks. This is posted to by the CallInfo's OnResultSet
    /// callback.
    /// <remarks>
    /// This must be thread-safe as Tasks may resume, and set info results thereafter, on any thread.
    /// </remarks>
    /// </summary>
    private readonly ConcurrentQueue<InfoCallbackContext> resultSetQueue;

    /// <summary>
    /// Set of Tasks that are currently suspended. Monitored to ensure Tasks are not left dangling.
    /// </summary>
    private readonly ConcurrentDictionary<Task, object?> suspendedTasks;

    private readonly Timer _diagnosticsTimer;

    public CoroutineMonitor()
    {
        this.completedTaskQueue = new();
        this.resultSetQueue = new();
        this.suspendedTasks = new();

        this._diagnosticsTimer = new Timer(
            _ => this.LogStatistics(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30)
        );
    }

    [SuppressMessage(
        "Microsoft.Performance",
        "CA1823:AvoidUnusedPrivateFields",
        Justification = "Used by the logging source generator"
    )]
    private readonly ILogger<CoroutineMonitor> logger =
        LoggerConfiguration.Factory.CreateLogger<CoroutineMonitor>();

    public void NotifyCallbackSuspended(Task task)
    {
        if (!this.suspendedTasks.TryAdd(task, null))
        {
            this.LogFailedToAddTask();
        }
    }

    public void NotifyCallInfoResultSet(InfoCallbackContext ctx)
    {
        this.resultSetQueue.Enqueue(ctx);
    }

    public void NotifyPluginTaskCompleted(TaskContext ctx)
    {
        this.completedTaskQueue.Enqueue(ctx);
    }

    public void ServiceCompletedCallInfos()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by ServiceCompletedTasks.
        if (this.resultSetQueue.TryDequeue(out var commandContext))
#pragma warning restore CA2000
        {
            LuxonGamePluginResult result = CSharpServerImports.FinishCommand(
                commandContext.CallInfo,
                commandContext.Request,
                commandContext.ManagedRequest
            );

            commandContext.CoroutineHandle.Resume(result);

            // Pass it on to the completed task queue, if already completed, but otherwise set it up to join that queue
            // once the task is completed. This allows the task queue to dispose of contexts with the knowledge that
            // their info coroutine has already been processed.
            if (commandContext.PluginTask.IsCompleted)
            {
                this.NotifyPluginTaskCompleted(commandContext);
            }
            else
            {
                commandContext
                    .PluginTask.GetAwaiter()
                    .OnCompleted(() => this.NotifyPluginTaskCompleted(commandContext));
            }
        }
    }

    public void ServiceCompletedTasks()
    {
        if (this.completedTaskQueue.TryDequeue(out var taskContext))
        {
            var task = taskContext.PluginTask;

            Debug.Assert(
                task.IsCompleted,
                "The only thing that adds to CompletedTaskQueue is the OnCompleted callback of task awaiters"
            );

            if (task.IsCompletedSuccessfully)
            {
                this.LogPluginTaskCompletedSuccessfully();
            }
            else
            {
                if (task.IsCanceled)
                {
                    this.LogPluginTaskWasCanceled();
                }
                else if (task.IsFaulted)
                {
                    this.OnUnhandledPluginException(task.Exception);
                }

                if (
                    taskContext is InfoCallbackContext infoContext
                    && !infoContext.CallInfo.HasResult()
                )
                {
                    // If the task was cancelled or failed before setting a result, then set an error and restart the command.
                    // This will enqueue it in CommandRestartQueue.
#if DEBUG
                    int oldLength = this.resultSetQueue.Count;
#endif
                    infoContext.CallInfo.Fail();

#if DEBUG
                    Debug.Assert(
                        this.resultSetQueue.Count == oldLength + 1,
                        "Calling Fail should add the info to the queue"
                    );
#endif
                }
            }

            taskContext.Dispose();
            if (!this.suspendedTasks.TryRemove(task, out _))
            {
                this.LogFailedToRemoveTask();
            }
        }
    }

    public void OnUnhandledPluginException(Exception exception)
    {
        Exception displayException =
            (exception is AggregateException { InnerExceptions: [{ } singleException] })
                ? singleException
                : exception;

        this.LogUnhandledExceptionInPluginCallback(displayException);
    }

    private void LogStatistics()
    {
        this.LogSize("SuspendedTasks", this.suspendedTasks.Count);
        this.LogSize("CompletedTaskQueue", this.completedTaskQueue.Count);
        this.LogSize("ResultSetQueue", this.resultSetQueue.Count);
    }

    [LoggerMessage(LogLevel.Error, "Unhandled exception in plugin callback")]
    private partial void LogUnhandledExceptionInPluginCallback(Exception exception);

    [LoggerMessage(LogLevel.Debug, "Plugin task completed successfully")]
    private partial void LogPluginTaskCompletedSuccessfully();

    [LoggerMessage(LogLevel.Warning, "Plugin task was canceled")]
    private partial void LogPluginTaskWasCanceled();

    [LoggerMessage(LogLevel.Debug, "{Name} size: {Size}")]
    private partial void LogSize(string name, int size);

    [LoggerMessage(LogLevel.Error, "Failed to add suspended task reference")]
    partial void LogFailedToAddTask();

    [LoggerMessage(LogLevel.Error, "Failed to remove suspended task reference")]
    partial void LogFailedToRemoveTask();
}
