using UnityEngine;
using System.Collections;


// Require a character controller to be attached to the same game object
[RequireComponent ( typeof(CharacterController) )]
[AddComponentMenu ("Character/Character Motor")]

public class CharacterMotorCS : MonoBehaviour 
{
	#region Movement Variables
	// The maximum horizontal speed when moving
	public float maxForwardSpeed = 8.0f;
	public float maxSidewaysSpeed = 8.0f;
	public float maxBackwardsSpeed = 8.0f;
	
	// Curve for multiplying speed based on slope (negative = downwards)
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve( new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0) );
	
	// How fast does the character change speeds?  Higher is faster.
	public float maxGroundAcceleration = 40.0f;
	public float maxAirAcceleration = 20.0f;

	// The gravity for the character
	public float gravity = 10.0f;
	public float maxFallSpeed = 20.0f;
	
	// For the next variables, @System.NonSerialized tells Unity to not serialize the variable or show it in the inspector view.
	// Very handy for organization!

	// The last collision flags returned from controller.Move
	[System.NonSerialized]
	public CollisionFlags collisionFlags; 

	// We will keep track of the character's current velocity,
	[System.NonSerialized]
	public Vector3 movVelocity;
	
	// This keeps track of our current velocity while we're not grounded
	[System.NonSerialized]
	public Vector3 frameVelocity = Vector3.zero;
	
	[System.NonSerialized]
	public Vector3 hitPoint = Vector3.zero;
	
	[System.NonSerialized]
	public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
	#endregion


	// Does this script currently respond to input?
	private bool canControl = true;
	
	private bool useFixedUpdate = true;
	
	// For the next variables, @System.NonSerialized tells Unity to not serialize the variable or show it in the inspector view.
	// Very handy for organization!
	
	// The current global direction we want the character to move in.
	[System.NonSerialized]
	public Vector3 inputMoveDirection = Vector3.zero;
	
	// Is the jump button held down? We use this interface instead of checking
	// for the jump button directly so this script can also be used by AIs.
	[System.NonSerialized]
	public bool inputJump = false;
	
	[System.NonSerializedAttribute]
	public bool inputCrouch = false;
	
	private enum MovementTransferOnJump 
	{
		None, // The jump is not affected by velocity of floor at all.
		InitTransfer, // Jump gets its initial velocity from the floor, then gradualy comes to a stop.
		PermaTransfer, // Jump gets its initial velocity from the floor, and keeps that velocity until landing.
		PermaLocked // Jump is relative to the movement of the last touched floor and will move together with that floor.
	}
	
	CharacterMotorJumping jumping = new CharacterMotorJumping();

	public CharacterMotorSliding sliding = new CharacterMotorSliding();
	
	[System.NonSerialized]
	public bool grounded = true;
	
	[System.NonSerialized]
	public Vector3 groundNormal = Vector3.zero;
	
	private Vector3 lastGroundNormal = Vector3.zero;
	
	private Transform tr;
	
	private CharacterController controller;
	
	void Awake () 
	{
		controller = GetComponent<CharacterController>();
		tr = transform;
	}

	private void UpdateFunction () 
	{
		// We copy the actual velocity into a temporary variable that we can manipulate.
		Vector3 velocity = movVelocity;
		
		// Update velocity based on input
		velocity = ApplyInputVelocityChange(velocity);
		
		// Apply gravity and jumping force
		velocity = ApplyGravityAndJumping (velocity);
	
		// Save lastPosition for velocity calculation.
		Vector3 lastPosition = tr.position;
		
		// We always want the movement to be framerate independent.  Multiplying by Time.deltaTime does this.
		Vector3 currentMovementOffset = velocity * Time.deltaTime;
		
		// Find out how much we need to push towards the ground to avoid loosing grouning
		// when walking down a step or over a sharp change in slope.
		float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
		if (grounded)
			currentMovementOffset -= pushDownOffset * Vector3.up;
		
		// Reset variables that will be set by collision function
		groundNormal = Vector3.zero;
		
	   	// Move our character!
		collisionFlags = controller.Move (currentMovementOffset);
		
		lastHitPoint = hitPoint;
		lastGroundNormal = groundNormal;
		
		// Calculate the velocity based on the current and previous position.  
		// This means our velocity will only be the amount the character actually moved as a result of collisions.
		Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
		movVelocity = (tr.position - lastPosition) / Time.deltaTime;
		Vector3 newHVelocity = new Vector3( movVelocity.x, 0, movVelocity.z);
		
		// The CharacterController can be moved in unwanted directions when colliding with things.
		// We want to prevent this from influencing the recorded velocity.
		if (oldHVelocity == Vector3.zero) 
		{
			movVelocity = new Vector3(0, movVelocity.y, 0);
		}
		else 
		{
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			movVelocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movVelocity.y * Vector3.up;
		}
		
		if ( movVelocity.y < velocity.y - 0.001) 
		{
			if ( movVelocity.y < 0)
			{
				// Something is forcing the CharacterController down faster than it should.
				// Ignore this
				movVelocity.y = velocity.y;
			}
			else 
			{
				// The upwards movement of the CharacterController has been blocked.
				// This is treated like a ceiling collision - stop further jumping here.
				jumping.holdingJumpButton = false;
			}
		}
		
		// We were grounded but just loosed grounding
		if (grounded && !IsGroundedTest()) 
		{
			grounded = false;
			
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			// We pushed the character down to ensure it would stay on the ground if there was any.
			// But there wasn't so now we cancel the downwards offset to make the fall smoother.
			tr.position += pushDownOffset * Vector3.up;
		}
		
		// We were not grounded but just landed on something
		else if (!grounded && IsGroundedTest()) 
		{
			grounded = true;
			jumping.jumping = false;
			//SubtractNewPlatformVelocity();
			
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	void FixedUpdate () 
	{	
		if (useFixedUpdate)
			UpdateFunction();
	}
	
	void Update () {
		if (!useFixedUpdate)
			UpdateFunction();
	}
	
	private Vector3 ApplyInputVelocityChange ( Vector3 velocity ) 
	{	
		if (!canControl)
			inputMoveDirection = Vector3.zero;
		
		// Find desired velocity
		Vector3 desiredVelocity;
		if (grounded && TooSteep()) 
		{
			// The direction we're sliding in
			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
			// Find the input movement direction projected onto the sliding direction
			Vector3 projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);
			// Add the sliding direction, the spped control, and the sideways control vectors
			desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;
			// Multiply with the sliding speed
			desiredVelocity *= sliding.slidingSpeed;
		}
		else
			desiredVelocity = GetDesiredHorizontalVelocity();
		
		if (grounded)
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
		else
			velocity.y = 0;
		
		// Enforce max velocity change
		float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 velocityChangeVector = desiredVelocity - velocity;
		if ( velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange ) 
		{
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}
		
		// If we're in the air and don't have control, don't apply any velocity change at all.
		// If we're on the ground and don't have control we do apply it - it will correspond to friction.
		if (grounded || canControl)
			velocity += velocityChangeVector;
		
		if (grounded) 
		{
			// When going uphill, the CharacterController will automatically move up by the needed amount.
			// Not moving it upwards manually prevent risk of lifting off from the ground.
			// When going downhill, DO move down manually, as gravity is not enough on steep hills.
			velocity.y = Mathf.Min(velocity.y, 0);
		}
		
		return velocity;
	}
	
	private Vector3 ApplyGravityAndJumping ( Vector3 velocity ) 
	{
		
		if (!inputJump || !canControl) 
		{
			jumping.holdingJumpButton = false;
			jumping.lastButtonDownTime = -100f;
		}
		
		if (inputJump && jumping.lastButtonDownTime < 0 && canControl)
			jumping.lastButtonDownTime = Time.time;
		
		if (grounded)
			velocity.y = Mathf.Min(0, velocity.y) - gravity * Time.deltaTime;
		else 
		{
			velocity.y = movVelocity.y - gravity * Time.deltaTime;
			
			// When jumping up we don't apply gravity for some time when the user is holding the jump button.
			// This gives more control over jump height by pressing the button longer.
			if (jumping.jumping && jumping.holdingJumpButton) 
			{
				// Calculate the duration that the extra jump force should have effect.
				// If we're still less than that duration after the jumping time, apply the force.
				if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)) 
				{
					// Negate the gravity we just applied, except we push in jumpDir rather than jump upwards.
					velocity += jumping.jumpDir * gravity * Time.deltaTime;
				}
			}
			
			// Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
			velocity.y = Mathf.Max (velocity.y, -maxFallSpeed);
		}
			
		if (grounded) 
		{
			// Jump only if the jump button was pressed down in the last 0.2 seconds.
			// We use this check instead of checking if it's pressed down right now
			// because players will often try to jump in the exact moment when hitting the ground after a jump
			// and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
			// it's confusing and it feels like the game is buggy.
			if (jumping.enabled && canControl && (Time.time - jumping.lastButtonDownTime < 0.2)) 
			{
				grounded = false;
				jumping.jumping = true;
				jumping.lastStartTime = Time.time;
				jumping.lastButtonDownTime = -100;
				jumping.holdingJumpButton = true;
				
				// Calculate the jumping direction
				if (TooSteep())
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
				else
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
				
				// Apply the jumping force to the velocity. Cancel any vertical velocity first.
				velocity.y = 0;
				velocity += jumping.jumpDir * CalculateJumpVerticalSpeed (jumping.baseHeight);
				
				SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
			}
			else 
			{
				jumping.holdingJumpButton = false;
			}
		}
		
		return velocity;
	}
	
	void OnControllerColliderHit ( ControllerColliderHit hit )
	{
		if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0) {
			if ((hit.point - lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
				groundNormal = hit.normal;
			else
				groundNormal = lastGroundNormal;
			
			hitPoint = hit.point;
			frameVelocity = Vector3.zero;
		}
	}
	
	private Vector3 GetDesiredHorizontalVelocity () 
	{
		// Find desired velocity
		Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
		float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);
		if (grounded) 
		{
			// Modify max speed on slopes based on slope speed multiplier curve
			float movementSlopeAngle = Mathf.Asin( movVelocity.normalized.y)  * Mathf.Rad2Deg;
			maxSpeed *= slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
		}
		return tr.TransformDirection(desiredLocalDirection * maxSpeed);
	}
	
	private Vector3 AdjustGroundVelocityToNormal ( Vector3 hVelocity, Vector3 groundNormal ) 
	{
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
	}
	
	private bool IsGroundedTest () {
		return (groundNormal.y > 0.01);
	}
	
	float GetMaxAcceleration ( bool grounded )
	{
		// Maximum acceleration on ground and in air
		if (grounded)
			return maxGroundAcceleration;
		else
			return maxAirAcceleration;
	}
	
	float CalculateJumpVerticalSpeed ( float targetJumpHeight ) 
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * gravity);
	}
	
	bool IsJumping () {
		return jumping.jumping;
	}
	
	bool IsSliding () {
		return (grounded && sliding.enabled && TooSteep());
	}
	
	bool IsTouchingCeiling () {
		return (collisionFlags & CollisionFlags.CollidedAbove) != 0;
	}
	
	bool IsGrounded () {
		return grounded;
	}
	
	bool TooSteep () {
		return (groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad));
	}
	
	Vector3 GetDirection () 
	{
		return inputMoveDirection;
	}
	
	void SetControllable ( bool controllable )
	{
		canControl = controllable;
	}
	
	// Project a direction onto elliptical quarter segments based on forward, sideways, and backwards speed.
	// The function returns the length of the resulting vector.
	float MaxSpeedInDirection ( Vector3 desiredMovementDirection )
	{
		if (desiredMovementDirection == Vector3.zero)
			return 0f;
		else 
		{
			float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
			Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
			float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
			return length;
		}
	}
	
	void SetVelocity ( Vector3 velocity )
	{
		grounded = false;
		movVelocity = velocity;
		frameVelocity = Vector3.zero;
		SendMessage("OnExternalVelocity");
	}
}

