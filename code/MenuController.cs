using System;
using Sandbox;

public sealed class MenuController : Component
{
	[Property] public ModelBrowser ModelMenu { get; set; }
	[Property] public BuyTool BuyTool { get; set; }

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "score" ) )
		{
			ModelMenu.Enabled = !ModelMenu.Enabled;
			BuyTool.Enabled = !ModelMenu.Enabled;
		}
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
