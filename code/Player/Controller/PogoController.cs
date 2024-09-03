using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
using Sandbox.Utility.Svg;

public sealed class PogoController : Component
{
	[Property] public float BaseJumpForce = 1000;
	[Property] public float MaxJumpForce { get; set; } = 15f;
	[Property] public float LeanSpeed { get; set; } = 100f;
	[Property] public CapsuleCollider Contact { get; set; }
	[Property] public BoxCollider DeathTrigger { get; set; }
	[RequireComponent] public Rigidbody rigidbody { get; set; }
	public ModelPhysics Ragdoll { get; set; }
	public List<Collider> Colliders { get; set; }
	public float JumpForce { get; set; }
	public bool IsGrounded = false;
	public bool IsJumping = false;
	public bool Alive = true;
	public TimeSince TimeHeld = 0f;
	public Angles tilt;
	public Rotation targetRotation;
	protected override void OnEnabled()
	{
		Ragdoll = Components.GetInChildrenOrSelf<ModelPhysics>();
		Alive = true;
	}
	protected override void OnFixedUpdate()
	{
		//Grounded check
		SceneTraceResult groundTrace = Scene.Trace.Body( Contact.Rigidbody.PhysicsBody, Transform.Position + Vector3.Down * 5)
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if( groundTrace.Hit ) IsGrounded = true; else IsGrounded = false;
		if( IsGrounded ) 
		{
			rigidbody.PhysicsBody.Velocity = 0;
			rigidbody.Gravity = false;
		} else
		{
			rigidbody.Gravity = true;
		}

		//if( DeathTrigger.Touching.Count() > 0 ) Die();
	}
	protected override void OnUpdate()
	{
		if( Alive ) Move();
	}
	public void Move()
	{
		//Jump hold and release
		if( Input.Down( "Jump" ) )
		{
			if( !IsJumping ) TimeHeld = 0f;
			IsJumping = true;
			JumpForce = BaseJumpForce + TimeHeld * 400;
			JumpForce = float.Clamp(JumpForce, BaseJumpForce, MaxJumpForce * 20);
		}
		if( Input.Released( "Jump" ) )
		{
			if(IsJumping) IsJumping = false;
			TimeHeld = 0f;
			if( IsGrounded ) rigidbody.ApplyImpulse( Transform.Rotation.Up * JumpForce * rigidbody.PhysicsBody.Mass );
		}

		//Pogo tilt
		rigidbody.PhysicsBody.Rotation *= Rotation.From(Input.AnalogMove.x, 0, -Input.AnalogMove.y) * Time.Delta * LeanSpeed;
		rigidbody.PhysicsBody.Rotation = rigidbody.PhysicsBody.Rotation.Angles().WithYaw(Scene.Camera.Transform.Rotation.Yaw());
	}
	public void Die()
	{
		Log.Info("dead");
		Alive = false;

		var locking = rigidbody.Locking;
		locking.Pitch = false;
		locking.Roll = false;
		locking.Yaw = false;
		rigidbody.Locking = locking;

		Ragdoll.Enabled = true;
	}
}