// We will contain all the jumping related variables in one helper class for clarity.
public class CharacterMotorJumping 
{
	// Can the character jump?
	public bool enabled = true;

	// How high do we jump when pressing jump and letting go immediately
	public float baseHeight = 1.0f;
	
	// We add extraHeight units (meters) on top when holding the button down longer while jumping
	public float extraHeight = 1f;
	
	// How much does the character jump out perpendicular to the surface on walkable surfaces?
	// 0 means a fully vertical jump and 1 means fully perpendicular.
	public float perpAmount = 0.0f;
	
	// How much does the character jump out perpendicular to the surface on too steep surfaces?
	// 0 means a fully vertical jump and 1 means fully perpendicular.
	public float steepPerpAmount = 0.5f;
	
	// For the next variables, @System.NonSerialized tells Unity to not serialize the variable or show it in the inspector view.
	// Very handy for organization!

	// Are we jumping? (Initiated with jump button and not grounded yet)
	// To see if we are just in the air (initiated by jumping OR falling) see the grounded variable.
	[System.NonSerialized]
	public bool jumping = false;
	
	[System.NonSerialized]
	public bool holdingJumpButton = false;

	// the time we jumped at (Used to determine for how long to apply extra jump power after jumping.)
	[System.NonSerialized]
	public float lastStartTime = 0.0f;
	
