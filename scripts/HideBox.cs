using Godot;
using System;
using System.Threading.Tasks;

public partial class HideBox : Node3D, IInteractable
{
	[Export] public float minHideDistance;
	[Export] public float tweenTime;
	[Export] public CollisionShape3D collider;
	[Export] public Node3D hidePos;
	[Export] public uint activeOdds;
	[Export] public bool active;
	private PlayerController plr;
	private ChapterTitle title;
	private bool IsHovered = false;
	private bool busy = false;
	public bool canUnhide = true;
	private bool hidden = false;

	private Vector3 exitPosPlayer;
	private Vector3 exitRot;
	private bool doHoverCheck = false;
	private async Task Unhide()
	{
		if (!canUnhide) {return;}
		busy = true;
		hidden = false;
		plr.IsHiding = false;
		Tween playerTween = GetTree().CreateTween();
		playerTween.SetParallel(true);
		playerTween.TweenProperty(plr.rigidbody, "global_position", exitPosPlayer, tweenTime);
		playerTween.TweenProperty(plr.rigidbody, "rotation", exitRot, tweenTime);
		await ToSignal(playerTween, Tween.SignalName.Finished);
		plr.CanMove = true;
		plr.CanUseFlashlight = true;
		plr.CanRotate = true;
		plr.rigidbody.Freeze = false;
		busy = false;
	}
	private async Task Hide()
	{
		busy = true;
		hidden = true;
		plr.IsHiding = true;
		plr.CanMove = false;
		plr.CanRotate = false;
		plr.CanUseFlashlight = false;
		plr.rigidbody.Freeze = true;
		exitPosPlayer = plr.rigidbody.GlobalPosition;
		exitRot = plr.rigidbody.Rotation;
		if (plr.isFlashlightEquipped)
		{
			plr.UnequipFlashlight();
		}
		
		Tween playerTween = GetTree().CreateTween();
		playerTween.SetParallel(true);
		playerTween.TweenProperty(plr.rigidbody, "global_position", hidePos.GlobalPosition, tweenTime);
		playerTween.TweenProperty(plr.rigidbody, "rotation", hidePos.Rotation, tweenTime);
		await ToSignal(playerTween, Tween.SignalName.Finished);
		busy = false;
		plr.CanRotate = true;
	}
	public void OnInteract(PlayerController plrcon)
	{
		if (!active) {return;}
		plr = plrcon;
		title = plr.GetChapterTitle();
		if (busy == false && hidden == false)
		{
			Hide();
		}
	}
	public void OnHover(PlayerController plrcon)
	{
		plr = plrcon;
		doHoverCheck = true;
	}
	public void OnHoverStop(PlayerController plrcon)
	{
		if (!active) {return;}
		doHoverCheck = false;
		plr = plrcon;
		title = plr.GetChapterTitle();
		if (IsHovered)
		{
			IsHovered = false;
			title.StopInteract();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (doHoverCheck)
		{
			if (!active) {return;}
			title = plr.GetChapterTitle();
			float distance = (plr.rigidbody.GlobalPosition - GlobalPosition).Length();
			if (!IsHovered && distance < minHideDistance && hidden == false && busy == false)
			{
				IsHovered = true;
				doHoverCheck = false;
				title.Interact("E - Hide");
			}
		}

		if (hidden && busy == false)
		{
			if (Input.IsKeyPressed(Key.E))
			{
				Unhide();
			}
		}
	}
	public override void _Ready()
	{
		if ((GD.Randi() % activeOdds + 1) == 1)
		{
			GD.Print("Summoning");
			active = true;
			collider.Disabled = false;
			Visible = true;
		}
	}
	
}
