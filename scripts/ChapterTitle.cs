using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class ChapterTitle : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Label label;
	[Export] public Panel panel;
	[Export] public Panel InteractPanel;
	[Export] public Label InteractText;
	[Export] public AudioStreamPlayer3D InteractSound;
	private bool strictApplied = false;
	private string lastText = "";
	public async Task ShowTitle(string title, float timeToShow)
	{
		label.Text = title;
		Tween opacityTween = GetTree().CreateTween();
		opacityTween.TweenProperty(label, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.2f);
		await ToSignal(GetTree().CreateTimer(timeToShow), SceneTreeTimer.SignalName.Timeout);
		opacityTween = GetTree().CreateTween();
		opacityTween.TweenProperty(label, "modulate", new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.2f);
	}
	public async Task SetBlackoutOpacity(float opacity, float time)
	{
		if (time == -1)
		{
			panel.Modulate = new Color(1.0f, 1.0f, 1.0f, opacity);
		}
		else
		{
			Tween opacityTween = GetTree().CreateTween();
			opacityTween.TweenProperty(panel, "modulate", new Color(1.0f, 1.0f, 1.0f, opacity), time);
		}
		
	}
	public void Interact(string text, bool strict=false)
	{
		if (strictApplied == true && strict == false) {return;}
		lastText = text;
		if (strict)
		{
			strictApplied = true;
		}
		InteractText.Text = text;
		if (InteractSound.Playing == true)
		{
			InteractSound.Stop();
		}
		InteractSound.Play();
		InteractPanel.Visible = true;
	}

	public async Task InteractTimed(string text, float time, bool strict=false, bool textCheck=true)
	{
		if (strictApplied == true && strict == false) {return;}
		lastText = text;
		if (strict)
		{
			strictApplied = true;
		}
		Interact(text);
		await Task.Delay((int)(time * 1000));
		if (text == lastText && textCheck)
		{
			StopInteract();
		}
		else if (!textCheck)
		{
			StopInteract();
		}
	}
	public void StopInteract(bool strict=false)
	{
		if (strictApplied == true && strict == false) {return;}
		strictApplied = false;
		InteractPanel.Visible = false;
	}


}
