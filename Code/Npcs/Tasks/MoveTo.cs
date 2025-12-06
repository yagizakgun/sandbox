namespace Sandbox.Npcs;

/// <summary>
/// Move to a location
/// </summary>
public class MoveTo : TaskBase
{
	public Vector3 TargetPosition { get; set; }
	public float StopDistance { get; set; } = 10f;

	private NavMeshAgent _agent;

	public MoveTo( Vector3 targetPosition, float stopDistance = 10f )
	{
		TargetPosition = targetPosition;
		StopDistance = stopDistance;
	}

	public override async Task Execute()
	{
		_agent = Npc.GetOrAddComponent<NavMeshAgent>();
		_agent.MoveTo( TargetPosition );

		// Wait until we reach the target
		while ( Npc.WorldPosition.Distance( TargetPosition ) > StopDistance )
		{
			await FrameEnd();
		}
	}
}
