using Godot;
using System;

public partial class NoSignalChannel : Node, ICrtChannel
{
	[Export] public Color LightColor;
	[Export] public Control NoSignal;
	public Color GetLightColor() {return LightColor;}

	public void OnChannelHit(float _) {
		NoSignal.Visible = true;
	}

	public void OnChannelLost() {
		NoSignal.Visible = false;
	}
}
