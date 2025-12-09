namespace Sandbox.Npcs;

[Title( "NPC" ), Group( "NPCs" ), Icon( "android" )]
public sealed class Npc : Component
{
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
			if ( behavior.InternalUpdate( this ) )
			{
				break;
			}
		}
	}
}
