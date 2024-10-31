using System;
using Sandbox;

public sealed class PogoController : Component
{
	[Property] public float BaseJumpForce = 500;
	[Property] public float MaxJumpMultiplier { get; set; } = 1.25f;
	[RequireComponent] public PlayerController Player { get; set; }
	public ModelPhysics Ragdoll;
	public bool IsJumpHeld = false;
	public bool IsGrounded = false;
	public bool IsAlive = true;
	public float JumpForce;
	public TimeSince TimeHeld = 0f;
	public Angles tilt;
	protected override void OnEnabled()
	{
		Ragdoll = Components.GetInChildrenOrSelf<ModelPhysics>();
		IsAlive = true;
	}
	protected override void OnFixedUpdate()
	{
		if ( IsAlive && !IsProxy ) Move();
	}
	public void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;
		Player.GroundFriction = 10000f;

		//Custom grounded check
		SceneTraceResult tr = Scene.Trace.Sphere( 13, LocalPosition, LocalPosition + Vector3.Down ).IgnoreGameObjectHierarchy( this.GameObject ).Run();
		IsGrounded = tr.Hit;

		if( IsGrounded )
		{
			Player.Body.Velocity = new Vector3(0, 0, 0);
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
		LocalRotation = Scene.Camera.LocalRotation;
	}
}