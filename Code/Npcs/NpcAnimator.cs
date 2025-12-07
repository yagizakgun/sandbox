namespace Sandbox.Npcs;

/// <summary>
/// Simple npc animator -- uses Citizen anim parameters
/// </summary>
[Title( "NPC Animator" ), Icon( "accessibility" ), Group( "NPCs" )]
public sealed class NpcAnimator : Component
{
	[RequireComponent]
	Npc Npc { get; set; }

	[Property]
	SkinnedModelRenderer Target { get; set; }

	NavMeshAgent NavMeshAgent => Npc.Agent;

	float _currentRotationSpeed;
	Rotation _lastRotation;

	protected override void OnUpdate()
	{
		if ( !Target.IsValid() || !NavMeshAgent.IsValid() )
			return;

		UpdateMovementAnimation();
		UpdateLookTargets();
		UpdateRotationSpeed();
	}

	/// <summary>
	/// Update movement-related animation parameters
	/// </summary>
	void UpdateMovementAnimation()
	{
		var vel = NavMeshAgent.Velocity;

		// Convert world velocity to local movement values
		var forward = Npc.WorldRotation.Forward.Dot( vel );
		var side = Npc.WorldRotation.Right.Dot( vel );

		Target.Set( "move_x", forward );
		Target.Set( "move_y", side );
		Target.Set( "move_speed", vel.Length );
		Target.Set( "move_rotationspeed", _currentRotationSpeed );
	}

	/// <summary>
	/// Update head and eye look targets
	/// </summary>
	void UpdateLookTargets()
	{
		if ( Npc.HeadTarget.HasValue )
		{
			Target.Set( "aim_head", Npc.HeadTarget.Value );
		}

		if ( Npc.EyeTarget.HasValue )
		{
			Target.Set( "aim_eyes", Npc.EyeTarget.Value );
		}
	}

	/// <summary>
	/// Calculate rotation speed for animation
	/// </summary>
	private void UpdateRotationSpeed()
	{
		var currentRotation = Npc.WorldRotation;

		if ( _lastRotation != default )
		{
			var rotationDiff = Rotation.Difference( _lastRotation, currentRotation );
			var yawDiff = rotationDiff.Angles().yaw;
			_currentRotationSpeed = yawDiff / Time.Delta;
		}

		_lastRotation = currentRotation;
	}
}
