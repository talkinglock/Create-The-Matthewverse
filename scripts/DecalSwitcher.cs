using Godot;
using System;

public partial class DecalSwitcher : Node3D
{
	[Export] public float timeBetweenDecals;
	[Export] public float decals;

	private int lastNum = 1;
	private float nextTime;
	private Decal lastDecal;
	public bool enabled = true;
	public override void _PhysicsProcess(double delta)
	{
		if (!enabled)
		{
			foreach (Node child in GetChildren())
			{
				if (child is Decal)
				{
					child.QueueFree();
				}
			}
		}
		float now = Time.GetTicksMsec();
		if (now > nextTime)
		{
			nextTime = now + timeBetweenDecals * 1000;
			Decal currentDecal = (Decal)GetNode(lastNum.ToString());
			
			if (lastDecal != null)
			{
				lastDecal.Visible = false;
			}

			currentDecal.Visible = true;
			lastDecal = currentDecal;
			lastNum++;
			if (lastNum > decals)
			{
				lastNum = 1;
			}
		}
	}
}
