using Godot;
using System;
using GArray = Godot.Collections.Array<Godot.Node>;
public partial class WarehouseStackConnector : Node3D
{
	[Export] public PlayerController plr;
	[Export] public float checkDistance;
	[Export] public float checkTime;
	[Export] public StackTemplate placeholder;
	public bool IsBoxed = false;
	public bool BoxesBusy = false;
	public GArray GetChildStacks()
	{
		GArray children = new GArray();
		foreach (Node child in GetChildren())
		{
			if (child is WarehouseStack)
			{
				children.Add((WarehouseStack)child);
			}
		} 
		return children;
	}

}
