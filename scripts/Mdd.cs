using Godot;
using System;

public partial class Mdd : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Node3D rotor;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		rotor.RotateY(0.01f);
	}
}
