namespace Sandbox.Npcs.Tasks;

public class LookAt : TaskBase
{
	public Vector3? TargetPosition { get; set; }
	public GameObject TargetObject { get; set; }
	public float Speed { get; set; } = 180f;

	public LookAt( Vector3 targetPosition, float speed = 180f )
	{
		TargetPosition = targetPosition;
		Speed = speed;
	}

	public LookAt( GameObject gameObject, float speed = 180f )
	{
		TargetObject = gameObject;
		Speed = speed;
	}

	public override async Task Execute()
	{
		while ( !IsLookingAtTarget() )
		{
			var targetPos = GetTargetPosition();
			if ( !targetPos.HasValue ) return;

			var direction = (targetPos.Value - Npc.WorldPosition).Normal;
			var targetRotation = Rotation.LookAt( direction );

			var lerpSpeed = Speed * Time.Delta / 180f;
			Npc.SetBodyTarget( Rotation.Lerp( Npc.WorldRotation, targetRotation, lerpSpeed ) );

			await FrameEnd();
		}
	}

	private bool IsLookingAtTarget()
	{
		var targetPos = GetTargetPosition();
		if ( !targetPos.HasValue ) return true;

		var direction = (targetPos.Value - Npc.WorldPosition).Normal;
		var targetRotation = Rotation.LookAt( direction );

		return Npc.WorldRotation.Forward.Dot( targetRotation.Forward ) > 0.999f; // whatever
	}

	private Vector3? GetTargetPosition()
	{
		if ( TargetObject.IsValid() ) return TargetObject.WorldPosition;
		if ( TargetPosition.HasValue ) return TargetPosition.Value;
		return null;
	}
}
