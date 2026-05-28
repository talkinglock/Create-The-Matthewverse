using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class ChapterTitle : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Label label;
	[Export] public Panel panel;
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
		Tween opacityTween = GetTree().CreateTween();
		opacityTween.TweenProperty(panel, "modulate", new Color(1.0f, 1.0f, 1.0f, opacity), time);
	}

}
