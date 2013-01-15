/* PositionSmoother.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 * Smoothes out syncing for transforms during playback
 */ 

using UnityEngine;
using System.Collections;

public class PositionSmoother : MonoBehaviour 
{
	InputVCR vcr;
	
	Vector3 lastPos;
	Quaternion lastRot;
	
	Vector3 targPos;
	Quaternion targRot;
	
	public float damping = 10f;	// how fast playback will catch up to recording. Higher = more accurate but less smooth
	
	void Awake()
	{
		vcr = GetComponent<InputVCR>();
		
		targPos = transform.position;
		targRot = transform.rotation;
		lastPos = transform.position;
		lastRot = transform.rotation;
	}
	
	void Update()
	{
		if ( vcr.mode == InputVCRMode.Playback )
		{	
			// will try to guess next target position between network frames. 
			Vector3 posChange = transform.position - lastPos;
			Quaternion rotChange = Quaternion.FromToRotation(  lastRot.eulerAngles, transform.rotation.eulerAngles );
			
			targPos += posChange;
			targRot *= rotChange;
			
			Debug.Log ( "targ" +targPos );
			Debug.Log ( "actual: " + transform.position );
			
			transform.position = Vector3.Lerp ( transform.position, targPos, Time.deltaTime * damping );
			transform.rotation = Quaternion.Lerp ( transform.rotation, targRot, Time.deltaTime * damping );
			
			// update target pos if location was recorded this frame
			string posString = vcr.GetProperty( "position" );
			if ( !string.IsNullOrEmpty( posString ) )
				targPos = InputVCR.ParseVector3 ( posString );
			string rotString = vcr.GetProperty ( "rotation" );
			if ( !string.IsNullOrEmpty ( rotString ) )
				targRot = Quaternion.Euler ( InputVCR.ParseVector3 ( rotString ) );
			
			lastPos = transform.position;
			lastRot = transform.rotation;
		}
		else
		{
			lastPos = targPos = transform.position;
			lastRot = targRot = transform.rotation;
		}
	}
}
