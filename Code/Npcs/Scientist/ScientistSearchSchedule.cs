using Sandbox.Npcs.Tasks;

namespace Sandbox.Npcs.Schedules;

public class ScientistSearchSchedule : ScheduleBase
{
	private Vector3 _lastKnown;

	public ScientistSearchSchedule( Vector3 last )
	{
		_lastKnown = last;
	}

	protected override void OnStart()
	{
		// Turn towards the last known position
		AddTask( new LookAt( _lastKnown ) );
		
		// Hold for a moment
		AddTask( new Wait( 1f ) );
		
		// Generate search positions, have a look around
		var searchPositions = GenerateSearchPositions();
		foreach ( var position in searchPositions )
		{
			AddTask( new LookAt( position ) );
			AddTask( new Wait( 0.8f ) );
		}
		
		// Wait some more
		AddTask( new Wait( 2f ) );
	}

	/// <summary>
	/// Generate positions to look at while searching
	/// </summary>
	private List<Vector3> GenerateSearchPositions()
	{
		var positions = new List<Vector3>();
		var npcPos = Npc.WorldPosition;
		var searchRadius = 200f;
		
		// Look in direction of last known position first
		var directionToTarget = (_lastKnown - npcPos).Normal;
		positions.Add( npcPos + directionToTarget * searchRadius );
		
		// Look 45 degrees left and right of that direction
		var leftDirection = directionToTarget * Rotation.FromYaw( -45f );
		var rightDirection = directionToTarget * Rotation.FromYaw( 45f );
		
		positions.Add( npcPos + leftDirection * searchRadius );
		positions.Add( npcPos + rightDirection * searchRadius );
		
		// Look behind (in case they moved around)
		var behindDirection = directionToTarget * Rotation.FromYaw( 180f );
		positions.Add( npcPos + behindDirection * searchRadius );
		
		return positions;
	}

	protected override void OnEnd()
	{
		Log.Info( $"Scientist finished searching for target" );
	}
}
