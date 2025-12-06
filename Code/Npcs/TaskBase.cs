using System.Threading;

namespace Sandbox.Npcs;

/// <summary>
/// Exception thrown when a task is cancelled due to conditions
/// </summary>
public sealed class TaskCancelledException : Exception
{
	public string CancelledCondition { get; }
	public bool WasConditionPresent { get; }

	public TaskCancelledException( string condition, bool wasPresent )
		: base( $"Task cancelled: condition '{condition}' {(wasPresent ? "became true" : "became false")}" )
	{
		CancelledCondition = condition;
		WasConditionPresent = wasPresent;
	}
}

public abstract class TaskBase
{
	protected ScheduleBase Schedule { get; private set; }
	protected Behavior Behavior => Schedule.Behavior;
	protected Npc Npc => Behavior.Npc;
	protected Conditions Conditions => Npc.Conditions;

	private TaskSource _taskSource;
	private readonly List<string> _cancelWhenConditions = new();
	private readonly List<string> _cancelWhenNotConditions = new();
	private Task _conditionCheckTask;
	private bool _cancelled;

	public bool IsCancelled => _cancelled || !_taskSource.IsValid;
	public TaskCancelledException TaskCancelledException { get; private set; }

	internal void Initialize( ScheduleBase schedule )
	{
		Schedule = schedule;
		_taskSource = schedule.Behavior.GetTaskSource();

		if ( _cancelWhenConditions.Count > 0 || _cancelWhenNotConditions.Count > 0 )
		{
			_conditionCheckTask = StartConditionChecking();
		}
	}

	/// <summary>
	/// Cancel the task when the specified condition becomes true
	/// </summary>
	public TaskBase CancelWhen( string condition )
	{
		_cancelWhenConditions.Add( condition );
		return this;
	}

	/// <summary>
	/// Cancel the task when the specified condition becomes false
	/// </summary>
	public TaskBase CancelWhenNot( string condition )
	{
		_cancelWhenNotConditions.Add( condition );
		return this;
	}

	/// <summary>
	/// Background task that continuously checks conditions
	/// </summary>
	private async Task StartConditionChecking()
	{
		try
		{
			while ( _taskSource.IsValid && !_cancelled )
			{
				if ( CheckCancellationConditions() )
					break;

				await _taskSource.FrameEnd();
			}
		}
		catch ( OperationCanceledException )
		{
			// Expected when TaskSource is cancelled
		}
	}

	/// <summary>
	/// Check if any cancellation conditions are met
	/// </summary>
	private bool CheckCancellationConditions()
	{
		// The TaskSource lifetime check eliminates most null reference issues
		if ( !_taskSource.IsValid )
		{
			Cancel( "component-destroyed", false );
			return true;
		}

		foreach ( var condition in _cancelWhenConditions )
		{
			if ( Conditions.Contains( condition ) )
			{
				Cancel( condition, true );
				return true;
			}
		}

		foreach ( var condition in _cancelWhenNotConditions )
		{
			if ( !Conditions.Contains( condition ) )
			{
				Cancel( condition, false );
				return true;
			}
		}

		return false;
	}

	public void Cancel( string condition = null, bool wasConditionPresent = false )
	{
		if ( !_cancelled )
		{
			_cancelled = true;
			TaskCancelledException = condition != null
				? new TaskCancelledException( condition, wasConditionPresent )
				: new TaskCancelledException( "manual", false );
		}
	}

	/// <summary>
	/// Execute the task with cancellation support
	/// </summary>
	public async Task ExecuteWithCancellation()
	{
		try
		{
			await Execute();
		}
		catch ( TaskCanceledException )
		{
			if ( TaskCancelledException != null )
			{
				throw TaskCancelledException;
			}
			throw;
		}
		finally
		{
			_cancelled = true;
		}
	}

	/// <summary>
	/// Execute the task - implement this in derived classes
	/// </summary>
	public abstract Task Execute();

	/// <summary>
	/// Delay for a specified time while respecting cancellation
	/// </summary>
	protected Task DelaySeconds( float seconds ) => _taskSource.DelaySeconds( seconds );

	/// <summary>
	/// Wait until next frame while respecting cancellation
	/// </summary>
	protected Task FrameEnd() => _taskSource.FrameEnd();

	/// <summary>
	/// Wait until start of next frame
	/// </summary>
	protected Task Frame() => _taskSource.Frame();

	/// <summary>
	/// Yield execution
	/// </summary>
	protected Task Yield() => _taskSource.Yield();

	/// <summary>
	/// Helper method to throw cancellation exception
	/// </summary>
	protected static TaskCancelledException Cancelled( string reason = "Task was cancelled" )
	{
		return new TaskCancelledException( reason, false );
	}
}
