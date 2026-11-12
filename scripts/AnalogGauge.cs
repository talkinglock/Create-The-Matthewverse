using Godot;
using System;

public partial class AnalogGauge : Node3D
{
	[ExportGroup("Objects")]
	[Export] public Node3D NeedleObj;
	[ExportGroup("Values")]
	[Export] public float MaximumDeflectionDegrees;

	public float MaximumDeflection; 
	public override void _Ready()
	{
		MaximumDeflection = Mathf.DegToRad(MaximumDeflectionDegrees);
	}

	public void SetToAlpha(float alpha, float time) {
		alpha = Mathf.Clamp(alpha, 0, 1);
		float newAngle = Mathf.Lerp(MaximumDeflection, -MaximumDeflection, alpha);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(NeedleObj, "rotation", new Vector3(0, newAngle, 0), time);
	}
}
