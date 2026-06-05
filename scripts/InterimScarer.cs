using Godot;
using System;
using System.Threading.Tasks;

public partial class InterimScarer : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public PlayerController plr;
	[Export] public WorldEnvironment env;
	[Export] public Node3D hallwayPos;
	[Export] public Node3D soundCarrier;
	[Export] public Node3D SoundsParent;
	[Export] public AudioStreamPlayer3D projectorBreathing;
	[Export] public Node3D projectorOutsidePos;
	[Export] public Node3D projectorInsidePos;
	[Export] public Node3D transOutsidePos;
	[Export] public Node3D transInsidePos;
	[Export] public AudioStreamPlayer3D doorOpen;
	[Export] public DecalSwitcher projectorSwitcher;
	[Export] public SpotLight3D projectorLight;
	[Export] public AudioStreamPlayer3D projectorIdle;
	[Export] public AudioStreamPlayer3D stomp;
	[Export] public AudioStreamPlayer3D stompHard;
	[Export] public AudioStreamPlayer3D chasingTalk;
	[Export] public AudioStreamPlayer ambience;
	[Export] public AudioStreamPlayer projectorBreathingGlobal;
	[Export] public HideBox tableHider;
	private Hallucinator hallucinator;
	private Tween movementTween;
	private bool Started = false;
	private float timeOfNextScare;
	private Standard std;
	public InterimScareMode mode;
	private float HallwayEndTime;
	public enum InterimScareMode
	{
		MatthewAtStart,
		MatthewGone,
		MatthewChasingHallway,
		PlayerInProjectorRoomHiding,
		PlayerBraveInHallway,
		PlayerInProjectorRoom,
		PlayerInTransducerRoom,
		PlayerInTransducerRoomNearDoor
	}
	public void Start()
	{
		mode = InterimScareMode.MatthewAtStart;
		std = new Standard();
		Started = true;
		plr.PlayMusic("SomewhereInBetween");	
		hallucinator = plr.hallucinator;
	}
	public async Task InterimToTransducerFired()
	{
		if (!Started) {return;}
		if (mode == InterimScareMode.MatthewChasingHallway)
		{
			mode = InterimScareMode.PlayerInTransducerRoom;
			Tween tween = GetTree().CreateTween();
			
			tween.SetParallel(true);
			tween.TweenProperty(stompHard, "volume_linear", 0.0f, 5.0f);
			tween.TweenProperty(chasingTalk, "volume_linear", 0.0f, 5.0f);
			tween.TweenProperty(plr, "shakeMultiplier", 0.0f, 2.0f);
			tween.TweenProperty(env.Environment, "fog_depth_end", 6, 5.0f);
			await std.Wait(8.0f);
			movementTween.Stop();
			soundCarrier.GlobalPosition = transOutsidePos.GlobalPosition;
			stomp.Play();
			plr.FadeMusic("SomewhereInBetween", .5f);
			plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
			await std.Wait(2.5f);
			plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
			stomp.Play();
			await std.Wait(3.0f);
			if ((plr.rigidbody.GlobalPosition - doorOpen.GlobalPosition).Length() > 10)
			{
				doorOpen.Play();
			}
			await std.Wait(3.0f);
			soundCarrier.GlobalPosition = transInsidePos.GlobalPosition;
			stomp.Play();
			projectorBreathing.Play();
			await ToSignal(projectorBreathing, AudioStreamPlayer3D.SignalName.Finished);
			mode = InterimScareMode.MatthewGone;

		}
	}
	public async Task InterimToMatthewFired()
	{
		if (!Started) {return;}
		if (mode == InterimScareMode.MatthewAtStart)
		{
			mode = InterimScareMode.MatthewChasingHallway;
			stompHard.Play();
			movementTween = GetTree().CreateTween();
			movementTween.SetParallel(true);
			movementTween.TweenProperty(soundCarrier, "global_position", hallwayPos.GlobalPosition, 10.0f);
			movementTween.TweenProperty(env.Environment, "fog_depth_end", 3.0f, 10.0f);
			chasingTalk.Play();
			plr.shakeMultiplier = 0.13f;
			await ToSignal(movementTween, Tween.SignalName.Finished);
			if (mode == InterimScareMode.MatthewChasingHallway)
			{
				float originalSpeed = plr.movementSpeed;
				plr.movementSpeed = 1.0f;
				mode = InterimScareMode.PlayerBraveInHallway;
				plr.FadeMusic("SomewhereInBetween", 0.1f);
				Tween tween = GetTree().CreateTween();
				projectorBreathingGlobal.VolumeLinear = 0;
				projectorBreathingGlobal.Play();
				tween.SetParallel(true);
				plr.shakeMultiplier = 0.0f;
				tween.TweenProperty(projectorBreathingGlobal, "volume_linear", 1.0f, 1.0f);
				chasingTalk.Stop();
				stompHard.Stop();
				await ToSignal(projectorBreathingGlobal, AudioStreamPlayer.SignalName.Finished);
				tween = GetTree().CreateTween(); 
				tween.TweenProperty(env.Environment, "fog_depth_end", 6.0f, 3.0f);
				mode = InterimScareMode.MatthewGone;
				plr.movementSpeed = originalSpeed;
			}	
		}
	}
	private void DoTimedChecks()
	{
	
	}
	public void MatthewEscapedFired()
	{
		if (!Started) {return;}
		if (mode == InterimScareMode.MatthewGone)
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(env.Environment, "fog_depth_end", 20, 10.0f);
			ambience.Play();
		}
	}
	public async Task InterimToProjectorFired()
	{
		if (!Started) {return;}
		if (mode == InterimScareMode.MatthewChasingHallway)
		{
			tableHider.canUnhide = false;
			mode = InterimScareMode.PlayerInProjectorRoom;
			Tween tween = GetTree().CreateTween();
			
			tween.SetParallel(true);
			tween.TweenProperty(stompHard, "volume_linear", 0.0f, 5.0f);
			tween.TweenProperty(chasingTalk, "volume_linear", 0.0f, 5.0f);
			tween.TweenProperty(plr, "shakeMultiplier", 0.0f, 2.0f);
			tween.TweenProperty(env.Environment, "fog_depth_end", 6, 5.0f);
			await std.Wait(8.0f);
			movementTween.Stop();
			soundCarrier.GlobalPosition = projectorOutsidePos.GlobalPosition;
			stomp.Play();
			plr.FadeMusic("SomewhereInBetween", .5f);
			plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
			await std.Wait(2.5f);
			plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
			stomp.Play();
			Tween projectorTween = GetTree().CreateTween();
			((Lightblinker)projectorLight).Enabled = false;
			projectorTween.SetParallel(true);
			projectorTween.TweenProperty(projectorIdle, "volume_linear", 0.0f, 0.5f);
			projectorTween.TweenProperty(projectorLight, "light_energy", 0.0f, 0.5f);
			projectorSwitcher.enabled = false;
			
			
			await std.Wait(3.0f);
			soundCarrier.GlobalPosition = projectorInsidePos.GlobalPosition;
			projectorBreathing.Play();
			await ToSignal(projectorBreathing, AudioStreamPlayer3D.SignalName.Finished);
			tableHider.canUnhide = true;
			tween = GetTree().CreateTween();
			tween.TweenProperty(env.Environment, "fog_depth_end", 6, 5.0f);
			mode = InterimScareMode.MatthewGone;
			

		}
	}
	private void HandleSounds()
	{
		Node sound = std.GetRandomChildFromParent(SoundsParent);
		if (sound is not AudioStreamPlayer3D) {return;}
		AudioStreamPlayer3D soundStream = (AudioStreamPlayer3D) sound;
		soundStream.Play();
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!Started) {return;}
		float now = Time.GetTicksMsec();
		DoTimedChecks();
		if (now > timeOfNextScare && mode == InterimScareMode.MatthewAtStart)
		{
			timeOfNextScare = now + ((GD.RandRange(-hallucinator.Variance, hallucinator.Variance) + hallucinator.TimeBetweenScares)/2 * 1000);
			HandleSounds();
			
		}
	
	}
}
