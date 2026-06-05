using Godot;
using System;
using System.Net;
using System.Threading.Tasks;
public partial class Hallucinator : Node
{
	[Export] public PlayerController player;
	[Export] public int TimeBetweenScares;
	[Export] public int Variance;
	[Export] public int playerDistanceLower;
	[Export] public int playerDistanceUpper;
	[Export] public Node3D soundPlayer;
	[Export] public float ColorTweenTime;
	private Standard std;
	private float timeOfNextScare;

	public bool hallucinate = false;
	private bool flashlightDone = false;
	public override void _Ready()
	{
		GD.Randomize();
		std = new Standard();
		HandleFlashlight();
	}
	private void HandleSounds()
	{
		Node sound = std.GetRandomChildFromParent(soundPlayer);
		if (sound is not AudioStreamPlayer3D) {return;}
		AudioStreamPlayer3D soundStream = (AudioStreamPlayer3D) sound;
		Vector3 direction = Vector3.Forward.Rotated(Vector3.Up, GD.RandRange(-5,5));
		float mag = GD.RandRange(playerDistanceLower, playerDistanceUpper);
		Vector3 relativePosition = direction * mag;
		soundPlayer.GlobalPosition = player.rigidbody.GlobalPosition + relativePosition;
		soundStream.Play();
	}

	private void HandleFlashlight()
	{
		SpotLight3D flashlight = player.GetFlashLightLight();
		Tween tween = GetTree().CreateTween();
		Color color = new Color(GD.Randf(), GD.Randf(), GD.Randf(), GD.Randf());
		tween.TweenProperty(flashlight, "light_color", color, ColorTweenTime).SetTrans(Tween.TransitionType.Linear);
		flashlightDone = true;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!hallucinate) {return;}
		if (flashlightDone)
		{
			HandleFlashlight();
			flashlightDone = false;
		}
		float now = Time.GetTicksMsec();
		if (now > timeOfNextScare)
		{
			timeOfNextScare = now + ((GD.RandRange(-Variance, Variance) + TimeBetweenScares) * 1000);
			HandleSounds();
			
		}
	
	}
}
