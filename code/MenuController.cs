using Sandbox;

public sealed class MenuController : Component
{
	[Property] public ModelBrowser ModelMenu { get; set; }
	protected override void OnUpdate()
	{
		if ( Input.Pressed( "score" ) )
		{
			ModelMenu.Enabled = !ModelMenu.Enabled;
		}
	}
}
