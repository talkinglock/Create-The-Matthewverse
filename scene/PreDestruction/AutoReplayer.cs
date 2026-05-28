using Godot;
using System;

public partial class AutoReplayer : AudioStreamPlayer3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Process(double delta)
	{
		if (Playing == false)
		{
			Play();
		}
	}
}
