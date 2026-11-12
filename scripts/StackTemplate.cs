using Godot;
using System;

public partial class StackTemplate : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public bool IsLoaded = false; 
	[Export] public bool doesHaveBox = true;
	[Export] public bool acceptableSpawnArea = true;
	public bool isDisabled = false;
	public void DestroyMesh()
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
	}
}
