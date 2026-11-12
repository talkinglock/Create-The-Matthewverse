using Godot;
using System;

public partial class FilmProjectorSpinner : Node3D
{
	[Export] public Node3D UpperSpindle;
	[Export] public Node3D LowerSpindle;
	private float SpinSpeed = 0.0f;
	
	public void SetSpinSpeed(float spinSpeed) {
		SpinSpeed = spinSpeed;
	}

	private void spin(Node3D obj, float rate) {
		obj.RotateX(rate);
	}

	public override void _PhysicsProcess(double delta) {
		spin(UpperSpindle, -SpinSpeed);
		spin(LowerSpindle, -SpinSpeed);
	}
}
