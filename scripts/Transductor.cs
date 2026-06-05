using Godot;
using System;

public partial class Transductor : StaticBody3D, IInteractable
{
	// Called when the node enters the scene tree for the first time.
	[Export] public TransducerMain main;
	public bool isHovered = false;
	
	public void OnHover(PlayerController plr)
	{
		
		if (main.busy == false && isHovered == false)
		{
			float distance = (plr.rigidbody.GlobalPosition - GlobalPosition).Length();
			GD.Print(distance);
			if (distance < 15)
			{
				plr.CanHold = false;
				isHovered = true;
				plr.GetChapterTitle().Interact("E - Activate");
			}	
		}
	}
	public void OnHoverStop(PlayerController plr)
	{
		if (main.busy == false && isHovered == true)
		{
			plr.CanHold = true;
			isHovered = false;
			plr.GetChapterTitle().StopInteract();
		}
	}
	public void OnInteract(PlayerController plr)
	{
		//plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
		if (main.busy == false)
		{
			isHovered = false;
			main.busy = true;
			plr.GetChapterTitle().StopInteract();
			main.Activate();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
