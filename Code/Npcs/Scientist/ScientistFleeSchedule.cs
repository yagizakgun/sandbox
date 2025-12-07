using Sandbox.Npcs.Tasks;

namespace Sandbox.Npcs.Schedules;

/// <summary>
/// Simple flee - just move away quickly, then complete
/// </summary>
public class ScientistFleeSchedule : ScheduleBase
{
	private GameObject _threat;

	public ScientistFleeSchedule( GameObject threat )
	{
		_threat = threat;
	}

	protected override void OnStart()
	{
		if ( !_threat.IsValid() ) return;

		var awayDirection = (Npc.WorldPosition - _threat.WorldPosition).Normal;
		var fleeTarget = Npc.WorldPosition + awayDirection * 150f;

		AddTask( new MoveTo( fleeTarget ) );
	}
}
