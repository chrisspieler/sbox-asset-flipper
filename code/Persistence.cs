using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Sandbox
{
	public static class Persistence
	{
		public static IEnumerable<GameObject> GetPersistentObjects()
		{
			return GameManager.ActiveScene
				.GetAllObjects( false )
				.Where( go => go.Tags.Has( "save" ) );
		}

		public static void SaveScene( string fileName )
		{
			if ( !GameManager.ActiveScene.IsValid() )
				return;

			var options = new GameObject.SerializeOptions();
			var goJson = GameManager.ActiveScene
				.GetAllObjects( false )
				.Where( go => go.Tags.Has( "save" ) && !go.Parent.Tags.Has( "save" ) )
				.Select( go => go.Serialize( options ) )
				.ToArray();
			Log.Info( $"Saving file: {fileName}" );
			FileSystem.Data.WriteJson( fileName, goJson );
		}

		public static void Quicksave()
		{
			SaveScene( "quicksave.json" );
		}

		public static void LoadScene( string fileName )
		{
			Log.Info( $"Loading file: {fileName}" );
			var saveable = GetPersistentObjects();
			foreach ( var go in saveable )
			{
				go.Destroy();
			}
			var goJson = FileSystem.Data.ReadJson<JsonObject[]>( fileName );
			foreach( var json in goJson )
			{
				var go = new GameObject();
				go.Deserialize( json );
			}
		}

		public static void Quickload()
		{
			LoadScene( "quicksave.json" );
		}
	}
}
