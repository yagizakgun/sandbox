using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs.Tasks;

/// <summary>
/// Tells the LookAtLayer to face a target position or object
/// </summary>
public class LookAt : TaskBase
{
	public Vector3? TargetPosition { get; set; }
	public GameObject TargetObject { get; set; }

	private LookAtLayer _lookAt;

	public LookAt( Vector3 targetPosition )
	{
		TargetPosition = targetPosition;
	}

	public LookAt( GameObject gameObject )
	{
		TargetObject = gameObject;
	}

	protected override void OnStart()
	{
		_lookAt ??= GetLayer<LookAtLayer>();
	}

	protected override TaskStatus OnUpdate()
	{
		if ( _lookAt is null )
			return TaskStatus.Failed;

		var targetPos = GetTargetPosition();
		if ( !targetPos.HasValue )
			return TaskStatus.Failed;

		_lookAt.LookAt( targetPos.Value );

		return _lookAt.IsFacingTarget() ? TaskStatus.Success : TaskStatus.Running;
	}

	private Vector3? GetTargetPosition()
	{
		if ( TargetObject.IsValid() ) return TargetObject.WorldPosition;
		return TargetPosition;
	}
}
