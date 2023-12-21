using Sandbox;
using System;
using System.Collections.Generic;

public sealed class BuyTool : Component
{
	[Property] public ToolInfoPanel ToolPanel { get; set; }
	[Property] public GameObject Product 
	{
		get => _product;
		set
		{
			// Make sure we don't leave template GameObjects lying around.
			if ( _product.IsValid() )
			{
				_product.Destroy();
			}
			_product = value;
			DestroyGhost();
		}
	}
	private GameObject _product;
	[Property] public GameObject CursorLight { get; set; }
	[Property] public GameObject Eyes { get; set; }
	[Property] public float TraceDistance { get; set; } = 600f;
	[Property, Range(1, 256, 1)] public int GridSnap { get; set; } = 16;
	[Property, Range( 1, 90, 1 )] public float RotationSnap { get; set; } = 30f;

	private Ghost _ghost;
	private GameObject _activeCursorLight;
	private float _rotationInput = 0f;

	const string TOOL_NAME = "Buy Tool";

	protected override void OnEnabled()
	{
		var glyphList = new List<InputGlyphData>()
		{
			new InputGlyphData { InputAction = "attack1", Description = "Place Product" },
			new InputGlyphData { InputAction = "score", Description = "Open Shop" }
		};
		ToolPanel.SetTool( TOOL_NAME, "Place assets in the world.", glyphList );
	}

	protected override void OnDisabled()
	{
		ToolPanel.ClearTool( TOOL_NAME );
	}

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
		var rotationInput = GetRotationInput();
		UpdateGhostTransform( tr, gridSnap, rotationInput );
		DrawPlacementPlane( tr, gridSnap );
		if ( _ghost.IsAllowed && Input.Pressed( "attack1" ) )
		{
			Buy();
		}
	}

	private void CreateGhost()
	{
		var ghostGo = SceneUtility.Instantiate( Product );
		ghostGo.BreakFromPrefab();
		_ghost = ghostGo.Components.GetOrCreate<Ghost>();
		_ghost.Enabled = true;
		ghostGo.Enabled = true;
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
			.WithTag( "solid" )
			.WithoutTags( "ghost" )
			.Run();
		return tr.Hit;
	}

	private float GetRotationInput()
	{
		// Grid products must remain axis-aligned.
		var isGridProduct = _ghost.Tags.Has( "grid" );
		var rotationSnap = isGridProduct ? 90f : RotationSnap;
		_rotationInput += (Input.MouseWheel.y * rotationSnap ) % 360f;
		return MathF.Round( _rotationInput / rotationSnap ) * rotationSnap;
	}

	private void UpdateGhostTransform( SceneTraceResult tr, bool gridSnap, float yaw )
	{
		var pos = tr.HitPosition;
		if ( gridSnap )
		{
			pos.x = GetSnap( pos.x, tr.Normal, Vector3.Forward );
			pos.y = GetSnap( pos.y, tr.Normal, Vector3.Right );
			pos.z = GetSnap( pos.z, tr.Normal, Vector3.Up );
		}
		_ghost.Transform.Position = pos;
		var rotation = Rotation.LookAt( tr.Normal );
		rotation *= Rotation.FromPitch( 90f );
		rotation *= Rotation.FromYaw( yaw );
		_ghost.Transform.Rotation = rotation;
	}

	private float GetSnap( float value, Vector3 normal, Vector3 axis )
	{
		// If the normal is parallel to the axis, just use the hit position.
		if ( MathF.Abs( normal.Dot( axis ) ) > 0.9f ) return value;
		return MathF.Round( value / GridSnap ) * GridSnap;
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
		// Persist this GameObject.
		_ghost.GameObject.Tags.Add( "save" );
		// Delete just the component, leaving the GameObject intact.
		_ghost.Destroy();
		_ghost = null;
		_activeCursorLight?.Destroy();
	}
}
