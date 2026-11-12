using Godot;
using System;

public partial class StaticChannel : Node, ICrtChannel
{
	[Export] public Color LightColor;
	[Export] public MeshInstance3D CrtMaterial;
	public Color GetLightColor() {return LightColor;}

	public void OnChannelHit(float _) {
		((ShaderMaterial)CrtMaterial.GetSurfaceOverrideMaterial(0)).SetShaderParameter("is_static", true);
	}

	public void OnChannelLost() {
		((ShaderMaterial)CrtMaterial.GetSurfaceOverrideMaterial(0)).SetShaderParameter("is_static", false);
	}
}
