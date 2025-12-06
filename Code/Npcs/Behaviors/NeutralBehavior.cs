using Sandbox.Npcs.Schedules;

namespace Sandbox.Npcs.Behaviors;

/// <summary>
/// Neutral NPC behavior - looks around when idle, stares at nearby players
/// </summary>
public class NeutralBehavior : Behavior
{
	[Property] public float DetectionRadius { get; set; } = 500f;
	[Property] public TagSet TargetTags { get; set; } = new() { "player" };

	private GameObject _currentTarget;
	private TimeSince _lastTargetScan;

	protected override void OnUpdate()
	{
		// Scan for targets every half second, this is shittty and temporary!!!!!!
		if ( _lastTargetScan > 0.5f )
		{
			Scan();
			_lastTargetScan = 0;
		}

		base.OnUpdate();
	}

	private void Scan()
	{
		var nearbyObjects = Scene.FindInPhysics( new Sphere( Npc.WorldPosition, DetectionRadius ) );

		GameObject closestTarget = null;
		float closestDistance = float.MaxValue;

		foreach ( var obj in nearbyObjects )
		{
			if ( !obj.Tags.HasAny( TargetTags ) )
				continue;

			var distance = Npc.WorldPosition.Distance( obj.WorldPosition );

			if ( HasLineOfSight( obj ) && distance < closestDistance )
			{
				closestDistance = distance;
				closestTarget = obj;
			}
		}

		_currentTarget = closestTarget;

		// Update conditions
		Conditions.Set( "has-target", _currentTarget.IsValid() );
	}

	private bool HasLineOfSight( GameObject target )
	{
		var trace = Scene.Trace.Ray( Npc.WorldPosition + Vector3.Up * 64, target.WorldPosition + Vector3.Up * 64 )
			.IgnoreGameObjectHierarchy( Npc.GameObject )
			.WithoutTags( "trigger" )
			.Run();

		return !trace.Hit || trace.GameObject == target || target.IsDescendant( trace.GameObject );
	}

	public override ScheduleBase QuerySchedule()
	{
		// If we have a target, stare at them
		if ( Conditions.Contains( "has-target" ) && _currentTarget.IsValid() )
		{
			return new LookAtSchedule( _currentTarget );
		}

		return null;
	}
}
