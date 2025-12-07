namespace Sandbox.Npcs.Layers;

/// <summary>
/// Base class for NPC behavior layers - NO task management, just state
/// </summary>
public abstract class BehaviorLayer
{
	protected Npc Npc { get; private set; }

	/// <summary>
	/// Initialize the layer with NPC context
	/// </summary>
	internal void Initialize( Npc npc )
	{
		Npc = npc;
	}

	/// <summary>
	/// Update this layer - called every frame
	/// </summary>
	internal void Update()
	{
		OnUpdate();
	}

	/// <summary>
	/// Layer-specific update logic - override in derived classes
	/// </summary>
	protected virtual void OnUpdate()
	{
	}

	/// <summary>
	/// Reset this layer to default state
	/// </summary>
	public virtual void Reset()
	{
	}
}
