using Godot;
using System;
using System.Threading.Tasks;

public partial class ProjectorAnimationHandler : Node3D 
{
	[ExportGroup("Objects")]
    [Export] public PlayerController player;
	[Export] public AudioStreamPlayer3D LowMainsSound;
	[Export] public AudioStreamPlayer3D HighMainsSound;
	[Export] public AudioStreamPlayer3D StartClicks;
	[Export] public AudioStreamPlayer3D StartClicksSecondary;
	[Export] public AudioStreamPlayer3D ExperimentStartClicks;
	[Export] public AudioStreamPlayer3D ExperimentReadyBeep;
	[Export] public AudioStreamPlayer3D DigitalProjectorSound;
    [Export] public Button StartButton;
    [Export] public OmniLight3D StartButtonLight;
    [Export] public MeshInstance3D StartButtonLightObj;
    [Export] public Button ExperimentButton;
    [Export] public OmniLight3D ExperimentButtonLight;
    [Export] public MeshInstance3D ExperimentButtonLightObj;
	[Export] public Lightbulb OverheadLight;
	[Export] public Lightbulb FilmLight;
	[Export] public Lightbulb DigitalLight;
    [Export] public Tracker EMFTracker;
    [Export] public Sprite3D DigitalProjectorScreen;
    [Export] public SpotLight3D DigitalProjectorLight;
    [Export] public SpotLight3D DigitalProjectorLightReflection;
    [Export] public Alarm OverloadAlarm;
    [Export] public Alarm ResidualAlarm;
    [Export] public AudioStreamPlayer3D ResidualAlarmSound;
    [Export] public AudioStreamPlayer3D OverloadAlarmSound;
    [Export] public AudioStreamPlayer3D DigitalProjectorPopSound;
    [Export] public AudioStreamPlayer3D FilmPopSound;
    [Export] public AudioStreamPlayer3D DigitalPopSound;
    [Export] public Node3D DestroyedLightbulbTemplate;
    [Export] public Node3D SparkTemplate;
    [Export] public AudioStreamPlayer3D SparkAudioTemplate;
    [Export] public RigidBody3D DestroyedProjector;
    [Export] public Node3D DigitalProjector;
    [ExportGroup("Materials")]
    [Export] public BaseMaterial3D BlueLightMaterial;
    [Export] public BaseMaterial3D OffLightMaterial;
    [Export] public BaseMaterial3D RedLightMaterial;
    [Export] public ShaderMaterial DigitalProjectorMaterial;
    [ExportGroup("Vacuum Pump")] 
    [Export] public Node3D VacuumPump;
    [Export] public AudioStreamPlayer3D VacuumPumpEngine;
    [Export] public AudioStreamPlayer3D VacuumPumpEngineStart;
    [ExportGroup("Gauges")]
    [Export] public AnalogGauge VacuumPressureGauge;
    [Export] public AnalogGauge VacuumVoltsGauge;
    [Export] public AnalogGauge VacuumCurrentGauge;
    [Export] public AnalogGauge TotalVoltsGauge;
    [Export] public AnalogGauge TotalCurrentGauge;
    [Export] public AnalogGauge EMFResidual;
    [ExportGroup("Particles")]
    [Export] public GpuParticles3D DigitalProjectorParticles;
    [Export] public GpuParticles3D DigitalProjectorLightParticles;
    [Export] public GpuParticles3D FilmProjectorLightParticles;
	[ExportGroup("Misc")]
    [Export] public Node3D AntennaLeftLow;
    [Export] public Node3D AntennaLeftHigh;
    [Export] public Node3D AntennaRightHigh;
    [Export] public Node3D AntennaRightLow;
    [Export] public ProjectorInfo DigitalProjectorInfo;
    [Export] public Vector3 VacuumPumpMovementModifier;
	[Export] public float StartTimeAlpha;
	[Export] public float StartTimeBravo;
    [Export] public float EndTweenTime;
    [Export] public float SparkTweenTime;
    [Export] public float TimeBetweenSparkSpawns;
    [Export] public float TimeBetweenSparkSpawnsVariation;
	[Export] public FilmProjectorAnimationHandler FilmProjector;

