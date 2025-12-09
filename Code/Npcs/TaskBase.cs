using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs;

/// <summary>
/// A task
/// </summary>
public abstract class TaskBase
{
	protected ScheduleBase Schedule { get; private set; }
	protected Behavior Behavior => Schedule.Behavior;
	protected Npc Npc => Schedule.Behavior.Npc;

	private TaskStatus _currentStatus;

	/// <summary>
	/// What is the current status of this task?
	/// </summary>
	protected TaskStatus Status
	{
		get => _currentStatus;
	}

	/// <inheritdoc cref="Behavior.GetLayer"/>
	protected T GetLayer<T>() where T : BehaviorLayer, new() => Behavior?.GetLayer<T>();

	internal void Initialize( ScheduleBase schedule )
	{
		Schedule = schedule;
		InternalStart();
	}

	private void InternalStart()
	{
		_currentStatus = TaskStatus.Running;
		OnStart();
	}

	internal TaskStatus InternalUpdate()
	{
		var status = OnUpdate();
		_currentStatus = status;

		return status;
	}

	internal void InternalEnd()
	{
		if ( _currentStatus == TaskStatus.Running )
		{
			_currentStatus = TaskStatus.Success;
		}

		OnEnd();
		Reset();
	}

	protected virtual void OnStart() { }
	protected abstract TaskStatus OnUpdate();
	protected virtual void OnEnd() { }
	protected virtual void Reset() { }
}
