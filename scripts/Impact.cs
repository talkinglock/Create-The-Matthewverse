using Godot;
using System;

public partial class Impact : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public RigidBody3D rigid; 
	public override void _Ready()
	{
		rigid.ContactMonitor = true;
		rigid.MaxContactsReported = 1;

		rigid.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (rigid.LinearVelocity.Length() > 3)
		{
			rigid.GetNode<AudioStreamPlayer3D>("Impact").Play();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		
	}
}