    private Standard std;

    private bool sparksEnabled = false;
    private float NextTimeOfSpark = 0.0f;
    private Vector3 vacuumPumpBasePosition;
    private float vacuumPumpShakeMultiplier = 0.0f;
    private float EmfMultiplier = 0.1f;
    private float EmfResidualMultiplier = 0.0f;

    private enum OscillationMode {
        None,
        LocalLights,
        AllLights,
        Desk,
        Extreme
    }

    private OscillationMode mode = OscillationMode.None;
    private void SetAllGauges(float value, float time) {
        VacuumPressureGauge.SetToAlpha(value, time);
        VacuumVoltsGauge.SetToAlpha(value, time);
        VacuumCurrentGauge.SetToAlpha(value, time);
        TotalVoltsGauge.SetToAlpha(value, time);
        TotalCurrentGauge.SetToAlpha(value, time);
        EMFResidual.SetToAlpha(value, time);
    }

    private float EmfBase() {
        return (((Mathf.Sin((Time.GetTicksMsec() / 10.0f) + 103.0f) + Mathf.Cos((Time.GetTicksMsec() / 100.0f))) / 10.0f + 2 - 1f) * EmfMultiplier);
    }
    private void ResidualEmfShaker() {
        float baseVal = EmfBase();
        EMFResidual.SetToAlpha(baseVal * EmfResidualMultiplier, 0.1f);
    }
    public void ShakeVacuumPump() {
        Vector3 rand = (new Vector3(GD.Randf(), GD.Randf(), GD.Randf()) - new Vector3(0.5f, 0.5f, 0.5f)) * VacuumPumpMovementModifier * vacuumPumpShakeMultiplier;
        VacuumPump.Position = vacuumPumpBasePosition + rand;
    }
    private Tween GetTween() {
        return GetTree().CreateTween().SetParallel(true);
    }

    public override void _Ready() {
        std = new Standard();
        vacuumPumpBasePosition = VacuumPump.Position;
        StartButton.onPressed = Start;
        ExperimentButton.onPressed = Experiment;
        SetAllGauges(0.0f, 0.1f);
    }

    private void ChangeVacuumPumpMultiplier(float alpha) {
        vacuumPumpShakeMultiplier = alpha;
    }

    private void ChangeEmfMultiplier(float alpha) {
        EmfMultiplier = alpha;
    }

    public async Task StartAsync() {
        // Called when the initial start button is pressed. Spring into action
        TotalVoltsGauge.SetToAlpha(0.67f, 0.3f);
        TotalCurrentGauge.SetToAlpha(0.5f,0.3f);
        VacuumPressureGauge.SetToAlpha(0.5f, 0.3f);
        StartClicks.Play();
        await Task.Delay(300);
        StartButtonLight.LightColor = RedLightMaterial.AlbedoColor;
        StartButtonLightObj.MaterialOverride = RedLightMaterial;
        await Task.Delay(700);
        Tween tween = GetTween();
        tween.TweenMethod(Callable.From<float>(OverheadLight.SetLightLevel), 0.0f, 0.2f, StartTimeAlpha);
        tween.Play();
        LowMainsSound.Play();
        TotalCurrentGauge.SetToAlpha(0.4f, 0.9f);
        await Task.Delay(1000);
        EMFTracker.SetFormula(EmfBase);
        EMFTracker.Speed = 20.0f;
        EmfResidualMultiplier = 0.3f;
        TotalCurrentGauge.SetToAlpha(0.35f, 0.2f);
        VacuumVoltsGauge.SetToAlpha(0.67f, 0.2f);
        VacuumCurrentGauge.SetToAlpha(0.5f, 0.2f);
        VacuumCurrentGauge.SetToAlpha(0.35f, 2.0f);
        VacuumPressureGauge.SetToAlpha(0.1f, 2.0f);
        VacuumPumpEngineStart.Play();
        tween = GetTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenMethod(Callable.From<float>(ChangeVacuumPumpMultiplier), 0.0f, 1.0f, VacuumPumpEngineStart.Stream.GetLength()); 
        tween.Play();
        await ToSignal(VacuumPumpEngineStart, AudioStreamPlayer3D.SignalName.Finished);
        VacuumPumpEngine.Play();
        await Task.Delay(2000);
        StartClicksSecondary.Play();
        tween = GetTween();
        tween.Play();
        await ToSignal(StartClicksSecondary, AudioStreamPlayer3D.SignalName.Finished);
        TotalCurrentGauge.SetToAlpha(0.30f, 2.0f);
        HighMainsSound.Play();
        tween = GetTween();
        tween.TweenProperty(HighMainsSound, "volume_db", 0.0, StartTimeBravo);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenMethod(Callable.From<float>(OverheadLight.SetLightLevel), 0.2f, 1.0f, StartTimeBravo);
        tween.TweenMethod(Callable.From<float>(ChangeEmfMultiplier), 0.1f, 0.3f, StartTimeBravo); 
        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
        ExperimentReadyBeep.Play();
        ExperimentButtonLight.LightColor = BlueLightMaterial.AlbedoColor;
        ExperimentButtonLightObj.MaterialOverride = BlueLightMaterial;
        ExperimentButton.enabled = true;
    }

