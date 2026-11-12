using Godot;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
public partial class Hallucinator : Node
{
	[Export] public PlayerController player;
	[Export] public WorldEnvironment env;
	[Export] public WarehouseStackPerformanceHandler stackPerformanceHandler;
	[Export] public int TimeBetweenScares;
	[Export] public int Variance;
	[Export] public int playerDistanceLower;
	[Export] public int playerDistanceUpper;
	[Export] public Node3D soundPlayer;
	[Export] public float ColorTweenTime;
	[Export] public Node3D placesOfAudio;

	[ExportGroup("Afar")]
	[Export] public float timeInWarehouseForStalkingStart;
	[Export] public float timeBetweenScaresAfar;
	[Export] public float timeVarianceAfar;
	[Export] public Node3D audiosAfar;
	[Export] public float nearestPosAfar;
	[Export] public float farthestPosAfar;
	[Export] public float chanceForBoxFailAfar;
	[ExportGroup("Nearby")]
	[Export] public float timeBetweenScaresNearby;
	[Export] public float timeVarianceNearby;
	[Export] public Node3D audiosNearby;
	[Export] public float nearestPosNearby;
	[Export] public float farthestPosNearby;
	[Export] public float chanceForBoxFailNearby;
	[ExportGroup("Audio")]
	[Export] public AudioStreamPlayer3D impactOne;
	[Export] public float impactOnePlayDistanceSquared;
	[ExportGroup("Random spawns")]
	[Export] public Node3D trashcan;
	[Export] public float trashcanSpawnChance;
	[ExportGroup("Eyes")]
	[Export] public Node3D eyes;
	[Export] public RayCast3D eyesRaycast;
	[Export] public float eyesLowerPos;
	[Export] public float eyesUpperPos;
	[Export] public float eyesExistCooldown;
	[Export] public float eyesTimeBetweenScares;
	[Export] public float eyesTimeBetweenScaresVariation;
	[Export] public VisibleOnScreenNotifier3D eyesOnScreen;

	[Export] public float eyesTimeout;
	private Standard std;
	private float timeOfEntry;
	private float timeUntilEyes = -1;
	public bool hallucinate = false;
	private bool flashlightDone = false;
	private bool CanDoEyeCheck = false;
	private float timeOfNextScare = -1;
	private float timeOfEyeCooldown;
	public ScareState state;
	public enum ScareState
	{
		LeavingAlone,
		StalkingFromAfar,
		StalkingNearby,
		StalkingShadows,
		StalkingEyesAndSound,
		AttackWaveStart,
		AttackWave
	}
	private bool impactOnePlayed = false;
	private bool hasTrashcanPlaced = false;
	private bool eyesActive = false;
	private bool eyePosFound = false;
	private bool eyesDisappearDebounce = false;
	public override void _Ready()
	{
		state = ScareState.LeavingAlone;
		GD.Randomize();
		std = new Standard();
		HandleFlashlight();
	}

	private void SetEyesPosition()
	{
		bool found = false;
		CanDoEyeCheck = true;
		
		eyesActive = true;
	}
	private void EndEyesSequence()
	{
		eyes.Visible = false;
		eyesDisappearDebounce = false;
		eyesActive = false;
		timeUntilEyes = std.GetRandomTimeInFuture(eyesTimeBetweenScares, eyesTimeBetweenScaresVariation);
	}
	private async Task KeepEyesActive()
	{
		if (eyesDisappearDebounce == true) {return;}
		//GD.Print("Ok");
		eyes.LookAt(player.camera.GlobalPosition);
		eyes.Visible = true;
		if (eyesOnScreen.IsOnScreen())
		{
			eyesDisappearDebounce = true;
			await std.Wait(eyesExistCooldown);
			GD.Print("Found");
			EndEyesSequence();
		}
		if (Time.GetTicksMsec() > timeOfEyeCooldown)
		{
			EndEyesSequence();
		}

	}
	public async Task ConnectorFabricated(WarehouseStackConnector connector)
	{
		float now = Time.GetTicksMsec();
		switch(state)
		{
			case(ScareState.StalkingNearby):
				{
					if (hasTrashcanPlaced == false && ((GD.Randi() % trashcanSpawnChance) + 1) == 1)
					{
						GD.Print("Spawning trashcan");
						
						Vector3 pos = stackPerformanceHandler.GetNearestBiasedSpawnPosition(player.rigidbody.GlobalPosition, env.Environment.FogDepthEnd);
						if (pos == Vector3.Zero)
						{
							GD.Print("Trashcan zeroed");
							return;
						}
						GD.Print("Spawned!");
						GD.Print("Distance: " + (player.rigidbody.GlobalPosition - pos).Length());
						hasTrashcanPlaced = true;
						WarehouseStackConnector.Direction direction = connector.GetDirectionToPosition(pos);
						trashcan.GlobalPosition = pos;
						if (direction == WarehouseStackConnector.Direction.ToTheLeft)
						{
							trashcan.RotateY(3.14f);
						}
					}
					break;
				}
			
				default:
				{
					break;
				}
		}
	}
	public Vector3 GetRandomPosition(Vector3 referencePos, float closest, float farthest)
	{
		float randXSign = GD.RandRange(1,2) == 1 ? -1 : 1;
		float randYSign = GD.RandRange(1,2) == 1 ? -1 : 1;
		float randX = (float)GD.RandRange(closest, farthest) * randXSign;
		float randZ = (float)GD.RandRange(closest, farthest) * randYSign;
 		Vector3 relativePosition = new Vector3(randX, 0, randZ);
		return (referencePos + relativePosition);
	}
	public Vector3 GetClosestPos(Vector3 reference, Node3D parent)
	{
		Node3D closestNode = null;
		float closestNodeDistance = -1;
		foreach (Node3D node in parent.GetChildren())
		{
			float distance = (reference - node.GlobalPosition).Length();
			if (distance < closestNodeDistance || closestNodeDistance == -1.0f)
			{
				closestNode = node;
				closestNodeDistance = distance;
			}
		}
		if (closestNode == null)
		{
			return Vector3.Inf;
		}
		return closestNode.GlobalPosition;
	}
	
