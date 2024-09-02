using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Sandbox;

public sealed class PogoController : Component
{
	[Property] public float BaseJumpForce = 1000;
	[Property] public float MaxJumpForce { get; set; } = 15f;
	[Property] public SphereCollider Contact { get; set; }
	[RequireComponent] public Rigidbody rigidbody { get; set; }
	public float JumpForce { get; set; }
	public bool IsGrounded = false;
	public bool IsJumping = false;
	public TimeSince TimeHeld = 0f;
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

		//Jumping
		if( Input.Down( "Jump" ) )
		{
			if( !IsJumping ) TimeHeld = 0f;
			IsJumping = true;
			
			JumpForce = BaseJumpForce + TimeHeld * 600;
			JumpForce = float.Clamp(JumpForce, BaseJumpForce, MaxJumpForce * 100);
			Log.Info(JumpForce);
		}
		if( Input.Released( "Jump" ) )
		{
			if(IsJumping) IsJumping = false;
			TimeHeld = 0f;
			Log.Info("jump");
			if( IsGrounded ) rigidbody.ApplyImpulse( Transform.Rotation.Up * JumpForce * rigidbody.PhysicsBody.Mass );
		}

		Scene.Camera.Transform.Position = Transform.Position + Vector3.Up * 200 + Vector3.Backward * 400;
		Transform.Rotation *= Rotation.From(Input.AnalogMove.x, 0, -Input.AnalogMove.y);
		// Vector3 eulerOffset = Time.Delta * new Vector3(Input.AnalogMove.y, -Input.AnalogMove.x, 0);
		// Vector3 eulerOffsetGlobal = Scene.Camera.Transform.Rotation * eulerOffset;
		// Rotation offset = new Quaternion(eulerOffsetGlobal, 0);
		// Transform.Rotation = offset * Transform.Rotation;

	}
}