    private void TweenFilmLight(float alpha) {
        FilmLight.SetLightLevel(alpha);
    }

    public void ModifyDigitalBrightnessShader(float alpha) {
        DigitalProjectorMaterial.SetShaderParameter("brightness_amplifier", alpha);
    }

    public void ModifyDigitalBurnCutoff(float alpha) {
        DigitalProjectorMaterial.SetShaderParameter("burn_cutoff", alpha);
    }
    
    public void ModifyDigitalSliceCutoff(float alpha) {
        DigitalProjectorMaterial.SetShaderParameter("slice_cutoff", alpha);
    }

    public float OscillateAround(float input, float timeDivisor, float amplitudeMultiplier, float salt = 0) {
        float time = Time.GetTicksMsec() / timeDivisor;
        float sin = (Mathf.Sin(input + time + salt) * amplitudeMultiplier) - 0.5f;
        return input + sin;
    }

    public void Oscillate() {
        if (mode == OscillationMode.None) {return;}
        if (mode >= OscillationMode.LocalLights) {
            float filmLightOscillation = OscillateAround(2.0f, 40.0f, 0.3f);
            float digitalLightOscillation = OscillateAround(5.0f, 30.0f, 0.6f, 342.0f);
            FilmLight.SetLightLevel(filmLightOscillation);
            DigitalLight.SetLightLevel(digitalLightOscillation);
        }
    }

    public Vector3 LerpVec3(float alpha, Vector3 start, Vector3 end) {
        return new Vector3(Mathf.Lerp(start.X, end.X, alpha), Mathf.Lerp(start.Y, end.Y, alpha), Mathf.Lerp(start.Z, end.Z, alpha));
    }
    public async Task SpawnSpark() {
        float leftPosNormalized = GD.Randf();
        float rightPosNormalized = GD.Randf();
        float distanceToTop;
        if (leftPosNormalized >= rightPosNormalized) {
            distanceToTop = 1.0f - leftPosNormalized;
        } else {
            distanceToTop = 1.0f - rightPosNormalized;
        }
        Vector3 leftPos = LerpVec3(leftPosNormalized, AntennaLeftLow.GlobalPosition, AntennaLeftHigh.GlobalPosition);
        Vector3 rightPos = LerpVec3(rightPosNormalized, AntennaRightLow.GlobalPosition, AntennaRightHigh.GlobalPosition);
        Vector3 leftPosTop = LerpVec3(leftPosNormalized + distanceToTop, AntennaLeftLow.GlobalPosition, AntennaLeftHigh.GlobalPosition);
        Vector3 rightPosTop = LerpVec3(rightPosNormalized + distanceToTop, AntennaRightLow.GlobalPosition, AntennaRightHigh.GlobalPosition);
        Vector3 sparkPosStart = LerpVec3(0.5f, leftPos, rightPos);
        Vector3 sparkPosEnd = LerpVec3(0.5f, leftPosTop, rightPosTop);
        float scaleIncrease = (leftPos - rightPos).Length();
        Vector3 targetRightVector = (rightPos - leftPos).Normalized();
        Vector3 up = Vector3.Up;
        Vector3 forward = targetRightVector.Cross(up).Normalized();

        Basis targetBasis = new Basis(targetRightVector, up, forward);

        Node3D spark = (Node3D) SparkTemplate.Duplicate(7);
        AudioStreamPlayer3D sparkAudio = (AudioStreamPlayer3D) SparkAudioTemplate.Duplicate(7);
        AddChild(spark);
        spark.AddChild(sparkAudio);
        spark.GlobalPosition = sparkPosStart;
        spark.GlobalBasis = targetBasis;
        spark.Scale = new Vector3(scaleIncrease * 31.0f, 1.0f, 1.0f);
        sparkAudio.Play();
        Tween tween = GetTween();
        EmfResidualMultiplier = 10.0f;
        ResidualAlarmSound.Play();
        ResidualAlarm.Enabled = true;
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(spark, "global_position", sparkPosEnd, SparkTweenTime * distanceToTop);
        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
        ResidualAlarmSound.Stop();
        ResidualAlarm.Enabled = false;
        EmfResidualMultiplier = 0.5f;
        spark.QueueFree();
    }

