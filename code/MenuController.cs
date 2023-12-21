using Sandbox;

public sealed class MenuController : Component
{
	[Property] public ModelBrowser ModelMenu { get; set; }
	[Property] public SystemMenuPanel SystemMenu { get; set; }
	[Property] public BuyTool BuyTool { get; set; }
	[Property] public bool IsMenuOpen => ModelMenu.Enabled || SystemMenu.Enabled;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "score" ) )
		{
			ModelMenu.Enabled = !ModelMenu.Enabled;
		}
		if ( Input.EscapePressed )
		{
			SystemMenu.Enabled = !SystemMenu.Enabled;
		}
		// TODO: Figure out how to make this work with multiple tools.
		BuyTool.Enabled = !IsMenuOpen;

		if ( Input.Pressed( "menu" ) )
		{
			if ( Input.Down( "run" ) )
			{
				Persistence.Quickload();
			}
			else
			{
				Persistence.Quicksave();
			}
		}
	}
}
