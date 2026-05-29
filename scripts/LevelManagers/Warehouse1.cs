using Godot;
using System;
using System.Threading.Tasks;

public partial class Warehouse1 : Node3D
{
	[Export] public Triggers triggerManager;
	[Export] public PlayerController plr;
	[Export] public SingleDoorConnector entranceDoor;
	[Export] public AudioStreamPlayer3D pickupSound;
	[Export] public AudioStreamPlayer sting;
	[Export] public Node3D flashlight;
	[Export] public float StingStopTime;


	private ChapterTitle title;
	private bool WaitingOnFlashlightEPress = false;
	private bool WaitingOnFlashlightFPress = false;

	private async Task HandleStartAsync()
	{
		plr.CanMove = false;
		title = plr.GetChapterTitle();
		title.SetBlackoutOpacity(1.0f, -1);
		await Task.Delay(6000);
		Tween stingTween = GetTree().CreateTween();
		stingTween.TweenProperty(sting, "volume_linear", 0.0f, StingStopTime);
		title.SetBlackoutOpacity(0.0f, StingStopTime);
		await Task.Delay((int)(StingStopTime/2 * 1000.0));
		plr.CanMove = true;
		WaitingOnFlashlightEPress = true;
		title.Interact("E - Pick up Flashlight", true);
	}

	public override async void _Ready()
	{
		HandleStartAsync();
		triggerManager.Connect(
			"TriggerActivated",
			Callable.From<string>(OnTriggerActivated)
		);
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsKeyPressed(Key.E) && WaitingOnFlashlightEPress)
		{
			WaitingOnFlashlightEPress = false;
			pickupSound.Play();
			flashlight.QueueFree();
			plr.CanUseFlashlight = true;
			title.Interact("F - Toggle Flashlight", true);
			WaitingOnFlashlightFPress = true;
		}
		if (Input.IsKeyLabelPressed(Key.F) && WaitingOnFlashlightFPress)
		{
			WaitingOnFlashlightFPress = false;
			title.StopInteract(true);
			entranceDoor.SetCanOpen(true);
		}
	}

	private async void OnTriggerActivated(string trigger)
	{
		switch(trigger)
		{
			case "StartTrigger":
			{
				plr.PlayMusic("Unemployment");
				await title.ShowTitle("Chapter 2 - Unemployed", 3);
				break;
			}
			
			default:
				break;
		}
	}
}
