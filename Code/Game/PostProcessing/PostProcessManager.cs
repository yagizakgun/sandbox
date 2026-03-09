public sealed class PostProcessManager : GameObjectSystem<PostProcessManager>
{
	private readonly Dictionary<string, GameObject> _active = new();
	private readonly Dictionary<string, List<Component>> _components = new();

	public string SelectedPath { get; private set; }
	public GameObject SelectedGo
	{
		get
		{
			if ( SelectedPath is null ) return null;
			_active.TryGetValue( SelectedPath, out var go );
			return go;
		}
	}

	public IReadOnlyList<Component> GetSelectedComponents() =>
		SelectedPath != null && _components.TryGetValue( SelectedPath, out var comps )
			? comps
			: Array.Empty<Component>();

	public PostProcessManager( Scene scene ) : base( scene ) { }

	public bool IsEnabled( string resourcePath ) =>
		_components.TryGetValue( resourcePath, out var comps ) && comps.Any( c => c.Enabled );

	private void SpawnGo( string resourcePath, bool startEnabled )
	{
		var resource = ResourceLibrary.Get<PostProcessResource>( resourcePath );
		if ( resource?.Prefab is null ) return;

		var camera = Scene.Camera?.GameObject;
		if ( camera is null ) return;

		var go = GameObject.Clone( resource.Prefab, new CloneConfig { StartEnabled = true, Parent = camera } );

		var comps = go.Components.GetAll<Component>().ToList();
		_components[resourcePath] = comps;
		foreach ( var c in comps ) c.Enabled = startEnabled;

		_active[resourcePath] = go;
	}

	public void Select( string resourcePath )
	{
		SelectedPath = resourcePath;
		if ( !_active.ContainsKey( resourcePath ) )
			SpawnGo( resourcePath, startEnabled: false );
	}

	private string _previewPath;

	public void Preview( string resourcePath )
	{
		if ( _previewPath == resourcePath ) return;
		Unpreview();

		_previewPath = resourcePath;
		if ( IsEnabled( resourcePath ) ) return;

		if ( !_active.ContainsKey( resourcePath ) )
			SpawnGo( resourcePath, startEnabled: true );
		else if ( _components.TryGetValue( resourcePath, out var comps ) )
			foreach ( var c in comps ) c.Enabled = true;
	}

	public void Unpreview()
	{
		if ( _previewPath is null ) return;

		if ( _previewPath != SelectedPath && !IsEnabled( _previewPath ) )
			if ( _components.TryGetValue( _previewPath, out var comps ) )
				foreach ( var c in comps ) c.Enabled = false;

		_previewPath = null;
	}

	public void Deselect()
	{
		SelectedPath = null;
	}

	public void Toggle( string resourcePath )
	{
		SelectedPath = resourcePath;

		if ( _active.ContainsKey( resourcePath ) )
		{
			var enabled = IsEnabled( resourcePath );
			if ( _components.TryGetValue( resourcePath, out var comps ) )
				foreach ( var c in comps ) c.Enabled = !enabled;
			return;
		}

		SpawnGo( resourcePath, startEnabled: true );
	}
}
