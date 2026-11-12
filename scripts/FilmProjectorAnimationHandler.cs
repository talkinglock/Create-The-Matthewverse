using Godot;
using System;
using System.Threading.Tasks;

public partial class FilmProjectorAnimationHandler : Node
{
	// Handles the playing of the Film Projectors animation in the faraday cage scene
	[ExportGroup("Objects")]
	[Export] public FilmProjectorSpinner FilmSpinner;
	[Export] public ProjectorInfo ProjectorInfo;
	[Export] public Sprite3D ProjectionSprite;
	[Export] public ShaderMaterial ProjectionSurface;
	[Export] public Light3D Ambience;
	[Export] public SpotLight3D FiringAmbience;
	[Export] public AudioStreamPlayer3D SoundAmbience;
	[Export] public AudioStreamPlayer3D ProjectorSound;
	[ExportGroup("Options")]
	[Export] public float FinalSpindleSpeed;
	[Export] public float StartupTime;
	[Export] public Vector3 DecalStutterAmplifier;
	[Export] public float DescendingSpeed;

	private Vector3 baseDecalPosition;
	private bool isDancing = false;

	private float DescenderContrib = 0.0f;
	public override void _Ready() {
		baseDecalPosition = ProjectionSprite.Position;
	}
	public override void _PhysicsProcess(double delta) {
		DecalDance();
	}

	private void DescenderDance(float severity) {
		if (DescenderContrib > severity) {
			DescenderContrib = -severity;
		}
		DescenderContrib += DescendingSpeed;
	}

	public void DecalDance() {
		// vfx of the projected surface 

		float xRand = GD.Randf() - 0.5f;
		float yRand = GD.Randf() - 0.5f;
		Vector3 delta = new Vector3(xRand, yRand - DescenderContrib, 0.0f);
		ProjectionSprite.Position = baseDecalPosition + delta * DecalStutterAmplifier;
	}

	public async Task Begin() {
		// Called by the animation controller

		Tween soundAmbienceStart = GetTree().CreateTween();
		Tween ambienceStart = GetTree().CreateTween();
		Tween firingAmbienceStart = GetTree().CreateTween();
		Tween callbackTween = GetTree().CreateTween();
		firingAmbienceStart.TweenProperty(FiringAmbience, "light_energy", 2.8f, StartupTime);
		ambienceStart.TweenProperty(Ambience, "light_energy", 3.0f, StartupTime);
		soundAmbienceStart.SetParallel(true);
		soundAmbienceStart.TweenProperty(SoundAmbience, "volume_linear", 1.0f, StartupTime);
		soundAmbienceStart.TweenProperty(SoundAmbience, "pitch_scale", 1.25f, StartupTime);
		callbackTween.SetParallel(true);
		callbackTween.TweenMethod(
				Callable.From<float>(DescenderDance),
				15.0f,
				0.0f,	
				StartupTime
		);
		callbackTween.TweenMethod(
				Callable.From<float>(FilmSpinner.SetSpinSpeed),
				0.0f,
				FinalSpindleSpeed,
				StartupTime
		);
		callbackTween.Play();
		firingAmbienceStart.Play();
		ambienceStart.Play();
		SoundAmbience.Play();
		soundAmbienceStart.Play();
		
        ProjectorSound.Play();
		ProjectionSprite.SetLayerMaskValue(1, true);
		await this.ProjectorInfo.ModifyShader(ProjectionSurface, "sprite_texture");
		Tween soundAmbienceEnd = GetTree().CreateTween();
		Tween ambienceEnd = GetTree().CreateTween();
		Tween firingAmbienceEnd = GetTree().CreateTween();
		callbackTween = GetTree().CreateTween();
		firingAmbienceEnd.TweenProperty(FiringAmbience, "light_energy", 0.0f, StartupTime);
		ambienceEnd.TweenProperty(Ambience, "light_energy", 0.0f, StartupTime);
		soundAmbienceEnd.SetParallel(true);
		soundAmbienceEnd.TweenProperty(SoundAmbience, "volume_linear", 0.0f, StartupTime);
		soundAmbienceEnd.TweenProperty(SoundAmbience, "pitch_scale", 0.0f, StartupTime);
		callbackTween.SetParallel(true);
		callbackTween.TweenMethod(
				Callable.From<float>(DescenderDance),
				0.0f,
				15.0f,	
				StartupTime	
		);
		callbackTween.TweenMethod(
				Callable.From<float>(FilmSpinner.SetSpinSpeed),
				FinalSpindleSpeed,
				0.0f,
				StartupTime
		);
		callbackTween.Play();
		firingAmbienceEnd.Play();
		ambienceEnd.Play();
		soundAmbienceEnd.Play();
		await ToSignal(callbackTween, Tween.SignalName.Finished);
		ProjectionSprite.SetLayerMaskValue(1, false);
	}
}
