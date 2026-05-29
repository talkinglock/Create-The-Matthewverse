using Godot;
using System;

public partial class SingleDoorConnector : Node3D
{
	// Called when the node enters the scene tree for the first time.
	
	[Export] public SingleDoor door;
	[Export] public bool InitialCanOpen = true;

	public override void _Ready()
	{
		door.Interactable = InitialCanOpen;
	}
	public void SetCanOpen(bool value)
	{
		door.Interactable = value;
	}
	public void OpenDoor(Node3D objToReference, float time)
	{
		door.OpenDoor(objToReference, time);
	}
}
