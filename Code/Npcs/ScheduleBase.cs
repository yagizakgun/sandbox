namespace Sandbox.Npcs;

/// <summary>
/// Base class for NPC schedules
/// </summary>
public abstract class ScheduleBase
{
	public Behavior Behavior { get; private set; }

	// Accessors
	protected Npc Npc => Behavior.Npc;

	private List<TaskBase> _tasks = new();
	private int _currentTaskIndex = 0;
	private bool _started;

	/// <summary>
	/// Initialize the schedule with the Behavior context
	/// </summary>
	internal void Initialize( Behavior behavior )
	{
		Behavior = behavior;
		_tasks.Clear();
		_currentTaskIndex = 0;
		_started = false;
	}

	/// <summary>
	/// Called once when schedule starts
	/// </summary>
	internal void InternalStart()
	{
		if ( _started ) return;
		_started = true;

		// Build task sequence
		OnStart();

		// Start first task
		StartCurrentTask();
	}

	/// <summary>
	/// Called every frame while schedule is running
	/// </summary>
	internal TaskStatus OnUpdate()
	{
		if ( _tasks.Count == 0 )
			return TaskStatus.Failed;

		if ( _currentTaskIndex >= _tasks.Count )
			return TaskStatus.Success; // All tasks completed

		var currentTask = _tasks[_currentTaskIndex];
		var status = currentTask.InternalUpdate();

		switch ( status )
		{
			case TaskStatus.Success:
				// Move to next task
				currentTask.InternalEnd();
				_currentTaskIndex++;
				StartCurrentTask();
				return TaskStatus.Running;

			case TaskStatus.Failed:
			case TaskStatus.Interrupted:
				currentTask.InternalEnd();
				return status;

			case TaskStatus.Running:
				return TaskStatus.Running;
		}

		return TaskStatus.Running;
	}

	/// <summary>
	/// Called once when schedule ends
	/// </summary>
	internal void InternalEnd()
	{
		// End current task if running
		if ( _currentTaskIndex < _tasks.Count )
		{
			_tasks[_currentTaskIndex].InternalEnd();
		}

		OnEnd();
	}

	/// <summary>
	/// Override to build the sequence of tasks for this schedule
	/// </summary>
	protected abstract void OnStart();

	/// <summary>
	/// Add a task to the sequence
	/// </summary>
	protected void AddTask( TaskBase task )
	{
		_tasks.Add( task );
	}

	/// <summary>
	/// Override for schedule cleanup
	/// </summary>
	protected virtual void OnEnd() { }

	/// <summary>
	/// Start the current task in sequence
	/// </summary>
	private void StartCurrentTask()
	{
		if ( _currentTaskIndex < _tasks.Count )
		{
			var task = _tasks[_currentTaskIndex];
			task.Initialize( Npc, Behavior );
			task.InternalStart();
		}
	}
}
