using Sandbox.Npcs.Tasks;

namespace Sandbox.Npcs.Schedules;

public class ScientistInvestigateSchedule : ScheduleBase
{
	private GameObject _target;

	public ScientistInvestigateSchedule( GameObject target )
	{
		_target = target;
	}

	protected override void OnStart()
	{
		if ( !_target.IsValid() ) return;

		// Turn to face the target
		AddTask( new LookAt( _target ) );

		// Wait a bit
		AddTask( new Wait( 1.5f ) );
	}
}
