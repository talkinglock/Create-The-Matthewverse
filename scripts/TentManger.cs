using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class TentManger : Node
{
	[ExportGroup("Objects")]
	[Export] public Node3D tent;
	[Export] public Node3D physicsObjects;
	[Export] public PlayerController plr;
	[Export] public Node TentSpawnsParent;
	[Export] public StackTemplate FirstTemplate;
	[Export] public StackTemplate SecondTemplate;
	[Export] public StackTemplate ThirdTemplate;
	[ExportGroup("Values")]
	[Export] public float TimeWait;
	[Export] public float TimeWaitVariation; 
	[Export] public float MinimumDistanceForRelocation;
	[Export] public float ObjectDontMoveDistance;
	[Export] public int RelocationChance;
	private Standard std;
	private bool busy = false;
	private float timeOfNextChange;
	public void MoveTent()
	{
		
		GD.Print("Doing Impossibility Check");
		float tentDistance = (tent.GlobalPosition - plr.rigidbody.GlobalPosition).Length();
		if (tentDistance < MinimumDistanceForRelocation) {return;}
		float firstDistance = (((Node3D)(TentSpawnsParent.GetNode("1"))).GlobalPosition - plr.rigidbody.GlobalPosition).Length();
		float secondDistance = (((Node3D)(TentSpawnsParent.GetNode("2"))).GlobalPosition - plr.rigidbody.GlobalPosition).Length();
		float thirdDistance = (((Node3D)(TentSpawnsParent.GetNode("3"))).GlobalPosition - plr.rigidbody.GlobalPosition).Length();

		if (firstDistance < MinimumDistanceForRelocation && secondDistance < MinimumDistanceForRelocation && thirdDistance < MinimumDistanceForRelocation) {return;}
		if (FirstTemplate.IsLoaded || SecondTemplate.IsLoaded || ThirdTemplate.IsLoaded) {return;}
		GD.Print("Impossibility Check Passed");
		// If the past checks fail, it is literally impossible to relocate the tent right now.
		
		foreach (Node3D obj in physicsObjects.GetChildren())
		{
			if (obj is RigidBody3D)
			{
				RigidBody3D body = (RigidBody3D)obj;
				body.Freeze = true;
			}
		}
		busy = true;
		bool doSpawnCheck = true;
		Node3D selectedSpawn = null;
		StackTemplate selectedTemplate = null;
		while (doSpawnCheck)
		{
			Node3D tentSpawn = (Node3D)std.GetRandomChildFromParent(TentSpawnsParent);
			if (tentSpawn.Name == "1")
			{
				if (FirstTemplate.IsLoaded || firstDistance < MinimumDistanceForRelocation)
				{
					continue;
				}
				else
				{
					selectedTemplate = FirstTemplate;
				}
			}
			else if (tentSpawn.Name == "2")
			{
				if (SecondTemplate.IsLoaded || secondDistance < MinimumDistanceForRelocation)
				{
					continue;
				}
				else
				{
					selectedTemplate = SecondTemplate;
				}
			}
			else if (tentSpawn.Name == "3")
			{
				if (ThirdTemplate.IsLoaded || thirdDistance < MinimumDistanceForRelocation)
				{
					continue;
				}
				else
				{
					selectedTemplate = ThirdTemplate;
				}
			}
			selectedSpawn = tentSpawn;
			doSpawnCheck = false;
		}
		FirstTemplate.isDisabled = false;
		SecondTemplate.isDisabled = false;
		ThirdTemplate.isDisabled = false;
		
		selectedTemplate.isDisabled = true;
		foreach (Node3D obj in physicsObjects.FindChildren("*", "RigidBody3D"))
		{
			if (obj is RigidBody3D)
			{
				float distance = (obj.GlobalPosition - tent.GlobalPosition).Length();
				if (distance < ObjectDontMoveDistance)
				{
					RigidBody3D body = (RigidBody3D)obj;
					body.Freeze = true;
				}
			}
		}
		tent.GlobalPosition = selectedSpawn.GlobalPosition;
		foreach (Node3D obj in physicsObjects.FindChildren("*", "RigidBody3D"))
		{
			if (obj is RigidBody3D)
			{
				RigidBody3D body = (RigidBody3D)obj;
				body.Freeze = false;
			}
		}
		GD.Print("spawned");
		busy = false;
	}
	
	public override void _Ready()
	{
		std = new Standard();

		MoveTent();
		timeOfNextChange = std.GetRandomTimeInFuture(TimeWait, TimeWaitVariation);
	}
	public override void _PhysicsProcess(double delta)
	{
		float now = Time.GetTicksMsec();
		if (busy) {return;}
		if (now > timeOfNextChange)
		{
			timeOfNextChange = std.GetRandomTimeInFuture(TimeWait, TimeWaitVariation);
			if (!std.RandomChance(RelocationChance)) {return;}
			MoveTent();
		}
	}
}
