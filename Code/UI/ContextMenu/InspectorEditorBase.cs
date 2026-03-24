using Sandbox.UI;

namespace Sandbox;

public abstract class InspectorEditorBase : Panel
{
	public abstract bool TrySetTarget( List<GameObject> selection );

	public virtual string TabTitle => TypeLibrary.GetType( GetType() )?.Title ?? GetType().Name;
}
