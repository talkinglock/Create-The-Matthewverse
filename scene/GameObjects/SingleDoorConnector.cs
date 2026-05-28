using Godot;
using System;

public partial class SingleDoorConnector : Node3D
{
	// Called when the node enters the scene tree for the first time.
	
	[Export] public SingleDoor door;

	public void OpenDoor(Node3D objToReference, float time)
	{
		door.OpenDoor(objToReference, time);
	}
}
