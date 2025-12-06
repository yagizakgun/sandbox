using Sandbox.Npcs.Schedules;

namespace Sandbox.Npcs.Behaviors;

/// <summary>
/// Neutral NPC behavior - looks around when idle, stares at nearby players, flees if they get too close
/// </summary>
public class NeutralBehavior : Behavior
{
	[Property] public float DetectionRadius { get; set; } = 500f;
	[Property] public float PersonalSpaceRadius { get; set; } = 100f;
	[Property] public float FleeDistance { get; set; } = 250f;
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
		bool targetTooClose = false;

		foreach ( var obj in nearbyObjects )
		{
			if ( !obj.Tags.HasAny( TargetTags ) )
				continue;

			var distance = Npc.WorldPosition.Distance( obj.WorldPosition );

			// Check if this target is too close (within personal space)
			if ( distance <= PersonalSpaceRadius )
			{
				targetTooClose = true;
				if ( distance < closestDistance )
				{
					closestDistance = distance;
					closestTarget = obj;
				}
			}
			// Only consider targets with line of sight for watching behavior
			else if ( HasLineOfSight( obj ) && distance < closestDistance )
			{
				closestDistance = distance;
				closestTarget = obj;
			}
		}

		_currentTarget = closestTarget;

		// Update conditions based on what we found
		Conditions.Set( "has-target", _currentTarget.IsValid() );
		Conditions.Set( "player-too-close", targetTooClose );

		if ( targetTooClose )
		{
			Conditions.Set( "threat-gone", false );
		}
		else if ( Conditions.Contains( "player-too-close" ) )
		{
			// We had a threat but now we don't
			Conditions.Set( "player-too-close", false );
			Conditions.Set( "threat-gone", true );
		}
	}

	private bool HasLineOfSight( GameObject target )
	{
		// TODO: eye target interface
		var trace = Scene.Trace.Ray( Npc.WorldPosition + Vector3.Up * 64, target.WorldPosition + Vector3.Up * 64 )
			.IgnoreGameObjectHierarchy( Npc.GameObject )
			.WithoutTags( "trigger" )
			.Run();

		return !trace.Hit || trace.GameObject == target || target.IsDescendant( trace.GameObject );
	}

	public override ScheduleBase QuerySchedule()
	{
		if ( Conditions.Contains( "player-too-close" ) && _currentTarget.IsValid() )
		{
			return new FleeSchedule( _currentTarget, FleeDistance );
		}

		if ( Conditions.Contains( "has-target" ) && _currentTarget.IsValid() )
		{
			return new LookAtSchedule( _currentTarget );
		}

		return null;
	}
}
