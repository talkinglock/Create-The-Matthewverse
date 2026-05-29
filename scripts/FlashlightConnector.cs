using Godot;
using System;

public partial class FlashlightConnector : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Node3D flashlightBody;
	[Export] public Node3D startPos;
	[Export] public AudioStreamPlayer3D click;

}
