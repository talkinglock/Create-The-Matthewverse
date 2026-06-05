using Godot;
using System;
using System.Threading.Tasks;

public partial class Shaker : Node
{
	[Export] public PlayerController player;

	private Standard std;


	public enum ImpulseShakes
	{
		Explosion,
		Transducer
	}

	public override void _Ready()
	{
		std = new Standard();
	}
	public async Task doShake(float windupTime, float timeOfShake, float multiplierEnd, float maxRandEnd, float windDownTime=-1,Tween.TransitionType trans = Tween.TransitionType.Linear)
	{
		if (windDownTime == -1)
		{
			windDownTime = windupTime;
		}
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(player, "shakeMultiplier", multiplierEnd, windupTime).SetTrans(trans);
		tween.TweenProperty(player, "maxRandShake", maxRandEnd, windupTime).SetTrans(trans);

		await ToSignal(tween, Tween.SignalName.Finished);
		await std.Wait(timeOfShake);
		
		tween = GetTree().CreateTween();
		tween.TweenProperty(player, "shakeMultiplier", 0, windDownTime).SetTrans(trans);
		tween.TweenProperty(player, "maxRandShake", 0, windDownTime).SetTrans(trans);
		await ToSignal(tween, Tween.SignalName.Finished);
	} 

	public async Task PremadeShake(ImpulseShakes shakeEnum)
	{
		switch(shakeEnum)
		{
			case(ImpulseShakes.Explosion):
				{
					await doShake(0.35f, 0.2f, 0.27f, 0.455f, -1, Tween.TransitionType.Linear);
					break;
				}
			case(ImpulseShakes.Transducer):
				{
					await doShake(25.0f, 0.0f, 0.27f, 0.255f, 10.0f, Tween.TransitionType.Cubic);
					break;
				}
			default:
				{
					break;
				}
		}
	}
	
	
}
