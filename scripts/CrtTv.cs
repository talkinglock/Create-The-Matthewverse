using Godot;
using System;

public interface ICrtChannel {
	Color GetLightColor();
	void OnChannelHit(float TimeUntilCut);
	void OnChannelLost();
}

public partial class CrtTv : Node3D
{
	/*
	 * Loop:
	 * No signal
	 * Static
	 * Random Channel
	 * Static
	*/

	private enum Status {
		NoSignal = 0,
		StaticStart = 1,
		Channel = 2,
		StaticEnd = 3
	}
  
	[ExportGroup("Object")]
	[Export] public Node NoSignalChannelExport;
	[Export] public Node StaticChannelExport;
	[Export] public Node ParentOfRandomChannels;
	[Export] public SpotLight3D ExteriorLight;
	[ExportGroup("Values")]
	[Export] public float TimeOfNoSignal;
	[Export] public float TimeOfNoSignalVariance;
	[Export] public float TimeOfStatic;
	[Export] public float TimeOfRandomChannel;
	[Export] public float TimeOfRandomChannelVariance;

	private Standard std;

	private Status currentStatus = Status.NoSignal; 
	private ICrtChannel currentChannel;

	private float TimeUntilNextChannelSwitch = 0.0f;
	private float timer = 0.0f;

	private ICrtChannel NoSignalChannel;
	private ICrtChannel StaticChannel;

	private void CutChannel() {
		currentChannel.OnChannelLost();
		currentChannel = null;
	}

	private void ActivateChannel(ICrtChannel channel) {
		if (currentChannel != null) {
			CutChannel();
		}
		currentChannel = channel;
		ExteriorLight.LightColor = channel.GetLightColor();
		channel.OnChannelHit(TimeUntilNextChannelSwitch);
	}
	
	public override void _Ready() {
		std = new Standard(); 
		currentStatus = (Status)(GD.Randi() % 4);
		NoSignalChannel = (ICrtChannel) NoSignalChannelExport;
		StaticChannel = (ICrtChannel) StaticChannelExport;
	}

	public override void _PhysicsProcess(double delta) {
		if (timer > TimeUntilNextChannelSwitch) {
			timer = 0.0f;
			if (currentStatus == Status.StaticEnd) {
				currentStatus = Status.NoSignal;
			} else {
				currentStatus = (Status) ((int)currentStatus + 1);
			}
			float initial, variance;
			switch (currentStatus) {
				case Status.NoSignal:
					initial = TimeOfNoSignal;
					variance = TimeOfNoSignalVariance;
					break;
				case Status.StaticStart or Status.StaticEnd:
					initial = TimeOfStatic;
					variance = 0.0f;
					break;
				case Status.Channel: 
					initial = TimeOfRandomChannel;
					variance = TimeOfRandomChannelVariance;
					break;
				default:
					initial = 0.0f;
					variance = 0.0f;
					break;
			}
			TimeUntilNextChannelSwitch = std.AddVariance(initial, variance);
			switch (currentStatus) {
				case Status.NoSignal:
					ActivateChannel(NoSignalChannel);
					break;
				case Status.StaticStart or Status.StaticEnd:
					ActivateChannel(StaticChannel);
					break;
				case Status.Channel:
					Node channel = std.GetRandomChildFromParent(ParentOfRandomChannels);
					ActivateChannel((ICrtChannel) channel);
					break;
				default:
					break;
			}
		}
		timer += (float) delta;
	}
}
