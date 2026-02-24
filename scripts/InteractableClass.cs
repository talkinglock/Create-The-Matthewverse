using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public interface IInteractable
{
	void OnInteract(PlayerController plrController);
}

public partial class InteractableClass : Node
{
	[ExportGroup("Objects")]
	[Export] public PlayerController plrController;
	[Export] public RayCast3D caster; 

	private void Interact(GodotObject collider)
	{
		if (collider is IInteractable Iinteractable)
		{
			Iinteractable.OnInteract(plrController);
		}
		else
		{
			Debug.WriteLine("Not IInteractable");
		}
	}
	private void TryInteraction()
	{
		if (Input.IsKeyPressed(Key.E))
		{
			if (plrController.IsHoldingObject()) {return;}
			//Debug.WriteLine("Not Holding Object");
			if (!caster.IsColliding()) {return;}
			//Debug.WriteLine("Caster Colliding");
			Interact(caster.GetCollider());
		}
	}
	public void Update()
	{
		TryInteraction();
	}
}
