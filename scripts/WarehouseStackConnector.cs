using Godot;
using System;
using GArray = Godot.Collections.Array<Godot.Node>;
public partial class WarehouseStackConnector : Node3D
{
	[Export] public PlayerController plr;
	[Export] public float checkDistance;
	[Export] public float checkTime;
	[Export] public StackTemplate placeholder;
	[Export] public Node3D Spawn1Start;
	[Export] public Node3D Spawn1End;
	[Export] public Node3D Spawn2Start;
	[Export] public Node3D Spawn2End;
	[Export] public Node3D Spawn3Start;
	[Export] public Node3D Spawn3End;
	[Export] public Node3D Spawn4Start;
	[Export] public Node3D Spawn4End;
	[Export] public Area3D line1;
	[Export] public Area3D line2;
	[Export] public Area3D line3;
	[Export] public Area3D line4;
	public bool IsBoxed = false;
	public bool BoxesBusy = false;

	private Vector3 Measure = new Vector3(0,0,1);
	public enum Direction
	{
		ToTheLeft,
		ToTheRight
	}

	public enum Line
	{
		First,
		Second,
		Third,
		Fourth,
		None
	}
	public Line GetPlayerLine()
	{
		if (line1.GetOverlappingBodies().Count != 0)
		{
			return Line.First;
		}
		else if (line2.GetOverlappingBodies().Count != 0)
		{
			return Line.Second;
		}
		else if (line3.GetOverlappingBodies().Count != 0)
		{
			return Line.Third;
		}
		else if (line4.GetOverlappingBodies().Count != 0)
		{
			return Line.Fourth;
		}
		return Line.None;
	}
	public void Materialize(Hallucinator hallucinator, StackTemplate placeholderPassed)
	{
		Visible = true;
		GlobalPosition = placeholderPassed.GlobalPosition;
		GlobalRotation = placeholderPassed.GlobalRotation;
		placeholder = placeholderPassed;
	}
	public Direction GetDirectionToPosition(Vector3 pos)
	{
		Vector3 localizedPos = ToLocal(pos);
		float alignment = localizedPos.Dot(Measure);
		if (alignment > 0)
		{
			return Direction.ToTheLeft;
		}
		else
		{
			return Direction.ToTheRight;
		}
	}
	public Vector3 GetRandomInsidePosition(Line line=Line.None)
	{
		float locationNum = 0;
		if (line == Line.None)
		{
			locationNum = (GD.Randi() % 4) + 1;
		}
		else if (line == Line.First)
		{
			locationNum = 1f;
		}
		else if (line == Line.Second)
		{
			locationNum = 2f;
		}
		else if (line == Line.Third)
		{
			locationNum = 3f;
		}
		else if (line == Line.Fourth)
		{
			locationNum = 4f;
		}
		float randNormalized = GD.RandRange(0,1);
		Node3D start = null;
		Node3D end = null;
		if (locationNum == 1)
		{
			start = Spawn1Start;
			end = Spawn1End;
		}
		else if (locationNum == 2)
		{
			start = Spawn2Start;
			end = Spawn2End;
		}
		else if (locationNum == 3)
		{
			start = Spawn3Start;
			end = Spawn3End;
		}
		else if (locationNum == 4)
		{
			start = Spawn4Start;
			end = Spawn4End;
		}
		Vector3 endPos = start.GlobalPosition.Lerp(end.GlobalPosition, randNormalized);
		return endPos;
	}
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
