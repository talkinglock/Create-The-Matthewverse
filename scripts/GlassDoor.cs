using Godot;
using System;
using System.Diagnostics;

public partial class GlassDoor : Node3D
{
	
	[Export] public PlayerController plr;
	[Export] public int DetectionRadius;
	[Export] public int ignoreRadiusTaxiCab;
	[Export] public float timeToOpen;
	[Export] public AudioStreamPlayer3D doorOpen;
	[Export] public AudioStreamPlayer3D doorClose;
	[Export] public bool Enabled = true;
	private RigidBody3D playerRigid;
	private StaticBody3D doorStatic;
	private CollisionShape3D doorCollider;
	private Node3D doorLeft;
	private Node3D doorRight;
	private bool IsOpen = false;

	public override void _Ready()
	{
		playerRigid = plr.rigidbody;
		doorStatic = GetNode<StaticBody3D>("Static");
		doorCollider = doorStatic.GetNode<CollisionShape3D>("Collider");
		doorLeft = GetNode<Node3D>("DoorLeft");
		doorRight = GetNode<Node3D>("DoorRight");
		if (doorStatic == null || doorLeft == null || doorRight == null)
		{
			GD.PrintErr("DoorStatic or DoorLeft or DoorRight doesn't exist.");
		}
	}

	private void OpenDoor()
	{
		IsOpen = true;
		doorCollider.Disabled = true;
		Tween doorTween = GetTree().CreateTween();
		doorOpen.Play();
		doorTween.SetParallel(true);
		doorTween.TweenProperty(doorLeft, "position", new Vector3(-0.049f, 0.0f, 2.49f), timeToOpen);
		doorTween.TweenProperty(doorRight, "position", new Vector3(-0.227f, 0.0f, -2.49f), timeToOpen);
		doorTween.Play();
	}
	private void CloseDoor()
	{
		IsOpen = false;
		doorCollider.Disabled = false;
		Tween doorTween = GetTree().CreateTween();
		doorClose.Play();
		doorTween.SetParallel(true);
		doorTween.TweenProperty(doorLeft, "position", new Vector3(-0.049f, 0.0f, 1.159f), timeToOpen);
		doorTween.TweenProperty(doorRight, "position", new Vector3(-0.227f, 0.0f, -1.159f), timeToOpen);
		doorTween.Play();
	}
	private void SetDoorStatus(bool willDoorOpen)
	{
		if (willDoorOpen == IsOpen)
		{
			return;
		}
		if (willDoorOpen == true)
		{
			OpenDoor();
		}
		else
		{
			CloseDoor();
		}
		
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled) {return;}
		Vector3 relativeVector = new Vector3(
			playerRigid.GlobalPosition.X - doorStatic.GlobalPosition.X,
			playerRigid.GlobalPosition.Y - doorStatic.GlobalPosition.Y,
			playerRigid.GlobalPosition.Z - doorStatic.GlobalPosition.Z
		);
		float taxiCab = relativeVector.LengthSquared();
		if (taxiCab <= ignoreRadiusTaxiCab)
		{
			//GD.Print("Within ignore radius.");
			float pythagDistance = relativeVector.Length();
			if (pythagDistance <= DetectionRadius)
			{
				//GD.Print("Within detection radius!");
				SetDoorStatus(true);
			} 
			else
			{
				SetDoorStatus(false);
			}
		}
		else
		{
			SetDoorStatus(false);
		}
	}
}
