using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class Ghost : Component
{
	[Property] public GameObject CurrentBuildplane { get; set; }
	public bool IsAllowed { get; private set; } = false;

	private static Material _ghostMat = Material.Load( "materials/default/white.vmat" );
	private Dictionary<Collider, Vector3> _colliderScales = new();
	private Dictionary<ModelRenderer, ModelRenderer.ShadowRenderType> _rendererShadows = new();

	protected override void OnEnabled()
	{
		Tags.Add( "ghost" );

		foreach ( var collider in GetColliders )
		{
			// Ensure that the colliders have touch enabled before checking for overlap.
			collider.IsTrigger = true;
			var oldScale = collider.Transform.LocalScale;
			_colliderScales[collider] = oldScale;
			// Ignore just a little bit of clipping.
			collider.Transform.LocalScale = oldScale * 0.95f;
		}

		foreach( var renderer in GetRenderers )
		{
			renderer.MaterialOverride = _ghostMat;
			_rendererShadows[renderer] = renderer.RenderType;
			renderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		}

		ShowAllowed( false );
	}

	private IEnumerable<ModelRenderer> GetRenderers 
		=> Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants );
	private IEnumerable<Collider> GetColliders 
		=> Components.GetAll<Collider>( FindMode.EnabledInSelfAndDescendants );

	protected override void OnDisabled()
	{
		Tags.Remove( "ghost" );
		Tags.Add( "buildplane" );

		foreach( var renderer in GetRenderers )
		{
			renderer.MaterialOverride = null;
			renderer.Tint = Color.White;
		}

		foreach( var collider in GetColliders )
		{
			if ( _colliderScales.ContainsKey( collider ) )
			{
				collider.Transform.LocalScale = _colliderScales[collider];
			}
		}
		foreach( var renderer in GetRenderers )
		{
			if ( _rendererShadows.ContainsKey( renderer ) )
			{
				renderer.RenderType = _rendererShadows[renderer];
			}
		}
		_colliderScales.Clear();
		_rendererShadows.Clear();
	}

	private void ShowAllowed( bool isAllowed )
	{
		foreach( var renderer in GetRenderers )
		{
			renderer.Tint = isAllowed ? Color.Cyan.WithAlpha( 0.6f ) : Color.Red.WithAlpha( 0.6f );
		}
	}

	protected override void OnUpdate()
	{
		IsAllowed = !CheckTouch();
		ShowAllowed( IsAllowed );
	}

	private bool CheckTouch()
	{
		var isTouching = false;
		foreach( var collider in GetColliders )
		{
			collider.IsTrigger = true;
			var validTouches = collider.Touching.Where( c => c.GameObject != CurrentBuildplane );
			if ( validTouches.Any() )
			{
				isTouching = true;
				break;
			}
		}
		return isTouching;
	}
}
