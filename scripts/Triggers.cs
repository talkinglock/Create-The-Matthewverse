using Godot;
using System;
using GNodeArray = Godot.Collections.Array<Godot.Node>;

public partial class Triggers : Node
{
	[Export] public PlayerController plr;

	[Signal] public delegate void TriggerActivatedEventHandler(string trigger);
	public override void _Ready()
	{
		GNodeArray childrenNodes = GetChildren();
		foreach (Area3D trigger in childrenNodes)
		{
			void entered(Node node)
			{
				GD.Print("collision");
				if (node.Name == "PlrRigid")
				{
					EmitSignal("TriggerActivated", trigger.Name);
					if (trigger.GetNodeOrNull<Node>("Oneshot") != null)
					{
						trigger.QueueFree();
					}
				}
			}
			trigger.Monitoring = true;
			trigger.BodyEntered += entered;
		}
	}
}
