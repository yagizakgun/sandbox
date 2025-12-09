using Sandbox.Npcs.Layers;
using System.Text.Json.Serialization;

namespace Sandbox.Npcs;

/// <summary>
/// A behavior for an NPC - manages schedules, layers provide services to tasks
/// </summary>
[Icon( "view_kanban" ), Group( "NPCs" )]
public abstract class Behavior : Component
{
	/// <summary>
	/// How important is this behavior -- lower priority behaviors will be cancelled if a higher priority one wants to run
	/// </summary>
	[Property, Range( 0, 16 )] public int Priority { get; set; } = 0;

	public Npc Npc => GetComponentInParent<Npc>();

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected ScheduleBase _currentSchedule;

	public ScheduleBase CurrentSchedule => _currentSchedule;

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected Dictionary<Type, ScheduleBase> _schedules = new();

	private bool _scheduleStarted;

	/// <summary>
	/// Get a layer -- if it doesn't exist, one will be created
	/// </summary>
	public T Layer<T>() where T : BehaviorLayer, new()
	{
		var layer = GetOrAddComponent<T>();
		layer.Behavior = this;
		return layer;
	}

	/// <inheritdoc cref="Layer"/>
	protected T AddLayer<T>() where T : BehaviorLayer, new() => Layer<T>();

	/// <summary>
	/// Get a schedule -- if it doesn't exist, one will be created
	/// </summary>
	protected T Schedule<T>() where T : ScheduleBase, new()
	{
		var type = typeof( T );
		if ( !_schedules.TryGetValue( type, out var schedule ) )
		{
			schedule = new T();
			_schedules[type] = schedule;
		}
		return (T)schedule;
	}

	/// <summary>
	/// Run this behavior -- return a schedule to run. This can happily return null if there's nothing to do.
	/// </summary>
	public abstract ScheduleBase Run();

	/// <summary>
	/// Updates a behavior, returns if there is an active schedule - this will stop lower priority behaviors from running
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	internal bool Update( Npc npc )
	{
		UpdateSchedule();

		return _currentSchedule is not null;
	}

	/// <summary>
	/// Update the current schedule
	/// </summary>
	private void UpdateSchedule()
	{
		var newSchedule = Run();

		if ( ShouldStartSchedule( newSchedule ) )
		{
			StartSchedule( newSchedule );
			return;
		}

		UpdateCurrentSchedule();
	}

	private bool ShouldStartSchedule( ScheduleBase newSchedule )
	{
		if ( newSchedule is null )
			return false;

		if ( _currentSchedule is null )
			return true;

		return newSchedule != _currentSchedule;
	}

	/// <summary>
	/// Updates the currently running schedule
	/// </summary>
	private void UpdateCurrentSchedule()
	{
		if ( _currentSchedule is null )
			return;

		if ( !_scheduleStarted )
		{
			_currentSchedule.InternalStart();
			_scheduleStarted = true;
		}

		_currentSchedule.OnUpdate();

		if ( _currentSchedule.InternalUpdate() is not TaskStatus.Running )
		{
			EndCurrentSchedule();
		}
	}

	/// <summary>
	/// Cancel this behavior
	/// </summary>
	internal void Cancel()
	{
		EndCurrentSchedule();

		// Reset all layers to default state
		foreach ( var layer in GetComponentsInChildren<BehaviorLayer>() )
		{
			layer.Reset();
		}
	}

	/// <summary>
	/// Start a new schedule
	/// </summary>
	private void StartSchedule( ScheduleBase newSchedule )
	{
		EndCurrentSchedule();

		_currentSchedule = newSchedule;
		_currentSchedule.InternalInit( this );
		_scheduleStarted = false;
	}

	/// <summary>
	/// End the current schedule cleanly
	/// </summary>
	private void EndCurrentSchedule()
	{
		if ( _currentSchedule is null )
		{
			return;
		}

		if ( _scheduleStarted )
		{
			_currentSchedule.InternalEnd();
		}

		_currentSchedule = null;
		_scheduleStarted = false;
	}
}
