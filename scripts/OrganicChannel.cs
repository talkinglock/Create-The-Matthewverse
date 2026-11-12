using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class OrganicChannel : Node, ICrtChannel
{
	[Export] public Color LightColor;

	[ExportGroup("Objects")]
	[Export] public Control Base;
	[Export] public TextureRect VeinTemplate;
	[ExportGroup("Tweakables")]
	[Export] public float InitialHorizontalScale;
	[Export] public float HorizontalScaleIncrement;
	[Export] public Array<Vector2> InterestPositions;
	[Export] public float MaxDistanceFromInterest;
	[Export] public float ChanceOfFailure;
	[Export] public float BaseChanceOfVein;
	[Export] public float VeinAdditionalScaleMultiplier;
	[Export] public float Speed;
	[Export] public float SpeedVariance;
	[Export] public float Distance;
	[Export] public float DistanceVariance;
	[Export] public Vector2 center;
	// Basically an organic-looking tree structure that grows from a central point.
	// Some branches are false grows, meaning they grow out but decide to disappear
	// Each branch point can have 0-3 branches, with one or more being false grows.
	// The closer the branches are to specified positions the more likely they are to grow,

	private Standard std;

	private int count = 0;
	private bool active = false;

	private async Task CreateBranch(Vector2 parentOrigin, float parentRotation, float offsetRotation, float currentScale) {
		if (!active) {return;}
		count += 1;
		if (count > 1000) {
			return;
		}
		float newScale = currentScale - HorizontalScaleIncrement;
		TextureRect vein = (TextureRect) VeinTemplate.Duplicate(7);
		float rot = parentRotation + offsetRotation;
		Base.AddChild(vein);
		vein.Visible = true;
		vein.Position = parentOrigin;
		vein.Rotation = rot + 3.14f / 2.0f;


		float distance = std.AddVariance(Distance, DistanceVariance);
		float speed = std.AddVariance(Speed, SpeedVariance);
		float time = distance / speed;

		Vector2 endPos = new Vector2(
			Mathf.Cos(rot) * distance,
			Mathf.Sin(rot) * distance
		) + parentOrigin;
		
		Vector2 midPoint = parentOrigin.Lerp(endPos, 0.5f);
		float scale = distance * VeinAdditionalScaleMultiplier;
		vein.Scale = new(newScale, vein.Scale.Y);
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(vein, "scale", new Vector2(newScale, scale), time);
		tween.TweenProperty(vein, "position", midPoint, time);
		tween.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
		
		if ((float) GD.Randf() < ChanceOfFailure) {
			tween = GetTree().CreateTween();
			tween.SetParallel(true);
			tween.SetTrans(Tween.TransitionType.Linear);
			tween.TweenProperty(vein, "scale", new Vector2(newScale, 0.0f), time);
			tween.TweenProperty(vein, "position", parentOrigin, time);
			tween.Play();
			await ToSignal(tween, Tween.SignalName.Finished);
			return;
		}
		float likelyhood = 0.0f;
		foreach (Vector2 pos in InterestPositions) {
			float dist = (pos - endPos).Length();
			if (dist > MaxDistanceFromInterest) {continue;}
			float normalizedDistance = dist / MaxDistanceFromInterest;
			float localLikelyhood = Mathf.Abs(normalizedDistance - 1);
			likelyhood = Mathf.Max(likelyhood, localLikelyhood);
		}

		float angle =  -(3.14f / 4.0f) + (float) GD.RandRange(-0.25, 0.25);
		for (int i = 0; i < 3; i++) {
			angle += 3.14f / 4.0f + (float) GD.RandRange(-0.25, 0.25);
			float roll = GD.Randf();
			if (roll > BaseChanceOfVein || roll > likelyhood) {
				continue;
			}
			CreateBranch(endPos, rot - 3.14f / 4.0f, angle, newScale);
		}
	}

	

	public void OnChannelHit(float allocatedTime) {
		active = true;
		Base.Visible = true;
		count = 0;
		CreateBranch(center, 0.0f, 3.14f / 2.0f, InitialHorizontalScale);
		CreateBranch(center, 0.0f, -(3.14f / 2.0f), InitialHorizontalScale);
	}

	public void OnChannelLost() {
		Base.Visible = false;
		foreach (var child in Base.GetChildren()) {
			if (child.Name != "Background") {
				child.QueueFree();
			}
		}
		count = 0;
		active = false;
	}

	public override void _Ready() {
		VeinAdditionalScaleMultiplier /= 10.0f;
		std = new Standard();
	}

	public Color GetLightColor() {return LightColor;}
}
