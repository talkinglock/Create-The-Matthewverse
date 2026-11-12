using Godot;
using System;
using System.Threading.Tasks;

public partial class Button : StaticBody3D, IInteractable
{
	[Export] public Node3D presser;
	[Export] public float DownwardsDistance;
	[Export] public float minInteractDistance;
	[Export] public AudioStreamPlayer3D pressAudio;
	[Export] public string pressText = "E - Press Button";
	[Export] public bool enabled = true;
	public Action onPressed;

	private Vector3 ButtonStartPos;

	private PlayerController plr;
	private ChapterTitle title;
	private bool isHovering = false;
	private bool doHoverCheck = false;
	private bool busy = false;

	public override void _Ready() {
		ButtonStartPos = presser.Position;
	}

	public void OnInteract(PlayerController plrcon)
	{
		if (!enabled) {return;}
		plr = plrcon;
		title = plr.GetChapterTitle();
		if (busy == false)
		{
			Hide();
		}
	}
	public void OnHover(PlayerController plrcon)
	{
		if (!enabled) {return;}
		plr = plrcon;
		doHoverCheck = true;
	}
	public void OnHoverStop(PlayerController plrcon)
	{
		if (!enabled) {return;}
		doHoverCheck = false;
		plr = plrcon;
		title = plr.GetChapterTitle();
		if (isHovering)
		{
			isHovering = false;
			title.StopInteract();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!enabled) {return;}
		if (doHoverCheck)
		{
			title = plr.GetChapterTitle();
			float distance = (plr.rigidbody.GlobalPosition - GlobalPosition).Length();
			if (!isHovering && distance < minInteractDistance && busy == false)
			{
				isHovering = true;
				doHoverCheck = false;
				title.Interact(pressText);
			}
		}

		if (busy == false && isHovering == true)
		{
			if (Input.IsKeyPressed(Key.E))
			{
				busy = true;
				title.StopInteract();
				HandlePress();
				onPressed();
			}
		}
	}
	private async Task HandlePress() {
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(presser, "position", ButtonStartPos - new Vector3(0.0f, DownwardsDistance, 0.0f), 0.2f);
		tween.Play();
		pressAudio.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
		tween = GetTree().CreateTween();
		tween.TweenProperty(presser, "position", ButtonStartPos, 0.2f);
		tween.Play();
	}
}
