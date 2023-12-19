using Sandbox;
using System;

public sealed class BuyTool : Component
{
	[Property] public GameObject Product { get; set; }
	[Property] public GameObject CursorLight { get; set; }
	[Property] public GameObject Eyes { get; set; }
	[Property] public float TraceDistance { get; set; } = 600f;
	[Property, Range(1, 256, 1)] public int GridSnap { get; set; } = 16;

	private Ghost _ghost;
	private GameObject _activeCursorLight;

	protected override void OnUpdate()
	{
		if ( Product == null || !TryRunTrace( out var tr ) )
		{
			DestroyGhost();
			return;
		}
		if ( _ghost is null )
		{
			CreateGhost();
			return;
		}
		_ghost.CurrentBuildplane = tr.GameObject;
		var gridSnap = tr.GameObject.Tags.Has( "grid" );
		UpdateGhostTransform( tr, gridSnap );
		DrawPlacementPlane( tr, gridSnap );
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
		if ( CursorLight != null )
		{
			_activeCursorLight = SceneUtility.Instantiate( CursorLight );
			_activeCursorLight.SetParent( _ghost.GameObject );
			_activeCursorLight.Transform.LocalPosition += Vector3.Up * 20f;
		}
	}

	private void DestroyGhost()
	{
		_activeCursorLight?.Destroy();
		_ghost?.GameObject?.Destroy();
		_ghost = null;
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

	private void UpdateGhostTransform( SceneTraceResult tr, bool gridSnap )
	{
		var pos = tr.HitPosition;
		if ( gridSnap )
		{
			pos.x = MathF.Round( pos.x / GridSnap ) * GridSnap;
			pos.y = MathF.Round( pos.y / GridSnap ) * GridSnap;
			pos.z = MathF.Round( pos.z / GridSnap ) * GridSnap;
		}
		_ghost.Transform.Position = pos;
		var rotation = Rotation.LookAt( tr.Normal );
		rotation *= Rotation.FromPitch( 90f );
		_ghost.Transform.Rotation = rotation;
	}

	private void DrawPlacementPlane( SceneTraceResult tr, bool useGrid )
	{
		Gizmo.Draw.Color = (Color.White * 0.75f).WithAlpha( 1f );
		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.SolidSphere( tr.HitPosition, 1.5f );
		if ( !useGrid )
		{
			Gizmo.Draw.LineCircle( _ghost.Transform.Position, tr.Normal, radius: 15f );
		}
		else
		{
			int gridSize = 5;
			var rotation = Rotation.LookAt( tr.Normal );
			var gridRightOffset = rotation.Right * gridSize * GridSnap / 2;
			var gridUpOffset = rotation.Up * gridSize * GridSnap / 2;
			var gridStart = _ghost.Transform.Position - gridRightOffset - gridUpOffset;
			for ( int x = 0; x <= gridSize; x++ )
			{
				var start = gridStart + rotation.Right * x * GridSnap;
				var end = start + rotation.Up * gridSize * GridSnap;
				Gizmo.Draw.Line( start, end );
			}
			for ( int y = 0; y <= gridSize; y++ )
			{
				var start = gridStart + rotation.Up * y * GridSnap;
				var end = start + rotation.Right * gridSize * GridSnap;
				Gizmo.Draw.Line( start, end );
			}
			// In the x+ direction on the edge of the grid, draw a red line.
			Gizmo.Draw.Color = Color.Red;
			var xHigh = rotation.Right * gridSize * GridSnap;
			var yHigh = rotation.Up * gridSize * GridSnap;
			Gizmo.Draw.Line( gridStart, gridStart + yHigh );
			Gizmo.Draw.Line( gridStart + xHigh, gridStart + yHigh + xHigh );
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.Line( gridStart, gridStart + xHigh );
			Gizmo.Draw.Line( gridStart + yHigh, gridStart + xHigh + yHigh );
		}
	}

	private void Buy()
	{
		// Delete just the component, leaving the GameObject intact.
		_ghost.Destroy();
		_ghost = null;
		_activeCursorLight?.Destroy();
	}
}
