using Godot;
using System;

public partial class Lightbulb : Node3D
{
	[Export] public ShaderMaterial LightbulbMaterial;
	[Export] public OmniLight3D Light;
	[Export] public AudioStreamPlayer3D ambience;


	public void SetLightLevelWithPitch(float level) {
		level = Mathf.Max(level, 0.0f);
		// LightbulbMaterial.SetShaderParameter("alpha", level);
		Light.LightEnergy = level / 4.0f;
		ambience.VolumeLinear = level / 16.0f;
        ambience.PitchScale = level;
		if (!ambience.Playing) {
			ambience.Play();
		}
	}
	public void SetLightLevel(float level) {
		level = Mathf.Max(level, 0.0f);
		// LightbulbMaterial.SetShaderParameter("alpha", level);
		Light.LightEnergy = level / 4.0f;
		ambience.VolumeLinear = level / 16.0f;
		if (!ambience.Playing) {
			ambience.Play();
		}
	}
}
