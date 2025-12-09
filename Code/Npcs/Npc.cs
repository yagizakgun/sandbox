namespace Sandbox.Npcs;

[Title( "NPC" ), Group( "NPCs" ), Icon( "android" )]
public sealed class Npc : Component
{
	public Vector3? HeadTarget { get; private set; }
	public Vector3? EyeTarget { get; private set; }

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

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
}
