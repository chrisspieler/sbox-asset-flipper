using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class Ghost : Component
{
	[Property] public GameObject Ignore { get; set; }

	private static Material _ghostMat = Material.Load( "materials/default/white.vmat" );

	protected override void OnEnabled()
	{
		// Ensure that the colliders have touch enabled before checking for overlap.
		foreach( var collider in GetColliders )
		{
			collider.IsTrigger = true;
		}

		foreach( var renderer in GetRenderers )
		{
			renderer.MaterialOverride = _ghostMat;
		}

		ShowAllowed( false );
	}

	private IEnumerable<ModelRenderer> GetRenderers 
		=> Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants );
	private IEnumerable<Collider> GetColliders 
		=> Components.GetAll<Collider>( FindMode.EnabledInSelfAndDescendants );

	protected override void OnDisabled()
	{
		foreach( var renderer in GetRenderers )
		{
			renderer.MaterialOverride = null;
			renderer.Tint = Color.White;
		}
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
		ShowAllowed( !CheckTouch() );
	}

	private bool CheckTouch()
	{
		var isTouching = false;
		foreach( var collider in GetColliders )
		{
			collider.IsTrigger = true;
			var validTouches = collider.Touching.Where( c => c.GameObject != Ignore );
			if ( validTouches.Any() )
			{
				isTouching = true;
				break;
			}
		}
		return isTouching;
	}
}
