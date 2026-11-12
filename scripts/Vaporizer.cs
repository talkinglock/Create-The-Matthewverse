using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public partial class Vaporizer : RigidBody3D
{
	[Export] public VisibleOnScreenNotifier3D notifier;
	[Export] public float distanceToStart;
	[Export] public float timeUntilVaporization;
	[Export] public float timeOfVaporization;
	[Export] public PlayerController plr;
	[Export] public ShaderMaterial disintegrator;
	private bool activated = false;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void SetDisintegratorShaderValue(float value)
	{
		disintegrator.SetShaderParameter("fadeB", value);
	}
	public async Task Destroy()
	{
		await Task.Delay((int)(timeUntilVaporization * 1000));
		foreach (Node node in GetChildren())
		{
			if (node is MeshInstance3D)
			{
				MeshInstance3D mesh = (MeshInstance3D)node;
				mesh.MaterialOverlay = null;
				mesh.MaterialOverride = disintegrator;
			}
			
		}
		SetDisintegratorShaderValue(10.0f);
		Tween tweenShader = GetTree().CreateTween();
		tweenShader.TweenMethod(Callable.From<float>(SetDisintegratorShaderValue), 30.0, 0.0, timeOfVaporization).SetTrans(Tween.TransitionType.Linear);
		await ToSignal(tweenShader, Tween.SignalName.Finished);
		GetParent().QueueFree();
	}
	public override void _PhysicsProcess(double delta)
	{
		if (notifier.IsOnScreen() && activated == false)
		{
			float distance = (plr.rigidbody.GlobalPosition - GlobalPosition).Length();
			if (distance < distanceToStart)
			{
				activated = true;
				Destroy();
			}
		}
	}
}
