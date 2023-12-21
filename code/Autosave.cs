using System;
using Sandbox;

public sealed class Autosave : Component
{
	[Property, Range(1, 300)] public int IntervalSeconds { get; set; } = 300;

	private TimeSince _lastAutosave;

	protected override void OnUpdate()
	{
		if ( _lastAutosave > IntervalSeconds )
		{
			Persistence.SaveScene( "autosave.json" );
			_lastAutosave = 0f;
		}
	}
}
