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
		var go = new GameObject( false, packageInfo.Title );
		var modelGo = new GameObject( true, "Model" );
		modelGo.SetParent( go );
		var renderer = modelGo.Components.Create<ModelRenderer>();
		renderer.Model = model;
		var collider = modelGo.Components.Create<ModelCollider>();
		collider.Model = model;
		var physMins = packageInfo.GetMeta<Vector3>( "PhysicsMins" );
		modelGo.Transform.Position += Vector3.Zero.WithZ( -physMins.z );
		return go;
	}
}
