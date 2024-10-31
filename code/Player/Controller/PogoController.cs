using System;
using Sandbox;

public sealed class PogoController : Component
{
	[Property] public float BaseJumpForce = 200;
	[Property] public float MaxJumpMultiplier { get; set; } = 5f;
	[Property] public float LeanSpeed { get; set; } = 100f;
	[RequireComponent] public PlayerController Player { get; set; }
	public ModelPhysics Ragdoll;
	public float JumpForce;
	public bool IsJumpHeld = false;
	public bool Alive = true;
	public bool IsGrounded = false;
	public TimeSince TimeHeld = 0f;
	public Angles tilt;
	protected override void OnEnabled()
	{
		Ragdoll = Components.GetInChildrenOrSelf<ModelPhysics>();
		Alive = true;
	}
	protected override void OnFixedUpdate()
	{
		if ( Alive && !IsProxy ) Move();
	}
	public void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;
		Player.GroundFriction = 10000f;
		Player.PreventGrounding(.1f);

		//Custom grounded check
		SceneTraceResult tr = Scene.Trace.Body(Player.Body.PhysicsBody, LocalPosition + Vector3.Down * 2).IgnoreGameObjectHierarchy(this.GameObject).Run();
		IsGrounded = tr.Hit;

		if( IsGrounded )
		{
			Player.Body.Velocity = new Vector3(0, 0, Player.Body.Velocity.z);
		}

		//Jump hold and release
		if ( Input.Down( "Jump" ) )
		{
			if ( !IsJumpHeld ) TimeHeld = 0f;
			IsJumpHeld = true;
			JumpForce = BaseJumpForce + TimeHeld * 400f;
			JumpForce = float.Clamp( JumpForce, BaseJumpForce, BaseJumpForce * MaxJumpMultiplier );
		}
		if ( Input.Released( "Jump" ) )
		{
			IsJumpHeld = false;
			TimeHeld = 0f;
			if ( IsGrounded ) Player.Jump(LocalRotation.Up * JumpForce);
		}

		if ( IsGrounded && !IsJumpHeld ) Player.Jump( LocalRotation.Up * BaseJumpForce );

		//Pogo rotation
		Angles wishRotation = new Angles( Input.AnalogMove.x, 0, 0 ) * Time.Delta;
		LocalRotation *= wishRotation * LeanSpeed;
		LocalRotation = LocalRotation.Angles().WithYaw( Scene.Camera.LocalRotation.Yaw() );
	}
}