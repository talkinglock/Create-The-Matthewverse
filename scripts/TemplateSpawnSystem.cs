using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class TemplateSpawnSystem : Node
{
	private Func<bool> Condition;
	private Func<Vector3, bool> DespawnCondition;
	private Action<Node3D> OnSpawned;
	[Export] public Node3D Scene;
	[Export] public float ItemDespawnDistance;
	[Export] public bool OnlySpawnsOnce;
	[ExportGroup("Misc")]
	[Export] public PlayerController plr;
	[Export] public float TimeBetweenChecksSec;

	private Standard std;

	private bool busy = false;
	private bool hasSpawned = false;
	private Node3D SpawnedTree;
	private Template SelectedTemplate;

	public void SetDespawnCondition(Func<Vector3, bool> cond) {
		DespawnCondition = cond;
	}

	public void SetCondition(Func<bool> cond) {
		Condition = cond;
	}


	public void SetOnSpawned(Action<Node3D> func) {
		OnSpawned = func;
	}
	private void Check() {

		if (hasSpawned && DespawnCondition != null && !OnlySpawnsOnce) {
			bool shouldDespawn = DespawnCondition(SpawnedTree.GlobalPosition);
			if (shouldDespawn) {
				foreach (StackTemplate temp in SelectedTemplate.AttachedStacks) {
					temp.isDisabled = false;
				}
				foreach (Node rigidNode3D in SpawnedTree.FindChildren("*", "RigidBody3D")) {
					RigidBody3D rigid = (RigidBody3D) rigidNode3D;
					float distance = (rigid.GlobalPosition - SpawnedTree.GlobalPosition).Length();
					if (distance > ItemDespawnDistance) {
						Node3D parent = rigid.GetParentOrNull<Node3D>();
						if (parent != null) {
							if (parent is Impact) {
								parent.CallDeferred(Node.MethodName.Reparent, this);
								continue;
							}
						}
						rigid.CallDeferred(Node.MethodName.Reparent, this);
					}
				}
				SpawnedTree.QueueFree();
				hasSpawned = false;
			}
		}

		if (Condition == null) {busy = false; return;}

		if (hasSpawned) {busy = false; return;}
		bool spawnAccepted = Condition();
		if (!spawnAccepted) {busy = false; return;}
		
		Template template = (Template) std.GetRandomChildFromParent(this);
		SpawnedTree = (Node3D) Scene.Duplicate(7);
		AddChild(SpawnedTree);
		SpawnedTree.GlobalPosition = template.GlobalPosition;
		SpawnedTree.GlobalRotation = template.GlobalRotation;

		SelectedTemplate = template;
		Array<StackTemplate> stacks = template.AttachedStacks;
		foreach (StackTemplate stack in stacks) {
			stack.isDisabled = true;
		}

		hasSpawned = true;
		OnSpawned?.Invoke(SpawnedTree);
		busy = false;
	}


	public override void _Ready() {
		std = new Standard(); 
	}
	private float timer = 0.0f;
	public override void _PhysicsProcess(double delta) {
		timer += (float)delta;
		if (timer >= TimeBetweenChecksSec)
		{
			timer = 0f;
			Check();
		}
	}
}
