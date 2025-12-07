namespace Sandbox.Npcs.Layers;

/// <summary>
/// Handles NPC movement - controlled by MoveTo tasks
/// </summary>
public class LocomotionLayer : BehaviorLayer
{
	public NavMeshAgent Agent => Npc?.Agent;

	public Vector3? MoveTarget { get; private set; }
	public float StopDistance { get; private set; } = 10f;

	/// <summary>
	/// Command this layer to move to a target
	/// </summary>
	public void MoveTo( Vector3 target, float stopDistance = 10f )
	{
		MoveTarget = target;
		StopDistance = stopDistance;

		if ( Agent.IsValid() )
		{
			Agent.MoveTo( target );
		}
	}

	/// <summary>
	/// Stop movement
	/// </summary>
	public void Stop()
	{
		MoveTarget = null;

		if ( Agent.IsValid() )
		{
			Agent.Stop();
		}
	}

	/// <summary>
	/// Check if we've reached our target
	/// </summary>
	public bool HasReachedTarget()
	{
		if ( !MoveTarget.HasValue || !Npc.IsValid() ) return true;

		var distance = Npc.WorldPosition.Distance( MoveTarget.Value );
		return distance <= StopDistance;
	}

	public bool IsMoving => Agent.IsValid() && Agent.IsNavigating;

	public override void Reset()
	{
		Stop();
	}
}
