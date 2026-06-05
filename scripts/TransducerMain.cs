using Godot;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GArray = Godot.Collections.Array<Godot.Node3D>;
using GArrayNode = Godot.Collections.Array<Godot.Node>;
using GArrayMesh = Godot.Collections.Array<Godot.MeshInstance3D>;

public partial class TransducerMain : Node3D
{
	[ExportGroup("Objects")]
	[Export] public PlayerController plr;
	[Export] public InterimScarer scarer;
	[Export] public AudioStreamPlayer ambience;
	[Export] public Transductor connector;
	[Export] public Area3D objectArea;
	[Export] public Area3D playerArea;
	[Export] public StaticBody3D barrier;
	[Export] public Node3D gotoPosition;
	[Export] public Node3D spinnerTop;
	[Export] public Node3D spinnerBottom;
	[Export] public Node3D spinner;
	[Export] public OmniLight3D light;
	
	[ExportSubgroup("Decals")]
	[Export] public Decal offline;
	[Export] public Decal activating;
	[Export] public Decal online;
	[Export] public Decal tooManyObjects;
	[Export] public Decal noObjects;
	[Export] public Decal failure;
	[ExportSubgroup("Audio")]
	[Export] public AudioStreamPlayer3D Alarm;
	[Export] public AudioStreamPlayer3D Idle;
	[Export] public AudioStreamPlayer3D bad;
	[Export] public AudioStreamPlayer3D bad2;
	[Export] public AudioStreamPlayer3D doorSound; 
	[Export] public AudioStreamPlayer3D InteractGood;
	[Export] public AudioStreamPlayer3D InteractBad;
	[ExportGroup("Shaders")]
	[Export] public ShaderMaterial brightifier;
	[Export] public ShaderMaterial glower;
	[Export] public ShaderMaterial disintegrator;
	[ExportGroup("Magic")]
	[Export] public float EndLightValue = 1.836f;
	[Export] public float forceMultiplier;
	[Export] public Color PostDestruction;
	
	private RigidBody3D mover;
	public bool busy = false;
	private bool spinnerDirection = false;
	private float spinnerMultiplier = 0;
	private async Task FailCase(string reason, Tween IdleTween)
	{
	
		IdleTween.Stop();
		IdleTween = GetTree().CreateTween();
		IdleTween.SetParallel(true);
		IdleTween.TweenProperty(Idle, "volume_linear", 0.0f, 3.0f);
		IdleTween.TweenProperty(light, "light_energy", 0.0f, 3.0f);
		IdleTween.TweenMethod(Callable.From<float>(SetGlowShaderValue), glower.GetShaderParameter("alphaVal"), 0.0, 3.0f).SetTrans(Tween.TransitionType.Linear);
		if (reason == "tmo")
		{
			InteractBad.Play();
			activating.Visible = false;
			tooManyObjects.Visible = true;
			await Task.Delay(3000);
			offline.Visible = true;
			tooManyObjects.Visible = false;
			busy = false;
		}
		if (reason == "no")
		{
			InteractBad.Play();
			activating.Visible = false;
			noObjects.Visible = true;
			await Task.Delay(3000);
			offline.Visible = true;
			noObjects.Visible = false;
			busy = false;
		}
		
	}

