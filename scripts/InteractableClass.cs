using Godot;
using Godot.NativeInterop;
using System;
using System.ComponentModel;
using System.Diagnostics;

public interface IInteractable
{
	void OnInteract(PlayerController plrController);
	void OnHover(PlayerController plrController);
	void OnHoverStop(PlayerController plrController);
}

public partial class InteractableClass : Node
{
	[ExportGroup("Objects")]
	[Export] public PlayerController plrController;
	[Export] public RayCast3D caster; 
	

	private GodotObject lastCollider;
	private string lastType;
	private void Interact(GodotObject collider, string type)
	{
		if (collider is IInteractable Iinteractable)
		{
			switch(type)
			{
				case "interact":
					{
						Iinteractable.OnInteract(plrController);
						break;
					}
				case "hover":
					{
						if (collider == lastCollider && lastType == type) {return;}
						Iinteractable.OnHover(plrController);
						break;
					}
				case "hoverstop":
					{
						if (collider == lastCollider && lastType == type) {return;}
						Iinteractable.OnHoverStop(plrController);
						break;
					}
				default:
					{
						break;
					}
			}
			lastType = type;
		}
		else
		{
			Debug.WriteLine("Not IInteractable");
		}
	}
	private void TryInteraction()
	{
		if (!caster.IsColliding()) {
			if (lastCollider != null)
			{
				Interact(lastCollider, "hoverstop");
			}
			return;
		}

		if (plrController.IsHoldingObject()) {return;}
		lastCollider = caster.GetCollider();
		if (Input.IsKeyPressed(Key.E))
		{
			Interact(lastCollider, "interact");
		}
		else
		{	
			
			Interact(lastCollider, "hover");
		}
	}
	public void Update()
	{
		TryInteraction();
	}
}
