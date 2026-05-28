using Godot;
using System;

public partial class EnStart1 : Node3D
{
	[Export] public Triggers triggerManager;
	[Export] public PlayerController plr;
	[Export] public hrCutscene hrCut;
	private ChapterTitle title;
	public override void _Ready()
	{
		title = plr.GetChapterTitle();
		triggerManager.Connect(
			"TriggerActivated",
			Callable.From<string>(OnTriggerActivated)
		);
	}

	private async void OnTriggerActivated(string trigger)
	{
		switch(trigger)
		{
			case "StartTrigger":
			{
				plr.PlayMusic("Welcome");
				await title.ShowTitle("Chapter 1 - Chipfood Co", 3);
				break;
			}
			case "FadeMusic":
			{
				plr.FadeMusic("Welcome", 2.0f);
				break;
			}
			case "StartCutscene":
				{
					hrCut.Start();
					break;
				}
			case "InRoomCutscene":
				{
					hrCut.InRoom();
					break;
				}
			default:
				break;
		}
	}
}
