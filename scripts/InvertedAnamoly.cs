using Godot;
using System;

public enum InvertedState {
	None,
	WaitingForApproach,
	WaitingForLeave
}
public partial class InvertedAnamoly : Node
{
	// Triggers when the player approaches it and they have visited camp 17 at least once, then disappears when they gain enough distance
	[Export] public StackTemplate normalTemplate;
	[Export] public StackTemplate invertedTemplate;
	[Export] public PlayerController plr;
	[Export] public float CannotSeeDistance;
	private InvertedState state = InvertedState.None;
	public void PlayerApproachedCamp() {
		invertedTemplate.isDisabled = false;
		normalTemplate.isDisabled = true;
	}
	public override void _Ready()
	{
		invertedTemplate.isDisabled = true;
	}
	public override void _PhysicsProcess(double delta)
	{
		switch (state) {
			case InvertedState.WaitingForApproach: {
				float distance = plr.GetDistanceTo(normalTemplate.GlobalPosition);
				if (distance < CannotSeeDistance) {
					state = InvertedState.WaitingForLeave;
				}
				break;
			}
			case InvertedState.WaitingForLeave: {
				float distance = plr.GetDistanceTo(normalTemplate.GlobalPosition);
				if (distance > CannotSeeDistance) {
					invertedTemplate.isDisabled = true;
					normalTemplate.isDisabled = false;
					state = InvertedState.None;
				}
				break;
			}
			default: {
				break;
			}
		}
	}
}
