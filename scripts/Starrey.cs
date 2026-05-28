using Godot;
using System;

public partial class Starrey : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public VisibleOnScreenNotifier3D notifier;
	[Export] public RayCast3D raycast;
	[Export] public Person person;
	[Export] public RigidBody3D rigid;
	[Export] public AudioStreamPlayer3D sightSound;
	
	private PlayerController plr;

	public override void _Ready()
	{
		plr = person.player;
	}
	private void InSight()
	{
		if (sightSound.Playing == false)
		{
			sightSound.Play();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (notifier.IsOnScreen())
		{
			raycast.TargetPosition = (plr.rigidbody.GlobalPosition - raycast.GlobalPosition);
			GodotObject body = raycast.GetCollider();
			
			if (body != null && body is RigidBody3D)
			{
				RigidBody3D bodyRigid = (RigidBody3D) body;
				if (bodyRigid.Name == "PlrRigid")
				{
					InSight();
				}
				else
				{
					sightSound.Playing = false;
				}
			}
			else
			{
				sightSound.Playing = false;
			}
		}
		else
		{
			sightSound.Playing = false;
		}
	}
}
