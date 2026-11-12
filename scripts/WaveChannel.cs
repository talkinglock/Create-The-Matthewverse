using Godot;
using System;

public partial class WaveChannel : Node, ICrtChannel
{
	[Export] public Color LightColor;
	[ExportGroup("Objects")]
	[Export] public TextureRect WaveTemplate;
	[Export] public Control Base;
	[ExportGroup("Tweakables")]
	[Export] public Vector2 TopLeft;
	[Export] public float WaveScaleMultiplier;
	[Export] public float RadiusOfDestruction;
	[Export] public float TimeBetweenSpawnsStart;
	[Export] public float TimeBetweenSpawnsEnd;
	[Export] public float TimeOfExpansion;
	[Export] public float TimeOfVariation;
	[Export] public Tween.TransitionType TimeBetweenSpawnsTransType;

	private Standard std;
	private float timeBetweenSpawns;
	private float timeOfNextSpawn;
	private float timer = 0.0f;
	private bool active = false;
	public Color GetLightColor() {return LightColor;}

	private void SetTimeBetweenSpawns(float alpha) {
		timeBetweenSpawns = alpha;
	}

	public void OnChannelHit(float time) {
		timer = 0.0f;
		timeOfNextSpawn = 0.0f;
		timeOfNextSpawn = TimeBetweenSpawnsStart;
		active = true;
		Base.Visible = true;
		Tween tween = GetTree().CreateTween().SetTrans(TimeBetweenSpawnsTransType);
		tween.TweenMethod(Callable.From<float>(SetTimeBetweenSpawns), TimeBetweenSpawnsStart, TimeBetweenSpawnsEnd, time);
		tween.Play();
	}

	public void OnChannelLost() {
		active = true;
		Base.Visible = false;
		foreach (Node node in Base.GetChildren()) {
			if (node.Name != "Background") {
				node.QueueFree();
			}
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (!active) {
			return;
		}
		if (timeOfNextSpawn < timer) {
			timer = 0.0f;
			timeOfNextSpawn = timeBetweenSpawns;
			float x = (float) GD.RandRange(TopLeft.X, TopLeft.X + 1280);
			float y = (float) GD.RandRange(TopLeft.Y, TopLeft.Y + 720);
			TextureRect wave = (TextureRect) WaveTemplate.Duplicate(7);
			wave.Visible = true;
			Base.AddChild(wave);
			wave.Position = new(x, y);
			Tween tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
			tween.TweenProperty(wave, "scale", new Vector2(RadiusOfDestruction, RadiusOfDestruction), std.AddVariance(TimeOfExpansion, TimeOfVariation));
			tween.Play();
		}
		timer += (float) delta;
	}
	public override void _Ready() {
		std = new();
	}
}
