namespace Sandbox.Npcs;

public partial class Npc : Component
{
	[RequireComponent]
	public NavMeshAgent Agent { get; private set; }

	public Conditions Conditions { get; } = new();

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
		var renderer = GetComponentInChildren<SkinnedModelRenderer>();
		if ( renderer.IsValid() )
		{
			renderer.Set( "aim_eyes", aimDirection );
		}
	}

	/// <summary>
	/// Set head aim direction
	/// </summary>
	public void SetHeadTarget( Vector3 aimDirection )
	{
		var renderer = GetComponentInChildren<SkinnedModelRenderer>();
		if ( renderer.IsValid() )
		{
			renderer.Set( "aim_head", aimDirection );
		}
	}
}
