using Godot;
using Godot.Collections;
using System;

public partial class ObjectChannel : Node, ICrtChannel
{
	[ExportGroup("Objects")]
	[Export] public Color LightColor;
	[Export] public Node3D Objects;
	[Export] public Node3D MainScreen;
	[Export] public Node3D CameraScreen;
	[ExportGroup("Tweakables")]
	[Export] public float StartShapification;

	private Standard std;
	private Node3D obj;
	private Array<ShaderMaterial> objects;
	public Color GetLightColor() {return LightColor;}

	public void OnChannelHit(float timeAllocated) {
		MainScreen.Visible = false;
		CameraScreen.Visible = true;
		objects = [];
		Node3D parent = (Node3D)std.GetRandomChildFromParent(Objects);
		parent.Visible = true;
		obj = parent;
		if (parent is MeshInstance3D mesh) {
			if (mesh.MaterialOverride is ShaderMaterial shader) {
				objects.Add(shader);
			}
		}
		foreach (Node child in parent.GetChildren()) {
			Node3D child3d = (Node3D) child;
			if (child3d is MeshInstance3D meshObj) {
				if (meshObj.MaterialOverride is ShaderMaterial shader) {
					objects.Add(shader);
				}
			}
		}
		void setShader(float alpha) {
			foreach (ShaderMaterial shader in objects) {
				shader.SetShaderParameter("normalsy", alpha);
			}
		}
		Tween tween = GetTree().CreateTween();
		tween.TweenMethod(Callable.From<float>(setShader), StartShapification, 0.99f, timeAllocated);
		tween.Play();
	}

	public void OnChannelLost() {
		MainScreen.Visible = true;
		CameraScreen.Visible = false;
		obj.Visible = false;
	}

	public override void _Ready() {
		std = new Standard();
	}
}
