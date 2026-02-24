using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class SingleDoor : Node3D, IInteractable
{
	[ExportGroup("Objects")]
	[Export] public Node3D DoorHinge;
	[Export] public Node3D DoorPhysical;
	[ExportGroup("Options")]
	[Export] public bool Interactable = true;
	[Export] public float DefaultTime;


	public void OnInteract(PlayerController plrController)
	{
		if (!Interactable) {return;}
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
		tween.TweenProperty(DoorHinge, "rotation", new Vector3(0,angle,0), time);
	}

	public void OpenDoor(Node3D objToReference, float time)
	{
		float sign = GetAngleSignFromBearing(objToReference.GlobalPosition);
		MoveToAngle((3.14f/2.0f) * sign, time);
	}
}
