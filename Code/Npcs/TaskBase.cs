using Sandbox.Npcs.Layers;

namespace Sandbox.Npcs;

/// <summary>
/// Base class for all tasks
/// </summary>
public abstract class TaskBase
{
	protected Npc Npc { get; private set; }
	protected Behavior Behavior { get; private set; }

	private bool _started;

	internal void Initialize( Npc npc, Behavior behavior )
	{
		Npc = npc;
		Behavior = behavior;
	}

	protected T GetLayer<T>() where T : BehaviorLayer
	{
		return Behavior?.GetLayer<T>();
	}

	internal void InternalStart()
	{
		if ( _started ) return;
		_started = true;
		OnStart();
	}

	internal TaskStatus InternalUpdate()
	{
		return OnUpdate();
	}

	internal void InternalEnd()
	{
		OnEnd();
	}

	protected virtual void OnStart() { }
	protected abstract TaskStatus OnUpdate();
	protected virtual void OnEnd() { }
}
