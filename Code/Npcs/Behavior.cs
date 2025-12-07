using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs;

/// <summary>
/// A behavior for an NPC - manages schedules, layers provide services to tasks
/// </summary>
public abstract class Behavior : Component
{
	[Property] public int Priority { get; set; } = 0;

	public Npc Npc { get; private set; }

	protected Dictionary<Type, BehaviorLayer> Layers { get; private set; } = new();

	private ScheduleBase _currentSchedule;
	private bool _scheduleStarted;

	/// <summary>
	/// Add a layer to this behavior
	/// </summary>
	protected T AddLayer<T>() where T : BehaviorLayer, new()
	{
		var layer = new T();
		Layers[typeof( T )] = layer;
		return layer;
	}

	/// <summary>
	/// Get a layer of specific type
	/// </summary>
	public T GetLayer<T>() where T : BehaviorLayer
	{
		return Layers.TryGetValue( typeof( T ), out var layer ) ? layer as T : null;
	}

	/// <summary>
	/// Npc calls this every tick to update this behavior
	/// </summary>
	internal bool Update( Npc npc )
	{
		Npc = npc;

		// Initialize layers if needed
		if ( Npc.IsValid() )
		{
			foreach ( var layer in Layers.Values )
			{
				layer.Initialize( Npc );
			}
		}

		UpdateSchedule();

		// Always update layers (they provide continuous services)
		foreach ( var layer in Layers.Values )
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
		if ( _currentSchedule is not null && newSchedule is not null &&
			 newSchedule.GetType() != _currentSchedule.GetType() )
		{
			Log.Info( $"Interrupting {_currentSchedule} for {newSchedule}" );
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
		foreach ( var layer in Layers.Values )
		{
			layer.Reset();
		}
	}

	/// <summary>
	/// Query for a schedule.
	/// Return null if this behavior doesn't want to run right now
	/// </summary>
	public abstract ScheduleBase QuerySchedule();

	/// <summary>
	/// Start a new schedule
	/// </summary>
	private void StartSchedule( ScheduleBase newSchedule )
	{
		EndCurrentSchedule();
		_currentSchedule = newSchedule;
		_currentSchedule.Initialize( this );
		_scheduleStarted = false;
	}

	/// <summary>
	/// End the current schedule cleanly
	/// </summary>
	private void EndCurrentSchedule()
	{
		if ( _currentSchedule != null )
		{
			if ( _scheduleStarted )
			{
				_currentSchedule.InternalEnd();
			}
			_currentSchedule = null;
			_scheduleStarted = false;
		}
	}
}
