using Godot;
using System;
using System.Collections;
using System.Dynamic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GArray = Godot.Collections.Array<Godot.Node>;
using Line = WarehouseStackConnector.Line;
public partial class WarehouseStackPerformanceHandler : Node
{
	[ExportGroup("Objects")]
	[Export] public Hallucinator hallucinator;
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
	This script is fundamentally a level of detail manager.
	*/
	
	public Vector3 GetNearestBiasedSpawnPosition(Vector3 reference, float fogPosition=0, float maxDistance =0)
	{
		// Get a spawn position of something the player is likely to stumble upon.
		bool found = false;
		Line playersLine = Line.None;
		WarehouseStackConnector playerConnector = null;;
		if (plr.rigidbody.LinearVelocity.Length() == 0)
		{
			return Vector3.Zero;
		}
		foreach (WarehouseStackConnector connector in LoadedWarehouseLargeStacksParent.GetChildren())
		{
			Line line = connector.GetPlayerLine();
			if (line != Line.None)
			{
				playerConnector = connector;
				playersLine = line;
				found = true;
			}
		}
		if (!found)
		{
			return Vector3.Zero;
		}

		GArray inLineConnectors = new GArray();

		foreach (WarehouseStackConnector connector in LoadedWarehouseLargeStacksParent.GetChildren())
		{
			if (connector != playerConnector && connector.placeholder.acceptableSpawnArea == true)
			{
				
				Vector3 localizedPosition =  connector.GlobalPosition - playerConnector.GlobalPosition;
				
				if (Mathf.Abs(localizedPosition.X) < 2)
				{
					GD.Print(localizedPosition.X);
					Vector3 look = plr.camera.GlobalBasis.Z;
					float alignment = look.Dot((connector.GlobalPosition - plr.camera.GlobalPosition).Normalized());
					GD.Print(alignment);
					if (alignment < 0)
					{
						inLineConnectors.Add(connector);
					}
				}	
			}
		}
		Vector3 endPos = Vector3.Zero;
		while (endPos == Vector3.Zero)
		{
			GD.Print("Looping, " + inLineConnectors.Count);
			int randChildNum = (int)((GD.Randi() % inLineConnectors.Count));
			WarehouseStackConnector connector = (WarehouseStackConnector)inLineConnectors[randChildNum];
			Vector3 tempPos = connector.GetRandomInsidePosition(playersLine);
			GD.Print(tempPos);
			if (fogPosition != 0)
			{
				float distance = (tempPos - plr.rigidbody.GlobalPosition).Length();
				if (distance > fogPosition)
				{
					if (maxDistance != 0 && distance < maxDistance)
					{
						endPos = tempPos;
					}
					else if (maxDistance == 0)
					{
						endPos = tempPos;
					}
				}
			}
			else
			{
				endPos = tempPos;
			}
		}
		return endPos;
	}
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
			bool passed = true;
			if (hallucinator.state == Hallucinator.ScareState.StalkingFromAfar)
			{
				if (((GD.Randi() % hallucinator.chanceForBoxFailAfar) + 1) == 1)
				{
					passed = false;
				}
			}

			if (passed == true)
			{
				await Task.Delay((int)timeBetweenBoxStackSpawnsMSECS);
				Node3D box = GetRandomBox();
				spawnedBoxesParent.AddChild(box);
				box.Position = spawnLocation.Position;
				box.RotateY(GD.RandRange(-5,5));
			}
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
			if (connector.placeholder.doesHaveBox == true)
			{
				foreach (Node stackNode in stackArray)
				{
					if (stackNode is not WarehouseStack) {continue;}
					
					//GD.Print("IsWarehouseStack");
					
					await HandleBoxLODForStack((WarehouseStack)stackNode);
				}
			}
			connector.IsBoxed = true;
			connector.BoxesBusy = false;
			hallucinator.ConnectorFabricated(connector);
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
		newConnector.Materialize(hallucinator, placeholder);
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
			if (placeholder.isDisabled == false)
			{
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
