using System;
using Sandbox;

public sealed class PogoController : Component
{
	[Property] public float BaseJumpForce = 200;
	[Property] public float MaxJumpMultiplier { get; set; } = 5f;
	[Property] public float LeanSpeed { get; set; } = 100f;
	[Property] public BoxCollider DeathTrigger { get; set; }
	[RequireComponent] public CharacterController Player { get; set; }
	public ModelPhysics Ragdoll;
	public float JumpForce;
	public bool IsJumping = false;
	public bool Alive = true;
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
		Player.Move();
		if ( Player.IsOnGround )
		{
			Player.Velocity = Player.Velocity.WithZ( 0 );
			Player.ApplyFriction( 100f );
		}
		else Player.Velocity += gravity * Time.Delta;

		//Jump hold and release
		if ( Input.Down( "Jump" ) )
		{
			if ( !IsJumping ) TimeHeld = 0f;
			IsJumping = true;
			JumpForce = BaseJumpForce + TimeHeld * 400f;
			JumpForce = float.Clamp( JumpForce, BaseJumpForce, BaseJumpForce * MaxJumpMultiplier );
		}
		if ( Input.Released( "Jump" ) )
		{
			IsJumping = false;
			TimeHeld = 0f;
			if ( Player.IsOnGround ) Player.Punch( Transform.Rotation.Up * JumpForce );
		}

		//Pogo tilt
		Angles wishRotation = new Angles( Input.AnalogMove.x, 0, -Input.AnalogMove.y ) * Time.Delta;
		Transform.Rotation *= wishRotation * LeanSpeed;
		Transform.Rotation = Transform.Rotation.Angles().WithYaw( Scene.Camera.Transform.Rotation.Yaw() );

		//Keybinds
		if ( Input.Pressed( "reload" ) ) Respawn();
	}
	public void Die()
	{
		Log.Info( "dead" );
		Alive = false;
		Ragdoll.Enabled = true;
	}

	public void Respawn()
	{
		//Ragdoll.Enabled = false;
		Transform.Position = new Vector3( 0, 0, 0 );
		Player.Velocity = Vector3.Zero;
		Alive = true;
	}
}