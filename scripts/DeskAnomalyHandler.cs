using Godot;
using System;

public partial class DeskAnomalyHandler : Node3D
{
	[Export] public PlayerController plr;
	[Export] public float distanceToEnterSquared;
	[Export] public float distanceToExitSquared;
	[Export] public float distanceToDestroyObject;
	private bool enteredOnce = false;
	private bool disappeared = false;
	public override void _PhysicsProcess(double delta)
	{
		float distanceSquared = (plr.rigidbody.GlobalPosition - GlobalPosition).LengthSquared();
		if (distanceSquared < distanceToEnterSquared)
		{
			if (enteredOnce == false)
			{
				enteredOnce = true;
			}
		}
		if (distanceSquared > distanceToExitSquared)
		{
			
			if (enteredOnce == true && disappeared == false)
			{
				disappeared = true;
				foreach (Node3D node in GetChildren())
				{
					if ((node.GlobalPosition - GlobalPosition).Length() < distanceToDestroyObject)
					{
						node.QueueFree();
					}
				}
			}
		}
	}
}
