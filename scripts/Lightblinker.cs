using Godot;
using System;

public partial class Lightblinker : SpotLight3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public float HigherEnergy;
	[Export] public float LowerEnergy;
	[Export] public float TimeDenom = 1000.0f;
	public bool Enabled = true;
	private FastNoiseLite fnl;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled) {return;}
		float time = Time.GetTicksMsec();
		float rand = fnl.GetNoise1D(time / TimeDenom);
		float rand2 = fnl.GetNoise1D(time / TimeDenom + rand);

		LightEnergy = LowerEnergy + Mathf.Abs(rand + rand2) * HigherEnergy;
	}
	public override void _Ready()
	{
		fnl = new FastNoiseLite();
	}
}
