using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class ProjectorInfo : Node
{
	[ExportGroup("Video Properties")]
	[Export] public float Framerate;


	public async Task ModifyShader(ShaderMaterial text, string uniformName) {
		Godot.Collections.Array<Node> rawFrameList = GetChildren();
		Dictionary<int, ProjectorFrame> frameList = new Dictionary<int, ProjectorFrame>();
		foreach (Node node in rawFrameList) {
			if (node is ProjectorFrame) {
				ProjectorFrame frame = (ProjectorFrame) node;
				bool isDigitsOnly = !string.IsNullOrEmpty(frame.Name.ToString()) && frame.Name.ToString().All(char.IsDigit);
				if (!isDigitsOnly) {
					GD.Print("Unacceptable frame with name '"+ frame.Name.ToString() +"'!");
					continue;
				}
				int digitizedName = frame.Name.ToString().ToInt();
				if (frameList.ContainsKey(digitizedName) == true) {
					GD.PrintErr("Multiple frame entries for frame +"+ digitizedName +"+!");
					continue;
				} 
				frameList.Add(digitizedName, frame);
			}
		}
		foreach (var framePair in frameList.OrderBy(frame => frame.Key)) {
			ProjectorFrame frame = framePair.Value;
			text.SetShaderParameter(uniformName, frame.Frame);
			float time = frame.GetSecondsUntilNextFrame(Framerate);
			await Task.Delay((int)(time * 1000));
		}
	}
	public async Task ModifySprite(Sprite3D text, bool clearTextureAfterFinished) {
		Godot.Collections.Array<Node> rawFrameList = GetChildren();
		Dictionary<int, ProjectorFrame> frameList = new Dictionary<int, ProjectorFrame>();
		foreach (Node node in rawFrameList) {
			if (node is ProjectorFrame) {
				ProjectorFrame frame = (ProjectorFrame) node;
				bool isDigitsOnly = !string.IsNullOrEmpty(frame.Name.ToString()) && frame.Name.ToString().All(char.IsDigit);
				if (!isDigitsOnly) {
					GD.Print("Unacceptable frame with name '"+ frame.Name.ToString() +"'!");
					continue;
				}
				int digitizedName = frame.Name.ToString().ToInt();
				if (frameList.ContainsKey(digitizedName) == true) {
					GD.PrintErr("Multiple frame entries for frame +"+ digitizedName +"+!");
					continue;
				} 
				frameList.Add(digitizedName, frame);
			}
		}
		foreach (var framePair in frameList.OrderBy(frame => frame.Key)) {
			ProjectorFrame frame = framePair.Value;
			text.Texture = frame.Frame;
			float time = frame.GetSecondsUntilNextFrame(Framerate);
			await Task.Delay((int)(time * 1000));
		}
		if (clearTextureAfterFinished) {
			text.Texture = null;
		}
	}
}
