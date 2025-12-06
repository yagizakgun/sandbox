namespace Sandbox.Npcs;

/// <summary>
/// Move to a location
/// </summary>
public sealed class MoveTo : TaskBase
{
	public Vector3 TargetPosition { get; set; }
	public float StopDistance { get; set; } = 10f;

	public MoveTo( Vector3 targetPosition, float stopDistance = 10f )
	{
		TargetPosition = targetPosition;
		StopDistance = stopDistance;
	}

	public override async Task Execute()
	{
		Npc.Agent.MoveTo( TargetPosition );

		while ( Npc.WorldPosition.Distance( TargetPosition ) > StopDistance )
		{
			await FrameEnd();
		}
	}
}