	private void SetBrightnessShaderValue(float value)
	{
		brightifier.SetShaderParameter("alphaVal", value);
	}
	private void SetDisintegratorShaderValue(float value)
	{
		disintegrator.SetShaderParameter("fadeB", value);
	}
	private void SetGlowShaderValue(float value)
	{
		glower.SetShaderParameter("alphaVal", value);
	}
	private void SetGlowShaderColor(Vector3 value)
	{
		glower.SetShaderParameter("color", value);
	}
	private async Task PlayerTeleport()
	{
		activating.Visible = false;
		online.Visible = true;
		GD.Print("Player transducing");
		barrier.SetCollisionLayerValue(1, true);
		plr.CanUseFlashlight = false;
		if (plr.isFlashlightEquipped)
		{
			plr.UnequipFlashlight();
		}
		Tween tweenStart = GetTree().CreateTween();
		tweenStart.SetParallel(true);
		tweenStart.TweenProperty(this, "spinnerMultiplier", 1.0f, 20.0f);
		tweenStart.TweenProperty(Idle, "pitch_scale", 12, 25.0f);
		plr.CanMove = false;
		await Task.Delay(2000);
		bad.Play();
		plr.shaker.PremadeShake(Shaker.ImpulseShakes.Explosion);
		await Task.Delay(3000);
		Tween tweenAwayLife = GetTree().CreateTween();
		tweenAwayLife.SetParallel(true);
		tweenAwayLife.TweenProperty(Idle, "volume_linear", 5, 10.0f);
		tweenAwayLife.TweenProperty(light, "light_energy", 10, 10.0f);
		tweenAwayLife.TweenProperty(light, "light_volumetric_fog_energy", 1000, 10.0f);
		await Task.Delay(5000);
		doorSound.Play();
		await ToSignal(doorSound, AudioStreamPlayer3D.SignalName.Finished);
		bad2.Play();
		await ToSignal(tweenAwayLife, Tween.SignalName.Finished);
		Idle.Stop();
		tweenStart.Stop();
		plr.CanRotate = false;
		plr.GetChapterTitle().SetBlackoutOpacity(1.0f, 0.0f);
		ambience.Stop();
		spinnerMultiplier = 0.2f;
		SetGlowShaderValue(0.2f);
		light.LightEnergy = 0.2f;
		light.LightColor = PostDestruction;
		glower.SetShaderParameter("color", new Vector3(PostDestruction.R, PostDestruction.G, PostDestruction.B));
		await Task.Delay(3500);
		plr.GetChapterTitle().SetBlackoutOpacity(0.0f, 0.0f);
		Alarm.Play();
		Idle.PitchScale = 0.5f;
		Idle.Play();
		Idle.VolumeLinear = 0.8f;
		light.LightVolumetricFogEnergy = 10.0f;
		plr.CanRotate = true;
		Tween antiTween = GetTree().CreateTween();
		antiTween.SetParallel(true);
		antiTween.TweenProperty(this, "spinnerMultiplier", 0.0f, 5.0f);
		antiTween.TweenProperty(Idle, "pitch_scale", 0.1, 5.0f);
		antiTween.TweenProperty(light, "light_energy", 0.0f, 5.0f);
		antiTween.TweenMethod(Callable.From<float>(SetGlowShaderValue), glower.GetShaderParameter("alphaVal"), 0.0, 5.0f).SetTrans(Tween.TransitionType.Linear);
		await ToSignal(antiTween, Tween.SignalName.Finished);
		Idle.Stop();
		barrier.SetCollisionLayerValue(1, false);
		plr.CanMove = true;
		plr.CanUseFlashlight = true;
		online.Visible = false;
		failure.Visible = true;
		Task.Delay(8000);
		Tween failureTween = GetTree().CreateTween();
		Tween failureTweenShader = GetTree().CreateTween();
		failureTween.SetParallel(true);
		failureTween.TweenProperty(failure, "modulate", new Color(1.0f,1.0f,1.0f,0.0f), 10.0f);
		failureTween.TweenProperty(Alarm, "volume_linear", 0, 10.0f).SetTrans(Tween.TransitionType.Sine);
		failureTweenShader.TweenMethod(Callable.From<Godot.Vector3>(SetGlowShaderColor), glower.GetShaderParameter("color"), new Vector3(0.0f,0.0f,0.0f), 10.0f).SetTrans(Tween.TransitionType.Linear);
		scarer.Start();
		//ambience.Play();
		await ToSignal(failureTween, Tween.SignalName.Finished);
		Alarm.Stop();
		
		//busy = false;
	}
	private void SpinnerLoop()
	{
		if (spinner.Position.Y > spinnerTop.Position.Y)
		{
			spinnerDirection = false;
		}
		if (spinner.Position.Y < spinnerBottom.Position.Y)
		{
			spinnerDirection = true;
		}
		if (spinnerDirection == false)
		{
			spinner.Position -= new Vector3(0,0.3f,0) * spinnerMultiplier;
		}
		else
		{
			spinner.Position += new Vector3(0,0.3f,0) * spinnerMultiplier;
		}
		spinner.RotateY(1.0f * spinnerMultiplier);
	}
	private void ObjectMover()
	{
		Vector3 directionToHoldPoint = mover.GlobalPosition.DirectionTo(gotoPosition.GlobalPosition);
		float distance = mover.GlobalPosition.DistanceTo(gotoPosition.GlobalPosition);
		float normalizedMultiplier = distance/2;
		//float inversed = Mathf.Abs(normalizedMultiplier - 1.0f);
		mover.ApplyForce(directionToHoldPoint * 100 * forceMultiplier * normalizedMultiplier);
		mover.ApplyForce(-mover.LinearVelocity * 0.99f);
	}
	private async Task ObjectTeleport(Node3D bodyNode)
	{
		activating.Visible = false;
		online.Visible = true;
		GD.Print("Object transducing");
		RigidBody3D body = (RigidBody3D)bodyNode;
		plr.shaker.PremadeShake(Shaker.ImpulseShakes.Transducer);
		body.Freeze = false;
		body.SetCollisionLayerValue(3, false);
		body.SetCollisionMaskValue(3, false);
		body.SetCollisionLayerValue(1, false);
		body.SetCollisionMaskValue(1, false);
		mover = body;
		barrier.SetCollisionLayerValue(1, true);
		GArrayNode children = body.FindChildren("*", "MeshInstance3D", true, false);
		GArrayMesh meshes = new GArrayMesh();
		GD.Print(children.Count());
		foreach (Node child in children)
		{
			if (child is MeshInstance3D)
			{
				meshes.Add((MeshInstance3D)child);
			}
		}
		foreach (MeshInstance3D mesh in meshes)
		{
			mesh.MaterialOverlay = brightifier;
		}
		Tween tweenShader = GetTree().CreateTween();
		Tween tweenAudio = GetTree().CreateTween();
		tweenAudio.SetParallel(true);
		tweenAudio.TweenProperty(this, "spinnerMultiplier", 1.0f, 20.0f);
		tweenAudio.TweenProperty(Idle, "pitch_scale", 12, 25.0f);
		tweenShader.TweenMethod(Callable.From<float>(SetBrightnessShaderValue), 0.0, 1.0, 5.0f);
		await ToSignal(tweenShader, Tween.SignalName.Finished);
		tweenShader.Stop();
		tweenShader = GetTree().CreateTween();
		foreach (MeshInstance3D mesh in meshes)
		{
			mesh.MaterialOverlay = null;
			mesh.MaterialOverride = disintegrator;
			
		}
		SetDisintegratorShaderValue(50.0f);
		tweenShader.TweenMethod(Callable.From<float>(SetDisintegratorShaderValue), 30.0, 0.0, 20.0f).SetTrans(Tween.TransitionType.Linear);
		await ToSignal(tweenShader, Tween.SignalName.Finished);
		mover = null;
		body.QueueFree();
		Tween antiTween = GetTree().CreateTween();
		antiTween.SetParallel(true);
		antiTween.TweenProperty(this, "spinnerMultiplier", 0.0f, 10.0f);
		antiTween.TweenProperty(Idle, "pitch_scale", 1, 10.0f);
		antiTween.TweenProperty(Idle, "volume_linear", 0, 10.0f);
		antiTween.TweenProperty(light, "light_energy", 0.0f, 10.0f);
		antiTween.TweenMethod(Callable.From<float>(SetGlowShaderValue), 10.0, 0.0, 10.0f).SetTrans(Tween.TransitionType.Linear);
		await ToSignal(antiTween, Tween.SignalName.Finished);
		barrier.SetCollisionLayerValue(1, false);
		online.Visible = false;
		offline.Visible = true;
		busy = false;
	}
	public async Task Activate()
	{
		// Connector calls this function when screen is activated
		busy = true;
		offline.Visible = false;
		activating.Visible = true;		
		InteractGood.Play();
		
		Idle.VolumeLinear = 0;
		Idle.Play();
		Tween IdleTween = GetTree().CreateTween();
		IdleTween.SetParallel(true);
		IdleTween.TweenProperty(Idle, "volume_linear", 1, 10.0f);
		IdleTween.TweenProperty(light, "light_energy", EndLightValue, 25.0f);
		IdleTween.TweenMethod(Callable.From<float>(SetGlowShaderValue), 0.0, 10.0, 10.0f).SetTrans(Tween.TransitionType.Linear);

		await Task.Delay(3000);
		plr.CanHold = true;
		GArray objectBodies = objectArea.GetOverlappingBodies();
		GArray playerBodies = playerArea.GetOverlappingBodies();
		if (objectBodies.Count() > 1){FailCase("tmo", IdleTween);}
		if (objectBodies.Count() > 0 && playerBodies.Count() > 0) {FailCase("tmo", IdleTween);}
		if (objectBodies.Count() == 0 && playerBodies.Count() == 0) {FailCase("no", IdleTween);}
		// go to activate hello
		if (playerBodies.Count() == 1 && objectBodies.Count() == 0)
		{
			PlayerTeleport();
		}
		else if (objectBodies.Count() == 1)
		{
			GD.Print(objectBodies.Count());
			ObjectTeleport(objectBodies[0]);
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (mover != null)
		{	
			ObjectMover();
		}
		SpinnerLoop();
	}
}
