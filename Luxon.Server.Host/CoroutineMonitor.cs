using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host;

internal sealed partial class CoroutineMonitor
{
    /// <summary>
    /// Queue of Task objects representing completed plugin callbacks. This is posted to by the awaiters of the Task
    /// objects when they are completed.
    /// <remarks>
    /// This must be thread-safe as Tasks may complete on any thread.
    /// </remarks>
    /// </summary>
    private ConcurrentQueue<TaskContext> completedTaskQueue = new();

    /// <summary>
    /// Queue of Task objects representing completed plugin callbacks. This is posted to by the CallInfo's OnResultSet
    /// callback.
    /// <remarks>
    /// This must be thread-safe as Tasks may resume, and set info results thereafter, on any thread.
    /// </remarks>
    /// </summary>
    private ConcurrentQueue<InfoCallbackContext> resultSetQueue = new();

    [SuppressMessage(
        "Microsoft.Performance",
        "CA1823:AvoidUnusedPrivateFields",
        Justification = "Used by the logging source generator"
    )]
    private readonly ILogger<CoroutineMonitor> logger =
        LoggerConfiguration.Factory.CreateLogger<CoroutineMonitor>();

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
#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by the task queue monitor.
        if (this.resultSetQueue.TryDequeue(out var commandContext))
#pragma warning restore CA2000
        {
            LuxonGamePluginResult result = CSharpServerImports.FinishCommand(
                commandContext.CallInfo,
                commandContext.Request,
                commandContext.ManagedRequest
            );

            commandContext.CoroutineHandle.Resume(result);
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

            // Don't dispose - if the queues are busy we may not have handled the info result yet
            // taskContext.Dispose();
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

    [LoggerMessage(LogLevel.Error, "Unhandled exception in plugin callback")]
    private partial void LogUnhandledExceptionInPluginCallback(Exception exception);

    [LoggerMessage(LogLevel.Debug, "Plugin task completed successfully")]
    private partial void LogPluginTaskCompletedSuccessfully();

    [LoggerMessage(LogLevel.Warning, "Plugin task was canceled")]
    private partial void LogPluginTaskWasCanceled();
}
