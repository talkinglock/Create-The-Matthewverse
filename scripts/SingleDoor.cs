using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class SingleDoor : Node3D, IInteractable
{
	[ExportGroup("Objects")]
	[Export] public StaticBody3D colliderStatic;
	[Export] public Node3D DoorHinge;
	[Export] public Node3D DoorPhysical;
	[Export] public AudioStreamPlayer3D openSound;
	[Export] public AudioStreamPlayer3D lockedDoorSound;

	[ExportGroup("Options")]
	[Export] public bool Interactable = true;
	[Export] public float DefaultTime;
	
	private bool isHover = false;

	private ChapterTitle title;
	public void OnHover(PlayerController plrController)
	{
		float distance = (plrController.rigidbody.GlobalPosition - DoorPhysical.GlobalPosition).Length();
		if (distance > 15) {return;}
		title = plrController.GetChapterTitle();
		if (Interactable && isHover == false)
		{
			isHover = true;
			plrController.GetChapterTitle().Interact("E - Open door");
		}
	}
	public void OnHoverStop(PlayerController plrController)
	{
		if (isHover == true)
		{
			isHover = false;
			plrController.GetChapterTitle().StopInteract();
		}
	}
	public void OnInteract(PlayerController plrController)
	{
		if (!Interactable) {
			if (lockedDoorSound.Playing == false)
			{
				lockedDoorSound.Play();
			}
			return;
		}
		Interactable = false;
		Debug.WriteLine("Interacting");
		OpenDoor(plrController.rigidbody, DefaultTime);
	}

	private float GetAngleSignFromBearing(Vector3 referencePosition)
	{
		Vector3 doorFVector = -(DoorHinge.Basis.Z).Normalized();
		Vector3 nPlayerToDoorVector = (DoorHinge.GlobalPosition - referencePosition).Normalized();
		float dot = doorFVector.Dot(nPlayerToDoorVector);

		if (dot > 0) { return 1; }
		return -1;
	}

	private void MoveToAngle(float angle, float time)
	{
		Tween tween = GetTree().CreateTween();
		if (angle < 0) {Debug.WriteLine("negative");}
		tween.TweenProperty(DoorHinge, "rotation", new Vector3(0,-angle,0), time);
	}

	public void OpenDoor(Node3D objToReference, float time)
	{
		if (title != null)
		{
			title.StopInteract();
		}
		openSound.Play();
		float sign = GetAngleSignFromBearing(objToReference.GlobalPosition);
		MoveToAngle((3.14f/2.0f) * sign, time);
		colliderStatic.QueueFree();
	}
}
