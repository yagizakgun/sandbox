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

	/// <summary>
	/// The Npc we're attached to
	/// </summary>
	public Npc Npc { get; private set; }

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected ScheduleBase _currentSchedule;

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected Dictionary<Type, BehaviorLayer> _layers = new();

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected Dictionary<Type, ScheduleBase> _schedules = new();

	private bool _scheduleStarted;

	/// <summary>
	/// Get a layer -- if it doesn't exist, one will be created
	/// </summary>
	public T Layer<T>() where T : BehaviorLayer, new()
	{
		var type = typeof( T );
		if ( !_layers.TryGetValue( type, out var layer ) )
		{
			layer = new T()
			{
				Behavior = this
			};

			_layers[type] = layer;
		}
		return (T)layer;
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

	protected override void OnStart()
	{
		Npc = GetComponentInParent<Npc>();
	}

	/// <summary>
	/// Query for a schedule.
	/// Return null if this behavior doesn't want to run right now
	/// </summary>
	public abstract ScheduleBase QuerySchedule();

	/// <summary>
	/// Updates a behavior, returns if there is an active schedule - this will stop lower priority behaviors from running
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	internal bool Update( Npc npc )
	{
		UpdateSchedule();

		// Always update layers (they provide continuous services)
		foreach ( var layer in _layers.Values )
		{
			layer.Update();
		}

		return _currentSchedule is not null;
	}

	/// <summary>
	/// Update the current schedule
	/// </summary>
	private void UpdateSchedule()
	{
		// check for higher priority schedules - even if current one is running
		var newSchedule = QuerySchedule();

		// if it's higher priority - interrupt current
		if ( _currentSchedule is not null && newSchedule is not null && newSchedule != _currentSchedule )
		{
			EndCurrentSchedule();
			StartSchedule( newSchedule );
			return;
		}

		// No current schedule? Start new
		if ( _currentSchedule is null && newSchedule is not null )
		{
			StartSchedule( newSchedule );
			return;
		}

		// Update current schedule
		if ( _currentSchedule is not null )
		{
			if ( !_scheduleStarted )
			{
				_currentSchedule.InternalStart();
				_scheduleStarted = true;
			}

			var status = _currentSchedule.OnUpdate();

			switch ( status )
			{
				case TaskStatus.Success:
				case TaskStatus.Failed:
				case TaskStatus.Interrupted:
					EndCurrentSchedule();
					break;
				case TaskStatus.Running:
					// Continue running
					break;
			}
		}
	}

	/// <summary>
	/// Cancel this behavior
	/// </summary>
	internal void Cancel()
	{
		EndCurrentSchedule();

		// Reset all layers to default state
		foreach ( var layer in _layers.Values )
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
