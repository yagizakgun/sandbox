using Sandbox.Npcs.Tasks;

namespace Sandbox.Npcs.Schedules;

/// <summary>
/// Schedule to stare at a specific target
/// </summary>
public sealed class StareSchedule : ScheduleBase
{
	private GameObject _target;

	public StareSchedule( GameObject target )
	{
		_target = target;
	}

	public override async Task Execute()
	{
		if ( !_target.IsValid() )
			return;

		await ExecuteTask( new LookAt( _target ).CancelWhenNot( "has-target" ) );
	}
}
