using Godot;
using System;

public partial class StackTemplate : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public bool IsLoaded = false; 

	public void DestroyMesh()
	{
		GetNode("Mesh").QueueFree();	
	}
}