    public void HandleSparks() {
        if (!sparksEnabled) {return;}
        if (Time.GetTicksMsec() >= NextTimeOfSpark) {
            NextTimeOfSpark = std.GetRandomTimeInFuture(TimeBetweenSparkSpawns, TimeBetweenSparkSpawnsVariation);
            SpawnSpark();
        }
    }

    public async Task DigitalProjectorDisaster() {
        Tween tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(DigitalLight.SetLightLevel), 0.0f, 1.0f, 1.0f);
        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
        DigitalProjectorScreen.SetLayerMaskValue(1, true);
        DigitalProjectorSound.Play();
        DigitalProjectorLight.LightEnergy = 1.0f;
        Task finished = DigitalProjectorInfo.ModifyShader(DigitalProjectorMaterial, "sprite_texture");
        await ToSignal(GetTree().CreateTimer(3.0f), Timer.SignalName.Timeout);
        tween = GetTween();

        tween.TweenProperty(DigitalProjectorLightReflection, "light_energy", 2.0f, 30.0f);
        tween.TweenProperty(DigitalProjectorLight, "light_energy", 10.0f, 30.0f);
        tween.TweenProperty(DigitalProjectorSound, "pitch_scale", 20.0f, 30.0f);
        tween.TweenProperty(DigitalProjectorSound, "volume_linear", 100.0f, 30.0f);
        tween.TweenMethod(Callable.From<float>(ModifyDigitalBrightnessShader), 1.0f, 10.0f, 30.0f);
        tween.TweenMethod(Callable.From<float>(ModifyDigitalBurnCutoff), 2.0f, 0.0f, 30.0f);
        tween.TweenMethod(Callable.From<float>(ModifyDigitalSliceCutoff), 1.0f, 0.0f, 30.0f);
        tween.TweenMethod(Callable.From<float>(player.SetShake), 0.0f, 0.2f, 30.0f);
        tween.Play();
        Tween initialOverloadTween = GetTween();
        initialOverloadTween.TweenMethod(Callable.From<float>(ChangeEmfMultiplier), 0.3f, 1.0f, 5.0f);
        initialOverloadTween.Play();
        Tween otherTween = GetTween();
        otherTween.SetTrans(Tween.TransitionType.Linear);
        otherTween.TweenMethod(Callable.From<float>(FilmLight.SetLightLevelWithPitch), 0.0f, 20.0f, 8.0f);
        otherTween.TweenMethod(Callable.From<float>(DigitalLight.SetLightLevelWithPitch), 1.0f, 30.0f, 8.0f);
        otherTween.Play();
        await ToSignal(initialOverloadTween, Tween.SignalName.Finished);
        OverloadAlarm.Enabled = true;
        OverloadAlarmSound.Play();
        await ToSignal(otherTween, Tween.SignalName.Finished);
        Node3D NewFilmLight = (Node3D) DestroyedLightbulbTemplate.Duplicate(7);
        Node3D NewDigitalLight = (Node3D) DestroyedLightbulbTemplate.Duplicate(7);

