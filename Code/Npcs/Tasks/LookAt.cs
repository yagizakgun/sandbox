namespace Sandbox.Npcs.Tasks;

public class LookAt : TaskBase
{
	public Vector3? TargetPosition { get; set; }
	public GameObject TargetObject { get; set; }
	public float Speed { get; set; } = 8f;
	public bool Once { get; set; } = true;

	public LookAt( Vector3 targetPosition, float speed = 8f )
	{
		TargetPosition = targetPosition;
		Speed = speed;
	}

	public LookAt( GameObject gameObject, float speed = 8f )
	{
		TargetObject = gameObject;
		Speed = speed;
		Once = false; 
	}

	public override async Task Execute()
	{
		if ( !Once )
		{
			// Continuous tracking - never completes, only stops when cancelled
			while ( !IsCancelled )
			{
				UpdateLookRotation();
				await FrameEnd();
			}
		}
		else
		{
			// One-time look - completes when aligned
			while ( !IsLookingAtTarget() && !IsCancelled )
			{
				UpdateLookRotation();
				await FrameEnd();
			}
		}
	}

	private void UpdateLookRotation()
	{
		var targetPos = GetTargetPosition();
		if ( !targetPos.HasValue ) return;

		var direction = (targetPos.Value - Npc.WorldPosition).Normal;
		var targetRotation = Rotation.LookAt( direction );

		var lerpSpeed = Speed * Time.Delta;
		Npc.SetBodyTarget( Rotation.Lerp( Npc.WorldRotation, targetRotation, lerpSpeed ) );
	}

	private bool IsLookingAtTarget()
	{
		var targetPos = GetTargetPosition();
		if ( !targetPos.HasValue ) return true;

		var direction = (targetPos.Value - Npc.WorldPosition).Normal;
		var targetRotation = Rotation.LookAt( direction );

		return Npc.WorldRotation.Forward.Dot( targetRotation.Forward ) > 0.999f;
	}

	private Vector3? GetTargetPosition()
	{
		if ( TargetObject.IsValid() ) return TargetObject.WorldPosition;
		if ( TargetPosition.HasValue ) return TargetPosition.Value;
		return null;
	}
}
