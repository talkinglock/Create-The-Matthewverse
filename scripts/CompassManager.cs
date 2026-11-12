using Godot;
using System;

public partial class CompassManager : Node3D
{
	[Export] public Node3D needle;
	[Export] public float TimeBetweenNeedleChanges;
	[Export] public float TimeBetweenNeedleChangesVariation;

	private float TimeOfNextChange = -1;
	private Standard std;
	
	private bool isPositive = true;
	public override void _Ready()
	{
		std = new Standard();
	}
	public override void _PhysicsProcess(double delta)
	{
		float now = Time.GetTicksMsec();
		if (now > TimeOfNextChange)
		{
			TimeOfNextChange = std.GetRandomTimeInFuture(TimeBetweenNeedleChanges, TimeBetweenNeedleChangesVariation);
			isPositive = !isPositive;
		}
		if (isPositive)
		{
			needle.RotateY(0.1f);
		}
		else
		{
			needle.RotateY(-0.1f);
		}

		
	}
}
