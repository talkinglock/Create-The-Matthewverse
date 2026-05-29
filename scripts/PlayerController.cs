using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.NetworkInformation;
//using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class PlayerController : Node3D
{
	[ExportGroup("Object References")]
	[Export] public Node music;
	[Export] public ChapterTitle title;
	[Export] public InteractableClass interactableClass;
	[Export] public RigidBody3D rigidbody;
	[Export] public Node3D cameraMount;
	[Export] public Camera3D camera;
	[Export] public RayCast3D floorCaster;
	[Export] public ShapeCast3D holdCaster;
	[Export] public AudioStreamPlayer3D footsteps;
	[ExportGroup("Physics")]
	[Export] public float movementSpeed;
	[Export] public float movementAccel;
	[Export] public float gravMultiplier;
	[Export] public float jumpVelocity;
	[Export] public float frictionMultiplier;
	[ExportSubgroup("Holding Objects")]
	[Export] public float objectAcceleration;
	[Export] public float maximumDistance;
	[Export] public float objectFrictionMultiplier;
	[Export] public float throwAcceleration;
	[Export] public Vector3 objectRotation;
	[Export] public Node3D cameraHoldObj;
	[Export] public Timer unholdCooldownTimer;
	[ExportGroup("Numbers")]
	[Export] public float jumpResponseTime;
	[Export] public float turnSens;
	[ExportGroup("Toggles")]
	[Export] public bool DoFirstObjectInteractCheck;
	[ExportGroup("Flashlight")]
	[Export] public FlashlightConnector flashlight;
	[Export] public SpotLight3D flashlightSpot;
	[Export] public float flashlightTweenTime;
	[Export] public float flashlightStartYOffset;
	[Export] public bool CanUseFlashlight = true;
	[ExportSubgroup("Viewbob Move")]
	[Export] public float maxViewBobOffsetMultiplier;
	[Export] public float viewBobTimeDenominator;
	[Export] public float viewBobNoiseDenominator;
	[ExportSubgroup("Viewbob Idle")]
	[Export] public float maxViewBobOffsetMultiplierIdle;
	[Export] public float viewBobTimeDenominatorIdle;
	[Export] public float viewBobNoiseDenominatorIdle;

	private bool isFlashlightEquipped = false;
	private bool flashlightBusy = false;
	private bool viewBobBusy = false;
	private bool lastPickupPass = false;
	private bool lastThrowPass = false;
	private bool isMoving;
	private bool FirstObjectThrowCheck ;
	private FastNoiseLite fnl;
	public bool CanMove = true;	
 	Vector2 lastCoords = Vector2.Inf;
	private float cameraAngle;

	const float PI = 3.1415926f;
	

	// pickup
	private float ePressedCount = 0;
	private bool holdingObject = false;
	private RigidBody3D objToHold = null;
	private void UpdateRotation(InputEventMouseMotion motion)
	{
		Vector2 currentMouseCoords = motion.Relative;

		if (lastCoords == Vector2.Inf)
		{
			lastCoords = currentMouseCoords;
			return;
		}

		// The X coordinate of the delta coordinates represents Y rotation of the Camera (relative to camera mount).
		// The Y coordinate of the delta coordinates represents X rotation of the Camera (relative to camera mount).
		Vector2 coordsMovement = -currentMouseCoords *  turnSens; //- lastCoords;
		
		cameraMount.Rotation = new Vector3(
			Mathf.Clamp(cameraMount.Rotation.X + coordsMovement.Y, -PI/2.0f, PI/2.0f),
			cameraMount.Rotation.Y,
			cameraMount.Rotation.Z
		);
		cameraMount.RotateY(coordsMovement.X);
		cameraAngle = cameraMount.Rotation.Y;
	}

	private float TweenWithTime(
		float startTime,
		float tweenTime,
		float startValue,
		float endValue
	)
	{
		float currentTime = Time.GetTicksMsec();
		float timeMsecs = tweenTime * 1000.0f;
		float relativeTime = currentTime - startTime;
		float normalizedTime = relativeTime/timeMsecs;

		if (normalizedTime >= 1.0f)
		{
			return endValue;
		}
		return Mathf.Lerp(startValue, endValue, normalizedTime);
	} 

	private bool IsOnFloor()
	{
		return floorCaster.IsColliding();
	}


	private float movementAccelCalc(Vector3 velocity, float targetSpeed)
	{
		float currentSpeed = velocity.Length();
		float normalizedMultiplier = currentSpeed/targetSpeed;
		return normalizedMultiplier;
	}

	private void ApplyForceToSpeed(Vector3 acceleration, float targetSpeed)
	{
		Vector3 velocity = rigidbody.LinearVelocity;
		float speed = velocity.Length();
		float multiplierRaw = speed/targetSpeed;
		if (multiplierRaw < 1.0f)
		{
			rigidbody.ApplyForce(acceleration);
		}
	}

	private void UpdateMovement(double delta)
	{
		float yVelocity = 0.0f;
		Vector3 horzMovement = Vector3.Zero;

		if (Input.IsKeyPressed(Key.W))
		{
			horzMovement.Z += 1.0f;
		}

		if (Input.IsKeyPressed(Key.S))
		{
			horzMovement.Z -= 1.0f;
		}

		if (Input.IsKeyPressed(Key.A))
		{
			horzMovement.X += 1.0f;
		}

		if (Input.IsKeyPressed(Key.D))
		{
			horzMovement.X -= 1.0f;
		}

		if (CanMove == false)
		{
			horzMovement = Vector3.Zero;
		}
		Vector3 finalMovement = (horzMovement * Transform.Basis).Normalized();
		finalMovement = finalMovement.Rotated(new Vector3(0,1,0), cameraAngle);	
		if (horzMovement != Vector3.Zero && isMoving == false)
		{
			if (footsteps.Playing == false)
			{
				footsteps.Play();
			}
			isMoving = true;
		}	
		else if (horzMovement == Vector3.Zero && isMoving == true)
		{
			footsteps.Stop();
			isMoving = false;
		}
		ApplyForceToSpeed(finalMovement * movementAccel, movementSpeed);
		if (IsOnFloor())
		{

			if (Input.IsKeyPressed(Key.Space) && CanMove)
			{
				rigidbody.ApplyForce(new Vector3(0, jumpVelocity, 0));
			}
			else
			{
				rigidbody.ApplyForce(new Vector3(0, -0.1f, 0));
			}
		}
		else
		{
			rigidbody.ApplyForce(new Vector3(0, -9.81f * gravMultiplier * rigidbody.Mass, 0));
		}
		rigidbody.ApplyForce(new Vector3(-rigidbody.LinearVelocity.X, 0, -rigidbody.LinearVelocity.Z) * frictionMultiplier);
	}

	public override void _Ready()
	{
		FirstObjectThrowCheck = DoFirstObjectInteractCheck;
		fnl = new FastNoiseLite();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			UpdateRotation(motion);
		}
	}

	
	private void objectHoldLoop()
	{
		if (FirstObjectThrowCheck && lastThrowPass == false)
		{
			lastThrowPass = true;
			title.Interact("Left Click - Throw");
		}
		Vector3 directionToHoldPoint = objToHold.GlobalPosition.DirectionTo(cameraHoldObj.GlobalPosition);
		float distance = objToHold.GlobalPosition.DistanceTo(cameraHoldObj.GlobalPosition);
		float normalizedMultiplier = distance/maximumDistance;
		//float inversed = Mathf.Abs(normalizedMultiplier - 1.0f);
		objToHold.Rotation = objectRotation;
		objToHold.ApplyForce(directionToHoldPoint * objectAcceleration * normalizedMultiplier);
		objToHold.ApplyForce(-objToHold.LinearVelocity * objectFrictionMultiplier);
	}

	private void hold()
	{
		// is there an object to hold 
		if (!holdCaster.IsColliding()) { return; }
		if (holdCaster.GetCollider(0) is StaticBody3D && holdCaster.GetCollisionCount() == 1) {return;}
		if (holdCaster.GetCollider(0) == rigidbody && holdCaster.GetCollisionCount() == 1) {return;}
		ePressedCount++;
		holdingObject = true;

		int colliderCount = holdCaster.GetCollisionCount();
		RigidBody3D closestCollider = null;
		if (colliderCount != 1)
		{
			
			Debug.WriteLine("Hold multiple GameObjects. Scanning for distance");
			
			float closestColliderDistance = -1;

			for (int i = 0; i < colliderCount; ++i)
			{
				if (!(holdCaster.GetCollider(i) is RigidBody3D)) {continue;}
				if (holdCaster.GetCollider(i) == rigidbody) {continue;}
				RigidBody3D testCollider = (RigidBody3D) holdCaster.GetCollider(i);
				float distance = (testCollider.GlobalPosition - camera.GlobalPosition).Length();
				if (closestCollider == null || closestColliderDistance == -1)
				{
					closestCollider = testCollider;
					closestColliderDistance = distance; 
					continue;
				}
				if (distance < closestColliderDistance)
				{
					closestCollider = testCollider;
					closestColliderDistance = distance;
				}
			}
			Debug.WriteLine("Closest collider found!");
		}
		else
		{
			Debug.WriteLine("One gameobject detected");
			closestCollider = (RigidBody3D) holdCaster.GetCollider(0);
		}
		
		closestCollider.SetCollisionLayerValue(3, false);
		closestCollider.SetCollisionMaskValue(3, false);
		closestCollider.SetCollisionLayerValue(1, false);
		closestCollider.SetCollisionMaskValue(1, false);
		objToHold = closestCollider;
		objectRotation = objToHold.Rotation;
		objToHold.GravityScale = 0.0f;
	}
	private void unhold()
	{
		lastThrowPass = false;
		title.StopInteract();
		objToHold.AngularVelocity = Vector3.Zero;
		objToHold.GravityScale = 1.0f;
		objToHold.SetCollisionLayerValue(3, true);
		objToHold.SetCollisionMaskValue(3, true);
		objToHold.SetCollisionLayerValue(1, true);
		objToHold.SetCollisionMaskValue(1, true);
		objToHold = null;
		holdingObject = false;
		unholdCooldownTimer.Start();
	}
	
	private void throwObj() {
		FirstObjectThrowCheck = false;
		Vector3 directionToThrow = camera.GlobalPosition.DirectionTo(cameraHoldObj.GlobalPosition);
		objToHold.ApplyImpulse(directionToThrow * throwAcceleration);
		unhold();
	}


	private void HandlePickup()
	{	
		if (Input.IsKeyPressed(Key.E))
		{
			if (!holdingObject)
			{
				if (unholdCooldownTimer.TimeLeft != 0) { return; }
				hold();
				DoFirstObjectInteractCheck = false;
			}
			else
			{
				if (Input.IsMouseButtonPressed(MouseButton.Left))
				{
					throwObj();
				}
				else
				{
					objectHoldLoop();
				}
			}
		}
		else
		{
			if (holdingObject)
			{
				unhold();
			}
		}
	}
	private void HandleViewBobbing()
	{
		Vector3 bob(float noiseDenominator, float timeDenominator, float offsetMultiplier)
		{
			float time = Time.GetTicksMsec();

			float xNoise = fnl.GetNoise1D((time - flashlight.flashlightBody.Position.Y) / noiseDenominator);
			float yNoise = fnl.GetNoise1D((time + flashlight.flashlightBody.Position.X) / noiseDenominator);
			
			float xOffset = Mathf.Cos(Mathf.Sin(time / timeDenominator));
			float yOffset = Mathf.Cos(Mathf.Sin(time / timeDenominator));

			xOffset = (xOffset + xNoise) * offsetMultiplier;
			yOffset = (yOffset + yNoise) * offsetMultiplier;

			Vector3 offset = new Vector3(xOffset, yOffset, 0);
			return offset;
		}


		Vector3 offset;
		if (isMoving)
		{
			offset = bob(viewBobNoiseDenominator, viewBobTimeDenominator, maxViewBobOffsetMultiplier);
		}
		else
		{
			offset = bob(viewBobNoiseDenominatorIdle, viewBobTimeDenominatorIdle, maxViewBobOffsetMultiplierIdle);
		}
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(flashlight.flashlightBody, "position", flashlight.startPos.Position + offset, 0.05f);
		//flashlight.flashlightBody.Position = flashlight.startPos.Position + offset;
	}
	private async Task EquipFlashlight()
	{
		flashlightBusy = true;
		isFlashlightEquipped = true;
		flashlight.flashlightBody.Position = flashlight.startPos.Position + new Vector3(0,flashlightStartYOffset,0);
		flashlight.flashlightBody.Visible = true;
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(flashlight.flashlightBody, "position", flashlight.startPos.Position, flashlightTweenTime);
		tween.TweenProperty(flashlightSpot, "light_energy", 1.0f, flashlightTweenTime);
		flashlight.click.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
		flashlightBusy = false;
	}
	private async Task UnequipFlashlight()
	{
		flashlightBusy = true;
		isFlashlightEquipped = false;
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(flashlight.flashlightBody, "position", flashlight.startPos.Position + new Vector3(0,flashlightStartYOffset,0), flashlightTweenTime);
		tween.TweenProperty(flashlightSpot, "light_energy", 0.0f, flashlightTweenTime);
		flashlight.click.Play();
		await ToSignal(tween, Tween.SignalName.Finished);
		flashlight.flashlightBody.Visible = false;
		flashlightBusy = false;
	}
	public void PlayMusic(string name)
	{
		
		AudioStreamPlayer3D song =  music.GetNodeOrNull<AudioStreamPlayer3D>(name);
		if (song != null)
		{
			song.Play();
		}
	}
	public void FadeMusic(string name, float timeToFade)
	{
		
		AudioStreamPlayer3D song =  music.GetNodeOrNull<AudioStreamPlayer3D>(name);
		if (song != null)
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(song, "volume_linear", 0, timeToFade);
		}
	}
	public async Task HandleOtherKeys()
	{
		if (Input.IsKeyPressed(Key.F) && flashlightBusy == false)
		{
			if (CanUseFlashlight)
			{
				if (isFlashlightEquipped)
				{
					await UnequipFlashlight();
				}
				else
				{
					await EquipFlashlight();
				}
			}
		}
	}
	public ChapterTitle GetChapterTitle()
	{
		return title;
	}

	public bool IsHoldingObject()
	{
		return holdingObject;
	}
	public override void _PhysicsProcess(double delta)
	{
		UpdateMovement(delta);
		interactableClass.Update();
		HandlePickup();
		HandleOtherKeys();
		if (isFlashlightEquipped && flashlightBusy == false) {HandleViewBobbing();}
	}

}
