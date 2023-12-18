using Sandbox;
using System;

public sealed class BuyTool : Component
{
	[Property] public GameObject Product { get; set; }
	[Property] public GameObject Eyes { get; set; }
	[Property] public float TraceDistance { get; set; } = 600f;
	[Property, Range(1, 256, 1)] public int GridSnap { get; set; } = 16;

	private Ghost _ghost;

	protected override void OnUpdate()
	{
		if ( Product == null || !TryRunTrace( out var tr ) )
		{
			_ghost?.GameObject?.Destroy();
			_ghost = null;
			return;
		}
		if ( _ghost is null )
		{
			CreateGhost();
			return;
		}
		_ghost.CurrentBuildplane = tr.GameObject;
		UpdateGhostTransform( tr );
		if ( _ghost.IsAllowed && Input.Pressed( "attack1" ) )
		{
			Buy();
		}
	}

	private void CreateGhost()
	{
		var ghostGo = SceneUtility.Instantiate( Product );
		_ghost = ghostGo.Components.GetOrCreate<Ghost>();
		_ghost.Enabled = true;
	}

	private bool TryRunTrace( out SceneTraceResult tr )
	{
		var ray = new Ray( Eyes.Transform.Position, Eyes.Transform.Rotation.Forward );
		tr = Scene.Trace
			.Ray( ray, TraceDistance )
			.WithTag( "buildplane" )
			.WithoutTags( "ghost" )
			.Run();
		return tr.Hit;
	}

	private void UpdateGhostTransform( SceneTraceResult tr )
	{
		var pos = tr.HitPosition;
		if ( Input.Down( "run" ) && tr.Normal == Vector3.Up )
		{
			pos.x = MathF.Round( pos.x / GridSnap ) * GridSnap;
			pos.y = MathF.Round( pos.y / GridSnap ) * GridSnap;
		}
		_ghost.Transform.Position = pos;
		var rotation = Rotation.LookAt( tr.Normal );
		rotation *= Rotation.FromPitch( 90f );
		_ghost.Transform.Rotation = rotation;
	}

	private void Buy()
	{
		// Delete just the component, leaving the GameObject intact.
		_ghost.Destroy();
		_ghost = null;
	}
}
