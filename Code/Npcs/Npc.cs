namespace Sandbox.Npcs;

public sealed class Npc : Component
{
	[RequireComponent]
	public NavMeshAgent Agent { get; private set; }

	public Conditions Conditions { get; } = new();

	public Vector3? HeadTarget { get; private set; }
	public Vector3? EyeTarget { get; private set; }

	protected override void OnDisabled()
	{
		foreach ( var behavior in GetComponents<Behavior>() )
		{
			behavior.Cancel();
		}
	}

	protected override void OnUpdate()
	{
		//
		// Iterate through behaviors by priority - stop at first one that's running
		//
		var behaviors = GetComponents<Behavior>().OrderByDescending( b => b.Priority );

		foreach ( var behavior in behaviors )
		{
			if ( behavior.Update( this ) )
			{
				break;
			}
		}
	}

	/// <summary>
	/// Rotate the body
	/// </summary>
	public void SetBodyTarget( Rotation rotation )
	{
		var angles = rotation.Angles();
		angles.pitch = 0;
		WorldRotation = angles.ToRotation();
	}

	/// <inheritdoc cref="SetBodyTarget(Rotation)" />
	public void SetBodyTarget( Vector3 direction )
	{
		var rotation = Rotation.LookAt( direction );
		SetBodyTarget( rotation );
	}

	/// <summary>
	/// Set eye aim direction
	/// </summary>
	public void SetEyeTarget( Vector3 aimDirection )
	{
		EyeTarget = aimDirection;
	}

	/// <summary>
	/// Set head aim direction
	/// </summary>
	public void SetHeadTarget( Vector3 aimDirection )
	{
		HeadTarget = aimDirection;
	}

	/// <summary>
	/// Clear eye target
	/// </summary>
	public void ClearEyeTarget()
	{
		EyeTarget = null;
	}

	/// <summary>
	/// Clear head target
	/// </summary>
	public void ClearHeadTarget()
	{
		HeadTarget = null;
	}
}
