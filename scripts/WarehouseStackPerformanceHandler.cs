using Godot;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GArray = Godot.Collections.Array<Godot.Node>;

public partial class WarehouseStackPerformanceHandler : Node
{
	[ExportGroup("Objects")]
	[Export] public PlayerController plr;
	[Export] public Node3D boxTemplate;
	[Export] public Node3D warehouseLargeStackTemplate;
	[Export] public Node LoadedWarehouseLargeStacksParent;
	[Export] public Node UnloadedWarehouseLargeStacksParent;
	[Export] public Node PlaceholderWarehouseLargeStacksParent;
	
	[ExportGroup("Values")]
	[Export] public float distanceToLoadStacksSquared;
	[Export] public float distanceToLoadBoxes;
	[Export] public float timeBetweenChecks;
	[Export] public float timeBetweenBoxStackSpawnsMSECS;

	private bool isFirstLoad = true;
	private float timeOfNextLoop;
	
	/*
	Unsuprisingly having thousands of warehouse table things (called stacks in game) can cause performance issues
	This script is fundementally a level of detail manager.
	*/


	private Node3D GetRandomBox()
	{
		Node3D duplicateBox = (Node3D)boxTemplate.Duplicate(7);
		uint boxNum = (GD.Randi() % 3) + 1;
		//GD.Print(boxNum.ToString());
		Node3D correctMesh = duplicateBox.GetNode<Node3D>(boxNum.ToString());
		correctMesh.Visible = true;
		return duplicateBox;
	}
	private async Task HandleBoxLODForStack(WarehouseStack stack)
	{
		Node spawnLocationsParent = stack.warehouseSpawnLocations;
		Node spawnedBoxesParent = stack.warehouseBoxNode;
		
		foreach (Node3D spawnLocation in spawnLocationsParent.GetChildren())
		{
			await Task.Delay((int)timeBetweenBoxStackSpawnsMSECS);
			Node3D box = GetRandomBox();
			spawnedBoxesParent.AddChild(box);
			box.Position = spawnLocation.Position;
			box.RotateY(GD.RandRange(-5,5));
		}
	}
	private void HandleStackBoxDestruction()
	{
		foreach (WarehouseStackConnector connector in UnloadedWarehouseLargeStacksParent.GetChildren())
		{
			if (connector.BoxesBusy == false && connector.IsBoxed == true)
			{
				connector.IsBoxed = false;
				foreach(WarehouseStack stack in connector.GetChildStacks())
				{
					foreach(Node3D box in stack.warehouseBoxNode.GetChildren())
					{
						box.QueueFree();
					}
				}
			}
		}
	}
	private async Task HandleBoxLOD()
	{
		//GD.Print("Check");
		// handles LOD on already loaded stacks
		foreach (Node connectorNode in LoadedWarehouseLargeStacksParent.GetChildren())
		{
			if (connectorNode is not WarehouseStackConnector) { continue; }
			//GD.Print("IsWarehouseStackConnector");
			WarehouseStackConnector connector = (WarehouseStackConnector) connectorNode;
			if (connector.IsBoxed || connector.BoxesBusy) {continue;}

			GArray stackArray = connector.GetChildStacks();

			connector.BoxesBusy = true;
			foreach (Node stackNode in stackArray)
			{
				if (stackNode is not WarehouseStack) {continue;}
				
				//GD.Print("IsWarehouseStack");
				
				await HandleBoxLODForStack((WarehouseStack)stackNode);
			}
			connector.IsBoxed = true;
			connector.BoxesBusy = false;
		}
	}

	private void CheckLoadedWarehouseStacks()
	{
		// determine which warehouse stacks should be loaded with boxes and which should be discarded void of boxes
		foreach (Node3D loadNode in UnloadedWarehouseLargeStacksParent.GetChildren())
		{
			if (loadNode is not WarehouseStackConnector) {continue;}
			float distance = (loadNode.GlobalPosition - plr.rigidbody.GlobalPosition).Length();
			if (distance < distanceToLoadBoxes)
			{
				//GD.Print("reparented");
				WarehouseStackConnector connector = (WarehouseStackConnector) loadNode;
				if (connector.BoxesBusy == false)
				{
					loadNode.Reparent(LoadedWarehouseLargeStacksParent);
				}
			}
		}
		foreach (Node3D loadNode in LoadedWarehouseLargeStacksParent.GetChildren())
		{
			if (loadNode is not WarehouseStackConnector) {continue;}
			float distanceSquared = (loadNode.GlobalPosition - plr.rigidbody.GlobalPosition).LengthSquared();
			float distance = (loadNode.GlobalPosition - plr.rigidbody.GlobalPosition).Length();
			WarehouseStackConnector connector = (WarehouseStackConnector) loadNode;
			if (distance > distanceToLoadBoxes)
			{
				//GD.Print("reparented");
				if (connector.BoxesBusy == false)
				{
					loadNode.Reparent(UnloadedWarehouseLargeStacksParent);
				}
			}
			if (distanceSquared > distanceToLoadStacksSquared)
			{
				StackTemplate placeholder = connector.placeholder;
				placeholder.IsLoaded = false;
				connector.QueueFree();
			}
		}
	}
	private void LoadPlaceholder(StackTemplate placeholder)
	{
		placeholder.IsLoaded = true;
		WarehouseStackConnector newConnector = (WarehouseStackConnector)warehouseLargeStackTemplate.Duplicate(7);
		UnloadedWarehouseLargeStacksParent.AddChild(newConnector);
		newConnector.Visible = true;
		newConnector.GlobalPosition = placeholder.GlobalPosition;
		newConnector.placeholder = placeholder;
	}
	private void CheckUnloadedWarehouseStacks()
	{
		// determine which warehouse placeholders should be loaded into stacks and which should be removed
		foreach (Node3D placeholderNode in PlaceholderWarehouseLargeStacksParent.GetChildren())
		{
			StackTemplate placeholder = (StackTemplate) placeholderNode;
			if (isFirstLoad)
			{
				placeholder.DestroyMesh();
			}
			if (placeholder.IsLoaded == false)
			{
				float distanceSquared = (plr.rigidbody.GlobalPosition - placeholder.GlobalPosition).LengthSquared();
				if (distanceSquared < distanceToLoadStacksSquared)
				{
					LoadPlaceholder(placeholder);
				}
			}
		}
			
	}

	public override void _PhysicsProcess(double delta)
	{
		float now = Time.GetTicksMsec();
		if (now > timeOfNextLoop)
		{
			timeOfNextLoop = now + timeBetweenChecks * 1000;
			CheckLoadedWarehouseStacks();
			CheckUnloadedWarehouseStacks();
			HandleBoxLOD();
			HandleStackBoxDestruction();
			isFirstLoad = false;
		}
	}
}
