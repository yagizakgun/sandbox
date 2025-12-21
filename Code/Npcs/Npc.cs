using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs;

[Title( "NPC" ), Group( "NPCs" ), Icon( "android" )]
public sealed class Npc : Component
{
	/// <summary>
	/// Senses layer - handles environmental awareness and target detection
	/// </summary>
	public SensesLayer Senses { get; private set; }

	/// <summary>
	/// Navigation layer - handles pathfinding and movement
	/// </summary>
	public NavigationLayer Navigation { get; private set; }

	/// <summary>
	/// Animation layer - handles look-at and animation parameters
	/// </summary>
	public AnimationLayer Animation { get; private set; }



	public Npc()
	{
		Senses = AddLayer<SensesLayer>();
		Navigation = AddLayer<NavigationLayer>();
		Animation = AddLayer<AnimationLayer>();
	}

	readonly List<BaseNpcLayer> _layers = new();

	protected T AddLayer<T>() where T : BaseNpcLayer, new()
	{
		var layer = new T();
		layer.Initialize( this );
		_layers.Add( layer );
		return layer;
	}

	protected override void OnStart()
	{
		base.OnStart();

		foreach ( var layer in _layers )
		{
			layer.InternalOnStart();
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		// Update layers first
		Senses.Update();
		Navigation.Update();
		Animation.Update();

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
