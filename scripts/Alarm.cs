using Godot;
using System;

public partial class Alarm : Node3D
{
	[Export] public BaseMaterial3D OnMaterial;
	[Export] public BaseMaterial3D OffMaterial;
	[Export] public MeshInstance3D LightBase;
	[Export] public Light3D Light;
	[Export] public float BlinkRateMsec;
	[Export] public bool Enabled = false;
	private bool IsOn = false;
	private float NextTime = -1.0f;
	public override void _PhysicsProcess(double delta) {
		if (!Enabled) {
			LightBase.MaterialOverride = OffMaterial;
			Light.LightEnergy = 0.0f;
			return;
		}
		float time = Time.GetTicksMsec();
		if (time >= NextTime) {
			NextTime = Time.GetTicksMsec() + BlinkRateMsec;
			if (IsOn) {
				Light.LightColor = OnMaterial.AlbedoColor;
				LightBase.MaterialOverride = OnMaterial;
				Light.LightEnergy = 0.5f;
			} else {
				LightBase.MaterialOverride = OffMaterial;
				Light.LightEnergy = 0.0f;
			}
			IsOn = !IsOn;
		}
	}
}
