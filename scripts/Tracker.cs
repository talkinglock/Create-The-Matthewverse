using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Tracker : Node3D
{
	[ExportGroup("Objects")]
	[Export] public Node3D Pointer;
	[Export] public Node3D StartReference;
	[Export] public Node3D MarkTemplate;
	[Export] public AudioStreamPlayer3D ServoSound;
	[ExportGroup("Values")]
	[Export] public float MovementHorizontalRadius;
	[Export] public int DisappearanceDistance;
	[Export] public float Speed;
	[Export] public float IncrementSpeed;
	[Export] public float Increment;

	private bool busy = false;

	private enum PointerStatus {
		LeftToRight,
		RightToLeft
	}
	private List<Node3D> marks = [];
	private PointerStatus pointerStatus = PointerStatus.LeftToRight;
	private float pointerPos = 0.0f;
	private Func<float> formula;
	// Formula that returns a result 0.0 to 1.0
	public void SetFormula(Func<float> formulaArg) {
		formula = formulaArg;
	}
	public void RemoveFormula() {
		formula = null;
	}
	private float MovementTime(float distanceNormalized) {
		return 1.0f / Speed;
	}
	// Move pen to position, -1.0 is left and 1.0 is right
	private async Task MovePenTo(float pos, float timeToMove) {
		Tween tween = GetTree().CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(Pointer, "position", new Vector3(pos * MovementHorizontalRadius, 0.0f, 0.0f), timeToMove);
		tween.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
		pointerPos = pos;
	}
	private async Task MoveMarks() {
		if (marks.Count == 0) {
			return;
		}
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.SetTrans(Tween.TransitionType.Linear);
		List<Node3D> newList = [null];
		for (int i = 0; i < marks.Count; i++) {
			Node3D mark = marks[i];
			if (mark != null) {
				tween.TweenProperty(mark, "position", mark.Position - new Vector3(0.0f, 0.0f, Increment), IncrementSpeed / Speed);
				newList.Add(mark);
			}
		}
		marks = newList;
		if (marks.Count > DisappearanceDistance + 1) {
			marks[marks.Count - 1].QueueFree();
			marks.RemoveAt(marks.Count - 1);
		}
		tween.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	private async Task Update() {
		if (Speed == 0.0f) {
			if (ServoSound.Playing) {
				ServoSound.Stop();
			}
		}
		if (busy) {
			return;
		}
		busy = true;

		if (formula != null) {
			float displacementNorm = Mathf.Clamp(formula(), 0.03f, 1.0f);
			float displacement = displacementNorm * MovementHorizontalRadius;
			float displacementMultiplier;
			float startPoint;
			if (!ServoSound.Playing) {
				ServoSound.Play();
			}
			if (pointerStatus == PointerStatus.LeftToRight) {
				pointerStatus = PointerStatus.RightToLeft;
				displacementMultiplier = -1.0f;
				startPoint = -displacementNorm;
				ServoSound.PitchScale = 1.2f;
			} else {
				pointerStatus = PointerStatus.LeftToRight;
				displacementMultiplier = 1.0f;
				startPoint = displacementNorm;
				ServoSound.PitchScale = 1.0f;
			}

			await MovePenTo(startPoint, MovementTime(Mathf.Abs(startPoint - pointerPos)));
			float endPoint = -startPoint;
			float movementTime =  MovementTime(Mathf.Abs(endPoint - startPoint));
			MovePenTo(endPoint, movementTime);
			Node3D mark = (Node3D)MarkTemplate.Duplicate((int)DuplicateFlags.Default);
			AddChild(mark);
			mark.Position = new Vector3(displacement * displacementMultiplier, 0.0f, 0.0f);
			Tween markTween = GetTree().CreateTween();
			markTween.SetParallel(true);
			markTween.SetTrans(Tween.TransitionType.Linear);
			markTween.TweenProperty(mark, "position", new Vector3(0.0f, 0.0f, 0.0f), movementTime);
			markTween.TweenProperty(mark, "scale", new Vector3(displacement * 100.0f, 0.58f, 0.58f), movementTime);
			markTween.Play();
			await ToSignal(markTween, Tween.SignalName.Finished);
			marks.Insert(0, mark);
		} else {
			ServoSound.Stop();
		}
		await MoveMarks();
		busy = false;
	}
	public override void _PhysicsProcess(double delta)
	{
		Update();
	}
}
