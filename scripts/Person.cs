using Godot;
using System;

public partial class Person : Node3D
{
	[ExportGroup("Fixed")]
	[Export] public PlayerController player;
	[Export] public Sprite3D sprite;
	[Export] public RigidBody3D rigidbody;
	[ExportGroup("Sounds")]
	[Export] public AudioStreamPlayer3D OnApproach;
	[Export] public Node HitSoundsFolder;
	[Export] public Node DialogueSoundsFolder;
	[ExportGroup("Distances")]
	[Export] public float OnApproachDistance;
	[ExportGroup("Amounts")]
	[Export] public int hitSoundCooldown;
	[Export] public float hitSoundMinimumVelocity;
	[Export] public float lowerRandomTimeForDialogue;
	[Export] public float higherRandomTimeForDialogue;
	private bool OnApproachPlayed = false;
	private Standard std;
	private float lastHitTime = Time.GetTicksMsec();
	private float nextTalkTime = Time.GetTicksMsec();

	private void SetRandomTalkTime(AudioStreamPlayer3D lastPlayer)
	{
		double timeSecondsTill = GD.RandRange((double)lowerRandomTimeForDialogue, (double)higherRandomTimeForDialogue);
		timeSecondsTill += lastPlayer.Stream.GetLength();
		nextTalkTime = Time.GetTicksMsec() + ((float)timeSecondsTill * 1000);
	}	
	public override void _PhysicsProcess (double delta)
	{
		
		if (DialogueSoundsFolder != null)
		{
			if (OnApproachPlayed && DialogueSoundsFolder.GetChildCount() > 0)
			{
				float nowTime = Time.GetTicksMsec();
				if (nowTime > nextTalkTime)
				{
					Node child = std.GetRandomChildFromParent(DialogueSoundsFolder);
					if (child is AudioStreamPlayer3D)
					{
						AudioStreamPlayer3D childPlayer = (AudioStreamPlayer3D) child;
						childPlayer.Play();
						SetRandomTalkTime(childPlayer);
					}
				}
			}
		}

		Vector3 playerPosition = player.rigidbody.GlobalPosition;
		Vector3 currentPosition = sprite.GlobalPosition;

		Vector3 relativeVector = new Vector3(
			playerPosition.X - currentPosition.X,
			playerPosition.Y - currentPosition.Y,
			playerPosition.Z - currentPosition.Z
		);
		float distance = relativeVector.Length();

		if (OnApproach != null)
		{
			if (distance <= OnApproachDistance && OnApproachPlayed == false)
			{
				
				OnApproach.Play();
				SetRandomTalkTime(OnApproach);
				OnApproachPlayed = true;
			} 
		}
		
	}
	public override void _Ready()
	{
		std = new Standard();
		GD.Print("added");
		rigidbody.BodyEntered += OnBodyEntered;
	}
	public void OnBodyEntered(Node body)
	{
		
		RigidBody3D bodyRigid = (RigidBody3D) body;

		if (bodyRigid != null)
		{
			float now = Time.GetTicksMsec();
			if (now - lastHitTime >= hitSoundCooldown * 1000)
			{
				lastHitTime = now;
				if (bodyRigid.LinearVelocity.Length() >= hitSoundMinimumVelocity) {
					if (bodyRigid.GetCollisionLayerValue(4))
					{
						Node hitSoundNode = std.GetRandomChildFromParent(HitSoundsFolder);
						if (hitSoundNode != null && hitSoundNode is AudioStreamPlayer3D)
						{
							AudioStreamPlayer3D hitSoundFinal = (AudioStreamPlayer3D)hitSoundNode;
							hitSoundFinal.Play();
						}
					}
				}
			}
		}
	}
}
