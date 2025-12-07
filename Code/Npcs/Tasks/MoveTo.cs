using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs.Tasks;

/// <summary>
/// Task that commands the LocomotionLayer to move to a target
/// </summary>
public class MoveTo : TaskBase
{
	public Vector3 TargetPosition { get; set; }
	public float StopDistance { get; set; } = 10f;

	private LocomotionLayer _locomotion;

	public MoveTo( Vector3 targetPosition, float stopDistance = 10f )
	{
		TargetPosition = targetPosition;
		StopDistance = stopDistance;
	}

	protected override void OnStart()
	{
		_locomotion = GetLayer<LocomotionLayer>();

		if ( _locomotion is not null )
		{
			_locomotion.MoveTo( TargetPosition, StopDistance );
		}
	}

	protected override TaskStatus OnUpdate()
	{
		if ( _locomotion is null )
			return TaskStatus.Failed;

		// Check if we've reached the target
		return _locomotion.HasReachedTarget() ? TaskStatus.Success : TaskStatus.Running;
	}
}
