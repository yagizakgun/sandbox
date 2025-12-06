namespace Sandbox.Npcs;

/// <summary>
/// Quick stupid class that holds condition tags for an Npc
/// </summary>
public sealed class Conditions
{
	private HashSet<string> _all = new();

	/// <summary>
	/// Do we have this condition?
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool Contains( string tag )
	{
		return _all.Contains( tag );
	}

	/// <summary>
	/// Sets a condition
	/// </summary>
	/// <param name="tag"></param>
	/// <param name="value"></param>
	public void Set( string tag, bool value = true )
	{
		if ( value )
		{
			_all.Add( tag );
		}
		else
		{
			_all.Remove( tag );
		}
	}
}