	[System.NonSerialized]
	public float lastButtonDownTime  = -100f;
	
	[System.NonSerialized]
	public Vector3 jumpDir = Vector3.up;
}

public class CharacterMotorMovement 
{
	// The maximum horizontal speed when moving
	public float maxForwardSpeed = 8.0f;
	public float maxSidewaysSpeed = 8.0f;
	public float maxBackwardsSpeed = 8.0f;
	
	// Curve for multiplying speed based on slope (negative = downwards)
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve( new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0) );
		
	// How fast does the character change speeds?  Higher is faster.
	public float maxGroundAcceleration = 40.0f;
	public float maxAirAcceleration = 20.0f;

	// The gravity for the character
	public float gravity = 10.0f;
	public float maxFallSpeed = 20.0f;
	
	// For the next variables, @System.NonSerialized tells Unity to not serialize the variable or show it in the inspector view.
	// Very handy for organization!

	// The last collision flags returned from controller.Move
	[System.NonSerialized]
	public CollisionFlags collisionFlags; 

	// We will keep track of the character's current velocity,
	[System.NonSerialized]
	public Vector3 velocity;
	
	// This keeps track of our current velocity while we're not grounded
	[System.NonSerialized]
	public Vector3 frameVelocity = Vector3.zero;
	
	[System.NonSerialized]
	public Vector3 hitPoint = Vector3.zero;
	
	[System.NonSerialized]
	public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
}

public class CharacterMotorSliding 
{
	// Does the character slide on too steep surfaces?
	public bool enabled = true;
	
	// How fast does the character slide on steep surfaces?
	public float slidingSpeed = 15f;
	
	// How much can the player control the sliding direction?
	// If the value is 0.5 the player can slide sideways with half the speed of the downwards sliding speed.
	public float sidewaysControl = 1.0f;
	
	// How much can the player influence the sliding speed?
	// If the value is 0.5 the player can speed the sliding up to 150% or slow it down to 50%.
	public float speedControl = 0.4f;
}