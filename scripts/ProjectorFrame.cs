using Godot;
using System;

public partial class ProjectorFrame : Node
{
	public enum FrameTimeType {
		Frames,
		Seconds
	}
	[ExportGroup("Properties")]
	[Export] public float TimeUntilNextFrame;
	[Export] public FrameTimeType TimeType;
	[ExportGroup("Frame")]
	[Export] public Texture2D Frame;

	public float GetSecondsUntilNextFrame(float framerate) {
		if (TimeType == FrameTimeType.Frames) {
			return TimeUntilNextFrame / framerate;
		} else {
			return TimeUntilNextFrame;
		}
	}
}
