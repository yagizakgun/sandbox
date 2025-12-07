namespace Sandbox.Npcs.Layers;

/// <summary>
/// Handles NPC head/eye tracking - controlled by LookAt tasks
/// </summary>
public class LookAtLayer : BehaviorLayer
{
	public Vector3? LookTarget { get; private set; }
	public float LookSpeed { get; set; } = 8f;
	public float MaxHeadAngle { get; set; } = 45f; // When to turn body vs just head

	protected override void OnUpdate()
	{
		if ( LookTarget.HasValue && Npc.IsValid() )
		{
			UpdateLookDirection( LookTarget.Value );
		}
	}

	/// <summary>
	/// Command this layer to look at a target (called by LookAt task)
	/// </summary>
	public void LookAt( Vector3 target )
	{
		LookTarget = target;
	}

	/// <summary>
	/// Stop looking (called by tasks)
	/// </summary>
	public void StopLooking()
	{
		LookTarget = null;
		Npc?.ClearHeadTarget();
		Npc?.ClearEyeTarget();
	}

	/// <summary>
	/// Check if we're facing the target
	/// </summary>
	public bool IsFacingTarget()
	{
		if ( !LookTarget.HasValue || !Npc.IsValid() ) return true;

		var direction = (LookTarget.Value - Npc.WorldPosition).Normal;
		var dot = Npc.WorldRotation.Forward.Dot( direction );
		return dot > 0.90f;
	}

	/// <summary>
	/// Update look direction - handles both head tracking and body rotation
	/// </summary>
	private void UpdateLookDirection( Vector3 targetPosition )
	{
		var direction = (targetPosition - Npc.WorldPosition).Normal;
		var currentForward = Npc.WorldRotation.Forward;
		var angleToTarget = MathF.Acos( Vector3.Dot( currentForward, direction ) ) * (180f / MathF.PI);

		// If angle is too large, rotate body
		if ( angleToTarget > MaxHeadAngle )
		{
			var targetRotation = Rotation.LookAt( direction );
			var lerpSpeed = LookSpeed * Time.Delta;
			var newRotation = Rotation.Lerp( Npc.WorldRotation, targetRotation, lerpSpeed );
			Npc.SetBodyTarget( newRotation );
		}

		// Always set head/eye targets for fine tracking
		var localDirection = Npc.WorldRotation.Inverse * direction;
		Npc.SetHeadTarget( localDirection );
		Npc.SetEyeTarget( localDirection );
	}

	public override void Reset()
	{
		StopLooking();
	}
}