	private void HandleHallucinations()
	{
		float now = Time.GetTicksMsec();
		switch(state)
		{
			case(ScareState.LeavingAlone):
				{
					if ((now - timeOfEntry) > timeInWarehouseForStalkingStart)
					{
						state = ScareState.StalkingNearby;
					}
					break;
				}
			case(ScareState.StalkingFromAfar):
				{
					if (impactOnePlayed == false)
					{
						float distanceSquared = (player.rigidbody.GlobalPosition - impactOne.GlobalPosition).LengthSquared();
						if (distanceSquared < impactOnePlayDistanceSquared)
						{
							impactOnePlayed = true;
							impactOne.Play();
						}
					}
					
					if (timeOfNextScare == -1)
					{
						timeOfNextScare = std.GetRandomTimeInFuture(timeBetweenScaresAfar, timeVarianceAfar);
					}
					if (now >= timeOfNextScare)
					{
						timeOfNextScare = std.GetRandomTimeInFuture(timeBetweenScaresAfar, timeVarianceAfar);
						Vector3 scarePosition = GetRandomPosition(GetClosestPos(player.rigidbody.GlobalPosition, placesOfAudio), nearestPosAfar, farthestPosAfar);
						AudioStreamPlayer3D sound = (AudioStreamPlayer3D)std.GetRandomChildFromParent(audiosAfar);
						soundPlayer.GlobalPosition = scarePosition;
						sound.Play();
					}
					break;
				}
			case(ScareState.StalkingNearby):
				{	
					if (timeOfNextScare == -1)
					{
						timeOfNextScare = std.GetRandomTimeInFuture(timeBetweenScaresNearby, timeVarianceNearby);
					}
					if (now >= timeOfNextScare)
					{
						timeOfNextScare = std.GetRandomTimeInFuture(timeBetweenScaresNearby, timeVarianceNearby);
						Vector3 scarePosition = GetRandomPosition(GetClosestPos(player.rigidbody.GlobalPosition, placesOfAudio), nearestPosNearby, farthestPosNearby);
						AudioStreamPlayer3D sound = (AudioStreamPlayer3D)std.GetRandomChildFromParent(audiosNearby);
						soundPlayer.GlobalPosition = scarePosition;
						sound.Play();
					}
					break;
				}
			case(ScareState.StalkingEyesAndSound):
				{
					if (eyesActive == true)
					{
						KeepEyesActive();
					}					
					if (timeUntilEyes == -1)
					{
						timeUntilEyes = std.GetRandomTimeInFuture(eyesTimeBetweenScares, eyesTimeBetweenScaresVariation); 
					}
					if (now > timeUntilEyes && eyesActive == false)
					{
						SetEyesPosition();
					}
					break;
				}
			default:
				{
					break;
				}
		}
		/*
		Node sound = std.GetRandomChildFromParent(soundPlayer);
		if (sound is not AudioStreamPlayer3D) {return;}
		AudioStreamPlayer3D soundStream = (AudioStreamPlayer3D) sound;
		Vector3 direction = Vector3.Forward.Rotated(Vector3.Up, GD.RandRange(-5,5));
		float mag = GD.RandRange(playerDistanceLower, playerDistanceUpper);
		Vector3 relativePosition = direction * mag;
		soundPlayer.GlobalPosition = player.rigidbody.GlobalPosition + relativePosition;
		soundStream.Play();
		*/
	}

	private void HandleFlashlight()
	{
		SpotLight3D flashlight = player.GetFlashLightLight();
		Tween tween = GetTree().CreateTween();
		Color color = new Color(GD.Randf(), GD.Randf(), GD.Randf(), GD.Randf());
		tween.TweenProperty(flashlight, "light_color", color, ColorTweenTime).SetTrans(Tween.TransitionType.Linear);
		flashlightDone = true;
	}



	private bool LastHallucinationValue = false;



	private void EyeCheckLoop()
	{
		if (CanDoEyeCheck == true)
		{
			eyes.Visible = false;
			Vector3 pos = GetRandomPosition(player.rigidbody.GlobalPosition, eyesLowerPos, eyesUpperPos);
			eyes.GlobalPosition = pos;
			eyesRaycast.TargetPosition = eyes.ToLocal(player.rigidbody.GlobalPosition);
			eyesRaycast.ForceRaycastUpdate();
			if (eyesRaycast.IsColliding())
			{
				if (!eyesOnScreen.IsOnScreen())
				{
					GD.Print("set");
					eyes.Visible = true;
					eyePosFound = true;
					CanDoEyeCheck = false;
					timeOfEyeCooldown = (eyesTimeout * 1000.0f) + Time.GetTicksMsec();
				}
			}
		}
		
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!hallucinate) {LastHallucinationValue = false;return;}

		if (flashlightDone)
		{
			HandleFlashlight();
			flashlightDone = false;
		}
		EyeCheckLoop();
		float now = Time.GetTicksMsec();
		if (LastHallucinationValue == false)
		{
			
			LastHallucinationValue = true;
			timeOfEntry = now;
		}
		HandleHallucinations();
	
	}
}
