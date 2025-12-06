namespace Sandbox.Npcs;

/// <summary>
/// A behavior for an NPC
/// </summary>
public abstract class Behavior : Component
{
	public Npc Npc { get; private set; }

	public Conditions Conditions => Npc.Conditions;

	[Property] public int Priority { get; set; } = 0;

	private ScheduleBase _currentSchedule;

	/// <summary>
	/// Get the TaskSource from this component - allows passing to tasks for lifetime management
	/// </summary>
	internal TaskSource GetTaskSource() => Task;

	/// <summary>
	/// Npc calls this every tick to update this behavior -- returns true if we're running a schedule
	/// </summary>
	internal bool Update( Npc npc )
	{
		Npc = npc;

		// Check if we need a new schedule
		if ( _currentSchedule == null || _currentSchedule.IsCancelled )
		{
			var newSchedule = QuerySchedule();
			if ( newSchedule != null )
			{
				SwitchToSchedule( newSchedule );
				return true;
			}
			return false;
		}

		return true;
	}

	/// <summary>
	/// Cancel this behavior's current schedule
	/// </summary>
	internal void Cancel()
	{
		_currentSchedule?.Cancel();
	}

	/// <summary>
	/// Query for a schedule - implement this in derived classes
	/// Return null if this behavior doesn't want to run right now
	/// </summary>
	public abstract ScheduleBase QuerySchedule();

	/// <summary>
	/// Switch to a new schedule
	/// </summary>
	private async void SwitchToSchedule( ScheduleBase newSchedule )
	{
		// Cancel current schedule
		_currentSchedule?.Cancel();

		// Start new schedule
		_currentSchedule = newSchedule;
		_currentSchedule.Initialize( this );

		try
		{
			await _currentSchedule.ExecuteWithCancellation();
		}
		catch ( OperationCanceledException )
		{
			// Schedule was cancelled, this is normal
		}
		catch ( TaskCancelledException )
		{
			// Task-specific cancellation, also normal
		}
		catch ( Exception ex )
		{
			Log.Error( $"Error executing schedule {newSchedule.GetType().Name} in behavior {GetType().Name}: {ex}" );
		}
		finally
		{
			if ( _currentSchedule == newSchedule )
			{
				_currentSchedule = null;
			}
		}
	}
}
