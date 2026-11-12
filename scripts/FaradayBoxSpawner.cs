using Godot;
using System;

public partial class FaradayBoxSpawner : Node3D
{
	[Export] public PlayerController Player;
	[Export] public float DistanceToSpawn;
	[Export] public float DistanceToDespawn;
	[Export] public TemplateSpawnSystem SpawnSystem;

	public void OnSpawned(Node3D faradayBox) {
		GD.Print("Spawned");
		ProjectorAnimationHandler proj = (ProjectorAnimationHandler)faradayBox.FindChild("AnalogDeskProjector", recursive: true, owned: false);
		if (proj == null) {
			GD.Print("null");
		}
		proj.player = Player;
	}
	public bool DespawnCondition(Vector3 pos) {
		return (Player.rigidbody.GlobalPosition - GlobalPosition).Length() > DistanceToDespawn;
	}
	public bool Condition() {
		return (Player.rigidbody.GlobalPosition - GlobalPosition).Length() < DistanceToSpawn && !DespawnCondition(Vector3.Zero);
	}
	public override void _Ready()
	{
		SpawnSystem.SetOnSpawned(OnSpawned);
		SpawnSystem.SetCondition(Condition);
		SpawnSystem.SetDespawnCondition(DespawnCondition);
	}

}
