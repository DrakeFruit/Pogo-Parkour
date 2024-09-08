using Sandbox;
using Sandbox.UI;

public sealed class PogoCameraController : Component
{
	[Property] public float Distance { get; set; } = 400;
	[Property] public float ZoomSpeed { get; set; } = 20;
	[RequireComponent] PogoController controller { get; set; }
	public Transform Head;
	protected override void OnStart()
	{
		if ( !IsProxy ) Head.Rotation = Transform.Rotation;
	}
	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			var eyeAngles = Head.Rotation.Angles();
			eyeAngles += Input.AnalogLook;
			eyeAngles.roll = 0;
			eyeAngles.pitch = eyeAngles.pitch.Clamp( -89, 89 );

			Head.Rotation = eyeAngles.ToRotation();
			Head.Position = controller.Transform.Position + Vector3.Up * 70;

			Distance += -Input.MouseWheel.y * ZoomSpeed;
			Distance = Distance.Clamp( 100, 1000 );

			if(Scene.Camera is not null)
			{
				var camforward = eyeAngles.ToRotation().Forward;
				SceneTraceResult camTrace = Scene.Trace.Ray( Head.Position, Head.Position - camforward * Distance )
					.IgnoreGameObjectHierarchy( GameObject )
					.WithoutTags( "trigger" )
					.Run();

				if( camTrace.Hit ) { Scene.Camera.Transform.Position = camTrace.EndPosition + camTrace.Normal; } 
				else Scene.Camera.Transform.Position = camTrace.EndPosition;

				Scene.Camera.Transform.Rotation = eyeAngles.ToRotation();
			}
		}
	}
}
