using UnityEngine;
using System.Collections;

public class FPSInput : MonoBehaviour 
{
	CharacterMotorCS motor;
	
	private bool useVCR;
	private InputVCR vcr;

	void Awake()
	{
		motor = GetComponent<CharacterMotorCS>();	
		
		Transform root = transform;
		while ( root.parent != null )
			root = root.parent;
		vcr = root.GetComponent<InputVCR>();
		useVCR = vcr != null;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Get the input vector from kayboard or analog stick
		Vector3 directionVector;
		if ( useVCR )
			directionVector = new Vector3( vcr.GetAxis ( "Horizontal" ), 0, vcr.GetAxis ( "Vertical" ) );
		else
			directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical" ) );
	
		if (directionVector != Vector3.zero)
		{
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			float directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
		
		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = transform.rotation * directionVector;
		motor.inputJump = useVCR && vcr.GetButton ( "Jump" ) || !useVCR && Input.GetButton("Jump");
	}
}
