namespace Sandbox.Npcs.Schedules;

/// <summary>
/// Schedule to move a npc away from a threat
/// </summary>
public class FleeSchedule : ScheduleBase
{
	private GameObject _threatTarget;
	private float _fleeDistance;

	public FleeSchedule( GameObject threatTarget, float fleeDistance = 512f )
	{
		_threatTarget = threatTarget;
		_fleeDistance = fleeDistance;
	}

	public override async Task Execute()
	{
		if ( !_threatTarget.IsValid() )
			return;

		// Find a position away from the threat
		var escape = FindEscapePosition();

		if ( escape.HasValue )
		{
			// Move to the escape position
			await ExecuteTask( new MoveTo( escape.Value )
				.CancelWhen( "threat-gone" )
				.CancelWhen( "new-threat" ) );
		}
		else
		{
			// Can't find escape position, just try to move in opposite direction
			var awayDirection = (Npc.WorldPosition - _threatTarget.WorldPosition).Normal;
			var fallbackPosition = Npc.WorldPosition + awayDirection * _fleeDistance;

			await ExecuteTask( new MoveTo( fallbackPosition )
				.CancelWhen( "threat-gone" )
				.CancelWhen( "new-threat" ) );
		}
	}

	/// <summary>
	/// Find a suitable position to flee to
	/// </summary>
	private Vector3? FindEscapePosition()
	{
		var threatPos = _threatTarget.WorldPosition;
		var npcPos = Npc.WorldPosition;

		// Calculate direction away from threat
		var awayDirection = (npcPos - threatPos).Normal;

		// Try multiple positions at increasing distances
		for ( int distance = 100; distance <= _fleeDistance; distance += 50 )
		{
			// Try straight away
			var testPos = npcPos + awayDirection * distance;
			if ( IsValidEscapePosition( testPos ) )
				return testPos;

			// Try 45 degrees left and right
			for ( float angle = -45f; angle <= 45f; angle += 45f )
			{
				var rotatedDirection = awayDirection * Rotation.FromYaw( angle );
				testPos = npcPos + rotatedDirection * distance;

				if ( IsValidEscapePosition( testPos ) )
					return testPos;
			}
		}

		return null;
	}

	/// <summary>
	/// Check if a position is suitable for escaping
	/// </summary>
	private bool IsValidEscapePosition( Vector3 position )
	{
		// Is it reachable?
		var path = Game.ActiveScene.NavMesh.CalculatePath( new Navigation.CalculatePathRequest()
		{
			Start = Npc.WorldPosition,
			Target = position
		} );

		if ( !path.IsValid() || path.Points.Count < 1 )
			return false;

		var distanceFromThreat = position.Distance( _threatTarget.WorldPosition );
		return distanceFromThreat >= _fleeDistance;
	}

	protected override async Task OnTaskCancelled( TaskBase task, string condition, bool wasConditionPresent )
	{
		if ( condition == "threat-gone" || condition == "new-threat" )
		{
			// the terms have changed, cancel -- re-evaluate if needed
			Cancel();
		}
	}
}
