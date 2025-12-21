namespace Sandbox.Npcs;

public partial class Npc : Component
{
	/// <summary>
	/// The current running schedule for this NPC.
	/// </summary>
	public ScheduleBase ActiveSchedule { get; private set; }

	readonly Dictionary<Type, ScheduleBase> _schedules = [];

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

	public virtual ScheduleBase GetSchedule()
	{
		return null;
	}

	/// <summary>
	/// Updates a behavior, returns if there is an active schedule - this will stop lower priority behaviors from running
	/// </summary>
	bool TickSchedule()
	{


		var newSchedule = GetSchedule();

		if ( ShouldStartSchedule( newSchedule ) )
		{
			EndCurrentSchedule();

			ActiveSchedule = newSchedule;
			ActiveSchedule?.InternalInit( this );
			ActiveSchedule?.InternalStart();
		}

		if ( ActiveSchedule is not null )
		{
			RunActiveSchedule();
		}




		return ActiveSchedule is not null;
	}

	private void RunActiveSchedule()
	{
		if ( ActiveSchedule.InternalUpdate() is not TaskStatus.Running )
		{
			EndCurrentSchedule();
		}
	}

	private bool ShouldStartSchedule( ScheduleBase newSchedule )
	{
		if ( newSchedule is null )
			return false;

		if ( ActiveSchedule is null )
			return true;

		return newSchedule != ActiveSchedule;
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
		ActiveSchedule?.InternalEnd();
		ActiveSchedule = null;
	}
}
