using Godot;
using System;

public partial class RedZone : Node3D
{
	[Export] public PlayerController plr;
	[Export] public Color redColor;
	[Export] public Color normalColor;
	[Export] public float distanceToStart;
	[Export] public WorldEnvironment env;
	[Export] public AudioStreamPlayer ambience;	
	[Export] public float endAmbiencePitchScale;
	private float distanceToStartSquared;
	public override void _Ready()
	{
		distanceToStartSquared = (distanceToStart * distanceToStart) - 20.0f;
	}
	public override void _PhysicsProcess(double delta)
	{
		float distanceSquared = (plr.rigidbody.GlobalPosition - GlobalPosition).LengthSquared();
		if (distanceSquared < distanceToStartSquared)
		{
			float distance = (plr.rigidbody.GlobalPosition - GlobalPosition).Length();
			float normalized = distance / distanceToStart;
			Color endColor = redColor.Lerp(normalColor, normalized);
			ambience.PitchScale = Mathf.Lerp(1.0f, endAmbiencePitchScale, -normalized + 1);
			env.Environment.AmbientLightColor = endColor;
			env.Environment.FogLightColor = endColor;
		}
	}
}
