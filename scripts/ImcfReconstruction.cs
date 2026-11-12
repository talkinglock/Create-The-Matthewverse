using Godot;
using Godot.Collections;
using System;

public partial class ImcfReconstruction : Node, ICrtChannel
{
	[Export] public Color LightColor;
	[Export] public Control Base;
	[ExportGroup("Textures")]
	[Export] public Node IParent;
	[Export] public Node MParent;
	[Export] public Node CParent;
	[Export] public Node FParent;
	[Export] public TextureRect Pillar;
	[ExportGroup("Positions")]
	[Export] public Vector2 IPos;
	[Export] public Vector2 MPos;
	[Export] public Vector2 CPos;
	[Export] public Vector2 FPos;
	[Export] public float PillarRadius;
	[Export] public Vector2 Center;
	[ExportGroup("Tweakables")]
	[Export] public float MaxScale;
	[Export] public float MinScale;
	[Export] public Vector2 PositionVariation;
	[Export] public float Speed;
	[Export] public float SpeedVariation;
	[Export] public float MinX;
	[Export] public float MaxX;
	[Export] public float MinY;
	[Export] public float MaxY;

	private bool active = false;
	private Standard std;


	private Tween ITween;
	private Tween MTween;
	private Tween CTween;
	private Tween FTween;
	private Array<Tween> pillarsTween;
	private Array<Vector2> pillarPositions;

	private TextureRect I;
	private TextureRect M;
	private TextureRect C;
	private TextureRect F;
	private Array<TextureRect> pillars;

	private TextureRect GetRandom(Node parent) {
		return (TextureRect) std.GetRandomChildFromParent(parent);
	}

	private TextureRect Instantiate(TextureRect main) {
		TextureRect obj = (TextureRect) main.Duplicate(7);
		Base.AddChild(obj);
		obj.Visible = true;
		float x = (float)GD.RandRange(MinX, MaxX);
		float y = (float)GD.RandRange(MinX, MaxY);
		obj.Position = new Vector2(x, y);
		return obj;
	}

	public Color GetLightColor() {return LightColor;}

	public void OnChannelHit(float _) {
		Base.Visible = true;
		I = Instantiate(GetRandom(IParent));
		M = Instantiate(GetRandom(MParent));
		C = Instantiate(GetRandom(CParent));
		F = Instantiate(GetRandom(FParent));
		pillars = [];
		pillarsTween = [null, null, null, null, null, null, null, null];
		float angle = 3.14f/2.0f;
		for (int i = 0; i < 8; i++) {
			TextureRect pillar = Instantiate(Pillar);
			pillar.Rotation = angle;
			angle -= 3.14f/4.0f;
			pillars.Add(pillar);
		}
		active = true;
	}

	public void OnChannelLost() {
		if (active) {
			active = false;
			Base.Visible = false;
			I.QueueFree();
			M.QueueFree();
			C.QueueFree();
			F.QueueFree();
			foreach (TextureRect pillar in pillars) {
				pillar.QueueFree();
			}
			pillars = [];
		}
	}


	private void GoToRandPos(TextureRect obj, Tween objTween, Vector2 pos) {
		if (objTween != null) {
			if (objTween.IsRunning()) {
				return;
			}
		}
		Vector2 newPos = pos + new Vector2((float)GD.RandRange(-PositionVariation.X, PositionVariation.X), (float)GD.RandRange(-PositionVariation.Y, PositionVariation.Y));
		objTween = GetTree().CreateTween();
		//objTween.SetTrans((Tween.TransitionType)(GD.Randi() % 11));
		objTween.SetParallel(true);
		float distance = (obj.Position - newPos).Length();
		float xScale = (float)GD.RandRange(MinScale, MaxScale);
		float yScale = (float)GD.RandRange(MinScale, MaxScale);
		Vector2 newScale = new(xScale, yScale); 
		float time = distance / (Speed + (float)GD.RandRange(-SpeedVariation, SpeedVariation));
		objTween.TweenProperty(obj, "scale", newScale, time);
		objTween.TweenProperty(obj, "position", newPos, time);
		objTween.Play();
	}

	public override void _PhysicsProcess(double delta) {
		if (!active) {return;}
		GoToRandPos(I, ITween, IPos);
		GoToRandPos(M, MTween, MPos);
		GoToRandPos(C, CTween, CPos);
		GoToRandPos(F, FTween, FPos);
		for (int i = 0; i < 8; i++) {
			GoToRandPos(pillars[i], pillarsTween[i], pillarPositions[i]);
		}
	}

	public override void _Ready() {
		std = new Standard();
		pillarPositions = [];
		float angle = 0.0f;
		for (int i = 0; i < 8; i++) {
			float x = Center.X + PillarRadius * Mathf.Cos(angle);
			float y = Center.Y + PillarRadius * Mathf.Sin(angle);
			Vector2 pos = new(x, y);
			pillarPositions.Add(pos);
			angle -= Mathf.Pi / 4.0f;
		}
	}
}
