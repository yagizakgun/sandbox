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
	/// The current schedule
	/// </summary>
	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	public ScheduleBase CurrentSchedule { get; private set; }

	/// <summary>
	/// Is this behavior running a schedule right now?
	/// </summary>
	public bool IsScheduleStarted { get; private set; }

	[Property, JsonIgnore, ReadOnly, Group( "Debug" )]
	protected Dictionary<Type, ScheduleBase> _schedules = new();

	/// <summary>
	/// Get a layer -- if it doesn't exist, one will be created
	/// </summary>
	public T GetLayer<T>() where T : BehaviorLayer, new()
	{
		return GetOrAddComponent<T>();
	}

	/// <summary>
	/// Get a schedule -- if it doesn't exist, one will be created
	/// </summary>
	protected T GetSchedule<T>() where T : ScheduleBase, new()
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
	internal bool InternalUpdate( Npc npc )
	{
		var newSchedule = Run();

		if ( ShouldStartSchedule( newSchedule ) )
		{
			EndCurrentSchedule();

			CurrentSchedule = newSchedule;
			CurrentSchedule?.InternalInit( this );
			CurrentSchedule?.InternalStart();
			IsScheduleStarted = true;
		}

		CurrentSchedule?.OnUpdate();

		if ( CurrentSchedule is not null && CurrentSchedule.InternalUpdate() is not TaskStatus.Running )
		{
			EndCurrentSchedule();
		}

		return CurrentSchedule is not null;
	}

	private bool ShouldStartSchedule( ScheduleBase newSchedule )
	{
		if ( newSchedule is null )
			return false;

		if ( CurrentSchedule is null )
			return true;

		return newSchedule != CurrentSchedule;
	}

	protected override void OnDisabled()
	{
		EndCurrentSchedule();
	}

	/// <summary>
	/// End the current schedule cleanly
	/// </summary>
	private void EndCurrentSchedule()
	{
		CurrentSchedule?.InternalEnd();
		CurrentSchedule = null;
	}
}
