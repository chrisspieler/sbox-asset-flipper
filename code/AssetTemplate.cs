using System.Threading.Tasks;

namespace Sandbox;

public static class AssetTemplate
{
	public static async Task<GameObject> GetModel( Package package )
	{
		var packageInfo = await Package.FetchAsync( package.FullIdent, false );
		var modelPath = packageInfo.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		Log.Info( $"({package.Title}) Loading model: {modelPath}" );
		await packageInfo.MountAsync();
		var model = Model.Load( modelPath );
		var go = new GameObject( false, "Model Prefab" );
		var renderer = go.Components.Create<ModelRenderer>();
		renderer.Model = model;
		var collider = go.Components.Create<ModelCollider>();
		collider.Model = model;
		return go;
	}
}
