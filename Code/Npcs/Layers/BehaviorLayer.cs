namespace Sandbox.Npcs.Layers;

/// <summary>
/// A behavior layer provides specific services for tasks to use -- we don't use behavior layers for state, they are services.
/// </summary>
public abstract class BehaviorLayer : Component
{
	/// <summary>
	/// The behavior this layer belongs to
	/// </summary>
	internal Behavior Behavior { get; set; }

	/// <inheritdoc cref="Behavior.Npc"/>
	protected Npc Npc => Behavior.Npc;
}
