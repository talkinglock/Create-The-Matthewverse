using Godot;
using System;
using System.Threading.Tasks;

public partial class hrCutscene : Node
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Sprite3D hrGuy;
	[Export] public Sprite3D guardA;
	[Export] public Sprite3D guardB;
	[Export] public StaticBody3D firstBlock;
	[Export] public StaticBody3D secondBlock;
	[Export] public SingleDoorConnector door;
	[Export] public Node3D guardA1Pos;
	[Export] public Node3D guardA2Pos;
	[Export] public Node3D guardB1Pos;
	[Export] public Node3D guardB2Pos;
	[Export] public Node3D hrGuy1Pos;
	[Export] public Node3D hrGuy2Pos;
	[Export] public Node3D hallwayMiddle;
	[Export] public AudioStreamPlayer3D greeting;
	[Export] public AudioStreamPlayer3D noSales;
	[Export] public AudioStreamPlayer3D restrain;
	[Export] public AudioStreamPlayer3D goodbye;
	[Export] public PlayerController plr;
	[Export] public AudioStreamPlayer3D ambience;
	[Export] public AudioStreamPlayer3D uncomfort;
	[Export] public AudioStreamPlayer3D uncomfort2;
	[Export] public AudioStreamPlayer3D punch;
	[Export] public AudioStreamPlayer3D sting;
	private ChapterTitle chapterTitle;
	private float minRegisterTime = 0;
	private bool uncomfortPlaying = false;
	private bool uncomfort2Playing = false;

	private bool IsHrSpeech = false;
	private bool IsRestrain = false;
	private bool secondTriggerHit = false;
	private bool IsGoodbye = false;
	public override void _Ready()
	{
		chapterTitle = plr.GetChapterTitle();
	}
    public override void _PhysicsProcess(double delta)
    {
		if (uncomfortPlaying)
		{
			if (uncomfort.Playing == false)
			{
				uncomfort.Play();
			}
		}
		if (uncomfort2Playing)
		{
			if (uncomfort2.Playing == false)
			{
				uncomfort2.Play();
			}
		}
        if (secondTriggerHit == true && Time.GetTicksMsec() > minRegisterTime)
		{
			AfterRoomEntrance();
		}
		if (IsHrSpeech == true && Time.GetTicksMsec() > minRegisterTime)
		{
			HrSpeech();
		}
		if (IsRestrain == true && Time.GetTicksMsec() > minRegisterTime)
		{
			Restrain();
		}
		if (IsGoodbye == true && Time.GetTicksMsec() > minRegisterTime)
		{
			Goodbye();
		}
    }
	private async Task Goodbye()
	{
		IsGoodbye = false;
		uncomfortPlaying = false;
		uncomfort2Playing = false;
		uncomfort2.Playing = false;
		uncomfort.Playing = false;
		punch.GlobalPosition = plr.rigidbody.GlobalPosition;
		sting.GlobalPosition = plr.rigidbody.GlobalPosition;
		punch.Play();
		sting.Play();
		await chapterTitle.SetBlackoutOpacity(1.0f, 0.01f);

	}
	private void MoveTo(Node3D node, Vector3 place, float time)
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(node, "position", place, time).SetTrans(Tween.TransitionType.Linear);
	}
	public void InRoom()
	{
		secondTriggerHit = true;
	}
	private void Restrain()
	{
		IsRestrain = false;
		uncomfort2Playing = true;
		uncomfort2.Play();
		Tween uncomfortTween = GetTree().CreateTween();
		uncomfortTween.TweenProperty(uncomfort2, "volume_db", 20.0, 5);
		Vector3 plrPos = plr.rigidbody.GlobalPosition;
		float distance = (plrPos - guardA.GlobalPosition).Length();
		Vector3 direction = (plrPos - guardA.GlobalPosition).Normalized();
		distance = distance - 1.0f;
		MoveTo(guardA, guardA.GlobalPosition + (direction * distance), 0.1f);
		plr.CanMove = false;
		goodbye.Play();
		minRegisterTime = ((float)goodbye.Stream.GetLength()) * 1000 + Time.GetTicksMsec();
		IsGoodbye = true;
	}
	private void HrSpeech()
	{
		uncomfortPlaying = true;
		uncomfort.Play();
		Tween uncomfortTween = GetTree().CreateTween();
		uncomfortTween.TweenProperty(uncomfort, "volume_db", 20.0, 5);
		IsHrSpeech = false;
		restrain.Play();
		minRegisterTime = (float)restrain.Stream.GetLength() * 1000 + Time.GetTicksMsec();
		IsRestrain = true;
	}
	private void AfterRoomEntrance()
	{
		secondTriggerHit = false;
		secondBlock.SetCollisionLayerValue(1, true);
		secondBlock.SetCollisionMaskValue(1, true);
		MoveTo(hrGuy, hrGuy2Pos.GlobalPosition, 0.5f);
		MoveTo(guardA, guardA2Pos.GlobalPosition, 0.5f);
		MoveTo(guardB, guardB2Pos.GlobalPosition, 0.5f);
		Tween ambienceTween = GetTree().CreateTween();
		ambienceTween.TweenProperty(ambience, "volume_linear", 0, 3f);
		noSales.Play();
		minRegisterTime = Time.GetTicksMsec() + (float)noSales.Stream.GetLength() * 1000;
		IsHrSpeech = true;
	}
	public void Start()
	{
		firstBlock.SetCollisionLayerValue(1, true);
		firstBlock.SetCollisionMaskValue(1, true);
		door.OpenDoor(hrGuy, 0.5f);
		MoveTo(hrGuy, hrGuy1Pos.GlobalPosition, 0.5f);
		MoveTo(guardA, guardA1Pos.GlobalPosition, 0.5f);
		MoveTo(guardB, guardB1Pos.GlobalPosition, 0.5f);
		greeting.Play();
		minRegisterTime = Time.GetTicksMsec() + (float)greeting.Stream.GetLength() * 1000.0f;
	}
}