        AddChild(NewFilmLight);
        AddChild(NewDigitalLight);
        NewFilmLight.GlobalPosition = FilmLight.GlobalPosition;
        NewDigitalLight.GlobalPosition = DigitalLight.GlobalPosition;
        NewFilmLight.GlobalRotation = FilmLight.GlobalRotation;
        NewDigitalLight.GlobalRotation = DigitalLight.GlobalRotation;
        FilmLight.QueueFree();
        DigitalLight.QueueFree();

        DigitalProjectorLightParticles.Emitting = true;
        FilmProjectorLightParticles.Emitting = true;
        FilmPopSound.Play();
        DigitalPopSound.Play();
        await ToSignal(GetTree().CreateTimer(5.0f), Timer.SignalName.Timeout);
        sparksEnabled = true;
        await ToSignal(tween, Tween.SignalName.Finished);
        Tween endTween = GetTween();
        endTween.TweenMethod(Callable.From<float>(player.SetShake), 0.23f, 0.0f, EndTweenTime);
        endTween.TweenProperty(DigitalProjectorLightReflection, "light_energy", 0.0f, EndTweenTime);
        endTween.TweenProperty(DigitalProjectorLight, "light_energy", 0.0f, EndTweenTime);
        endTween.Play();
        OverheadLight.SetLightLevel(0.0f);
        EmfMultiplier = 0.0f;
        EmfResidualMultiplier = 0.0f;
        SetAllGauges(0.0f, 0.3f);
        vacuumPumpShakeMultiplier = 0.0f;
        VacuumPumpEngine.Stop();
        StartButtonLight.QueueFree();
        ExperimentButtonLight.QueueFree();
        StartButtonLightObj.MaterialOverride = OffLightMaterial;
        ExperimentButtonLightObj.MaterialOverride = OffLightMaterial;
        EMFTracker.Speed = 0.0f;
        EMFTracker.Increment = 0.0f;
        EMFTracker.IncrementSpeed = 0.0f;
        sparksEnabled = false;
        OverloadAlarmSound.Stop();
        ResidualAlarmSound.Stop();
        OverloadAlarm.Enabled = false;
        ResidualAlarm.Enabled = false;
        DigitalProjectorSound.Stop();
        HighMainsSound.Stop();
        LowMainsSound.Stop();
        DigitalProjectorPopSound.Play();
        Vector3 oldPos = DigitalProjector.GlobalPosition;
        DigitalProjector.QueueFree();

        DestroyedProjector.GlobalPosition = oldPos;
        DestroyedProjector.Freeze = false;
        DestroyedProjector.ApplyImpulse(new Vector3(0.0f, 10.0f, 0.0f));
        await ToSignal(endTween, Tween.SignalName.Finished);
        DigitalProjectorLightParticles.Emitting = true;
        DigitalProjectorLight.QueueFree();
        DigitalProjectorLightReflection.QueueFree();
        DigitalProjectorScreen.QueueFree();
    }
    public async Task ExperimentAsync() {
        ExperimentButtonLight.LightColor = RedLightMaterial.AlbedoColor;
        ExperimentButtonLightObj.MaterialOverride = RedLightMaterial;
        ExperimentStartClicks.Play();
        await Task.Delay(1000);
        Tween tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(TweenFilmLight), 0.0f, 0.5f, 1.0f);
        tween.Play();
        await Task.Delay(2000);
        Task delay = FilmProjector.Begin();
        await delay;
        tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(TweenFilmLight), 0.5f, 0.0f, 1.0f);
        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
        await Task.Delay(2000);
        await DigitalProjectorDisaster();
    }

    public override void _PhysicsProcess(double delta) {
        ShakeVacuumPump();
        ResidualEmfShaker();
        Oscillate();
        HandleSparks();
    }

	public void Experiment() {
        ExperimentAsync();
	}
	public void Start() {
        StartAsync();
	}
}
