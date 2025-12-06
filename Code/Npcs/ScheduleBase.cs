using System.Threading;

namespace Sandbox.Npcs;

/// <summary>
/// Sometimes execution will have different outcomes, for example, when in parallel
/// </summary>
public enum ExecutionMode
{
	/// <summary>
	/// Run all tasks until one succeeds.
	/// </summary>
	SucceedOnOne,

	/// <summary>
	/// Run all tasks until they have all succeeded.
	/// </summary>
	SucceedOnAll
}

public abstract class ScheduleBase
{
    public Behavior Behavior { get; private set; }

	//
	// Some accessors
	//

	protected Npc Npc => Behavior.Npc;
	protected Scene Scene => Behavior.Scene;
	protected Conditions Conditions => Npc.Conditions;

	private CancellationTokenSource _cancellationTokenSource;
    private TaskBase _currentTask;

    public bool IsCancelled => _cancellationTokenSource?.Token.IsCancellationRequested == true;

    /// <summary>
    /// Initialize the schedule with the Behavior context
    /// </summary>
    internal void Initialize( Behavior behavior )
    {
        Behavior = behavior;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Cancel the entire schedule
    /// </summary>
    public void Cancel()
    {
        _currentTask?.Cancel();
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Execute the schedule and handle its cancellation
    /// </summary>
    public async Task ExecuteWithCancellation()
    {
        try
        {
            await Execute();
        }
        catch ( OperationCanceledException )
        {
            // Schedule was cancelled
            throw;
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Execute the schedule
    /// </summary>
    public abstract Task Execute();

    /// <summary>
    /// Execute a task and handle its cancellation
    /// </summary>
    protected async Task ExecuteTask( TaskBase task )
    {
        task.Initialize( this );
        _currentTask = task;

        try
        {
            await task.ExecuteWithCancellation();
        }
        catch ( TaskCancelledException ex )
        {
            // Handle task cancellation
            await OnTaskCancelled( task, ex.CancelledCondition, ex.WasConditionPresent );
        }
        finally
        {
            _currentTask = null;
        }
    }

	/// <summary>
	/// Execute multiple tasks concurrently
	/// </summary>
	protected async Task ExecuteParallel( ExecutionMode mode, params TaskBase[] tasks )
    {
        var taskList = new List<Task>();

        foreach ( var task in tasks )
        {
            task.Initialize( this );
            taskList.Add( task.ExecuteWithCancellation() );
        }

        try
        {
			if ( mode == ExecutionMode.SucceedOnOne )
			{
				await GameTask.WhenAny( taskList );
			}
			if ( mode == ExecutionMode.SucceedOnAll )
			{
				await GameTask.WhenAll( taskList );
			}
		}
        catch ( TaskCancelledException ex )
        {
            // Handle the cancellation
            var cancelledTask = tasks.FirstOrDefault( t => t.TaskCancelledException?.CancelledCondition == ex.CancelledCondition );
            if ( cancelledTask != null )
            {
                await OnTaskCancelled( cancelledTask, ex.CancelledCondition, ex.WasConditionPresent );
            }
        }
		finally
		{
			// Cancel any remaining tasks
			foreach ( var task in tasks )
			{
				task.Cancel();
			}
		}
	}

    /// <summary>
    /// Called when a task is cancelled due to conditions
    /// Override this to handle task cancellations
    /// </summary>
    protected virtual Task OnTaskCancelled( TaskBase task, string condition, bool wasConditionPresent )
    {
        Log.Info( $"Task {task.GetType().Name} cancelled due to condition '{condition}' (was present: {wasConditionPresent})" );
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delay for a specified time while checking for cancellation
    /// </summary>
    protected async Task DelaySeconds( float seconds )
    {
        await GameTask.DelaySeconds( seconds, _cancellationTokenSource?.Token ?? CancellationToken.None );
    }
}
