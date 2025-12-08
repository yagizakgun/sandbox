namespace Sandbox.Npcs.Layers;

/// <summary>
/// A behavior layer provides specific services for tasks to use -- we don't use behavior layers for state, they are services.
/// </summary>
public abstract class BehaviorLayer
{
	/// <summary>
	/// The behavior this layer belongs to
	/// </summary>
	internal Behavior Behavior { get; set; }

	/// <inheritdoc cref="Behavior.Npc"/>
	protected Npc Npc => Behavior.Npc;

	/// <summary>
	/// Update this layer - called every frame
	/// </summary>
	internal void Update()
	{
		OnUpdate();
	}

	/// <summary>
	/// Called every update while the behavior is ticking
	/// </summary>
	protected virtual void OnUpdate()
	{
	}

	/// <summary>
	/// Reset this layer to default
	/// </summary>
	public virtual void Reset()
	{
	}
}
