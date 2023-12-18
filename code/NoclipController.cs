using Sandbox;

public sealed class NoclipController : Component
{
	[Property] public GameObject Eyes { get; set; }
	[Property] public float LookSpeed { get; set; } = 10f;
	[Property] public float MoveSpeed { get; set; } = 100f;

	public Angles EyeAngles { get; set; }
	public Ray EyeRay => new( Eyes.Transform.Position, Eyes.Transform.Rotation.Forward );

	protected override void OnEnabled()
	{
		EyeAngles = Eyes.Transform.Rotation;
	}

	protected override void OnUpdate()
	{
		UpdateEyes();

		var cc = Components.Get<CharacterController>();
		if ( cc is null )
			return;

		var moveVec = Input.AnalogMove * MoveSpeed * Eyes.Transform.Rotation;
		if ( Input.Down( "jump" ) )
		{
			moveVec += Vector3.Up * MoveSpeed;
		}
		else if ( Input.Down( "duck" ) )
		{
			moveVec += Vector3.Down * MoveSpeed;
		}
		if ( Input.Down( "run" ) )
		{
			moveVec *= 3f;
		}

		cc.Velocity = moveVec;
		cc.Move();
	}

	private void UpdateEyes()
	{
		var newAngles = EyeAngles + Input.AnalogLook * LookSpeed;
		newAngles.pitch = newAngles.pitch.Clamp( -89f, 89f );
		EyeAngles = newAngles;
		Eyes.Transform.Rotation = EyeAngles.ToRotation();
		
	}
}
