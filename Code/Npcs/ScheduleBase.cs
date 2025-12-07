namespace Sandbox.Npcs;

/// <summary>
/// A schedule -- can be understood as a way to execute a sequence of tasks
/// </summary>
public abstract class ScheduleBase
{
	public Behavior Behavior { get; private set; }
	protected Npc Npc => Behavior.Npc;

	private List<TaskBase> _tasks = new();
	private int _currentTaskIndex = 0;
	private bool _started;

	/// <summary>
	/// Initialize the schedule with the Behavior context
	/// </summary>
	internal void InternalInit( Behavior behavior )
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
		if ( _tasks.Count == 0 ) return TaskStatus.Failed;
		if ( _currentTaskIndex >= _tasks.Count ) return TaskStatus.Success;

		var currentTask = _tasks[_currentTaskIndex];
		var status = currentTask.InternalUpdate();

		if ( status is not TaskStatus.Running )
		{
			currentTask.InternalEnd();

			if ( status is TaskStatus.Success )
			{
				_currentTaskIndex++;
				StartCurrentTask();
				return TaskStatus.Running;
			}

			return status;
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

		_currentTaskIndex = 0;
		_started = false;

		OnEnd();
	}

	/// <summary>
	/// Called when this schedule starts -- this is where you can add tasks to run
	/// </summary>
	protected virtual void OnStart() { }

	/// <summary>
	/// Called when this schedule ends -- this is where you can clean stuff up
	/// </summary>
	protected virtual void OnEnd() { }

	/// <summary>
	/// Add a task to the sequence
	/// </summary>
	protected void AddTask( TaskBase task )
	{
		_tasks.Add( task );
	}

	/// <summary>
	/// Start the current task in sequence
	/// </summary>
	private void StartCurrentTask()
	{
		if ( _currentTaskIndex < _tasks.Count )
		{
			var task = _tasks[_currentTaskIndex];
			task.Initialize( this );
		}
	}
}
