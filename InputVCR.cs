/* InputVCR.cs - Eddie Cameron
 * ----------
 * Place on any object you wish to use to record or playback any inputs for
 * Switch modes to change current behaviour
 *   - Passthru : object will use live input commands from player
 *   - Record : object will use, as well as record, live input commands from player
 *   - Playback : object will use either provided input string or last recorded string rather than live input
 *   - Pause : object will take no input (buttons/axis will be frozen in last positions)
 * 
 * To use, place in a gameobject, and have all scripts in the object refer to it instead of Input.
 * 
 * eg: instead of Input.GetButton( "Jump" ), you would use vcr.GetButton( "Jump" ), where vcr is a 
 * reference to the component in that object
 * If VCR is in playback mode, and the "Jump" input was recorded, it will give the recorded input state, 
 * otherwise it will just pass through the live input state
 * 
 * Note, InputVCR can't be statically referenced like Input, since you may have multiple objects playing
 * different recordings, or an object playing back while another is taking live input...
 * ----------
 * Use this snippet in scripts you wish to replace Input with InputVCR, so they can be used in objects without a VCR as well:
 
  private bool useVCR;
  private InputVCR vcr;
  
  void Awake()
  {
    GameObject root = gameObject;
	while ( root.transform.parent != null )
		root = root.transform.parent;
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
 * Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
 * Easy! 
 */ 

using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class InputVCR : MonoBehaviour 
{
	public InputInfo[] inputsToRecord;  // list of axis and button names ( from Input manager) that should be recorded
	public bool recordMouseEvents;		// whether mouse position/button states should be recorded each frame (mouse axes are separate from this)
	private string recordString;
	
	private StringReader playbackReader;
	
	private Dictionary<string, InputInfo> watchedInputs;	// list of inputs currently being recorded, or contains most recent inputs during playback
	private bool mouseIsWatched;
	private Vector3 lastMousePosition;						// holds mouse position if was or is being recorded
	
	[SerializeField]
	private InputVCRMode _mode = InputVCRMode.Passthru; // mode that vcr is operating in
	public InputVCRMode mode
	{
		get { return _mode; }
	}
	
	public void Record()
	{
		// start recording live input
		
		// set up all inputs that should be recorded
		watchedInputs = new Dictionary<string, InputInfo>();
		foreach( InputInfo input in inputsToRecord )
		{
			input.mouseButtonNum = -1;
			watchedInputs.Add( input.inputName, input );
		}
		
		// add mouse buttons if needed
		if ( recordMouseEvents )
		{
			for ( int i = 0; i < 3; i++ )
			{
				InputInfo mouseInput = new InputInfo();
				mouseInput.inputName = "mousebutton" + i;
				mouseInput.isAxis = false;
				mouseInput.mouseButtonNum = i;
				watchedInputs.Add( mouseInput.inputName, mouseInput );
			}			
		}
		
		_mode = InputVCRMode.Record;
		playbackReader = null;
	}
	
	public void Play()
	{
		// if currently paused during playback, will continue
		if ( playbackReader != null )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recorded string
			Play ( recordString );
		}
	}
	
	public void Play( string inputString )
	{
		// restart playback with given input string
		playbackReader = new StringReader( inputString );
		_mode = InputVCRMode.Playback;
	}
	
	public void  Pause()
	{
		_mode = InputVCRMode.Pause;
	}
	
	public void Stop()
	{
		_mode = InputVCRMode.Passthru;
		playbackReader = null;
		watchedInputs = new Dictionary<string, InputInfo>();
	}
	
	void LateUpdate()
	{	
		if ( _mode == InputVCRMode.Record )
		{
			// record any inputs that should be
			StringBuilder sb = new StringBuilder();
			
			// mouse position if required
			if ( recordMouseEvents )
				sb.Append ( "mousepos," + Input.mousePosition.x.ToString () + "," + Input.mousePosition.y );
			
			// and buttons
			foreach( InputInfo input in watchedInputs.Values )
				sb.Append ( "{" + input.ToString () + "}" );
			
			sb.AppendLine ();
			recordString += sb.ToString ();
		}
		else if ( _mode == InputVCRMode.Playback )
		{
			// parse next frame from playbackreader and place info into watchedInputs to be read from  when needed
			watchedInputs = new Dictionary<string, InputInfo>();
			mouseIsWatched = false;
			
			try
			{
				if ( playbackReader.Peek() != -1 )
				{
					string[] inputs = playbackReader.ReadLine ().Split ( "{".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries );
					if ( inputs.Length > 0 )
					{
						foreach( string inputInfo in inputs )
						{
							if ( inputInfo[inputInfo.Length - 1] != '}' )
							{
								// not in InputInfo format, so should be mouseposition
								mouseIsWatched = true;
								string[] mouse = inputInfo.Split ( ",".ToCharArray() );
								lastMousePosition.x = float.Parse ( mouse[1] );
								lastMousePosition.y = float.Parse ( mouse[2] );
							}
							else
							{
								// strip closing bracket and make new InputInfo object
								InputInfo newInput = new InputInfo( inputInfo.Remove ( inputInfo.Length - 1 ) );
								watchedInputs.Add ( newInput.inputName, newInput );
							}
						}
					}
				}
				else
					Stop ();
			}
			catch ( System.Exception e )
			{
				Debug.LogWarning ( e.Message );
				Debug.LogWarning ( playbackReader.ReadToEnd () );
				watchedInputs = new Dictionary<string, InputInfo>();
				mouseIsWatched = false;
			}
		}
	}
	
	// These methods replace those in Input, so that this object can ignore whether it is record
	#region Input replacements
	public bool GetButton( string buttonName )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( buttonName ) )
			return watchedInputs[buttonName].buttonState;
		else
			return Input.GetButton ( buttonName );
	}
	
	public bool GetButtonDown( string buttonName )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return watchedInputs[buttonName].justDown;
		else
			return Input.GetButtonDown ( buttonName );
	}
	
	public bool GetButtonUp( string buttonName )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return watchedInputs[buttonName].justUp;
		else
			return Input.GetButtonUp ( buttonName );
	}
	
	public float GetAxis( string axisName )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( axisName ) )
			return watchedInputs[axisName].axisValue;
		else
			return Input.GetAxis ( axisName );
	}
	
	public bool GetMouseButton( int buttonNum )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].buttonState;
		else
			return Input.GetMouseButton( buttonNum );
	}
	
	public bool GetMouseButtonDown( int buttonNum )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].justDown;
		else
			return Input.GetMouseButtonDown( buttonNum );
	}
	
	public bool GetMouseButtonUp( int buttonNum )
	{
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].justUp;
		else
			return Input.GetMouseButtonUp( buttonNum );
	}
	
	public Vector3 mousePosition
	{
		get {
			if ( _mode == InputVCRMode.Playback && mouseIsWatched )
				return lastMousePosition;
			else
				return Input.mousePosition;
		}
	}
	#endregion
}

public enum InputVCRMode
{
	Passthru,	// normal input
	Record,
	Playback,
	Pause
}

[System.Serializable]
public class InputInfo	// represents all input during one frame
{
	public string inputName;	// make sure this doesn't have any of { } , characters
	public bool isAxis;
	
	[HideInInspector]
	public int mouseButtonNum = -1; // only positive if is mouse button
	
	[HideInInspector]
	public bool buttonState;
	[HideInInspector]
	public bool justDown;
	[HideInInspector]
	public bool justUp;
	
	[HideInInspector]
	public float axisValue;	// not raw value
	
	// make blank info object
	public InputInfo()
	{
		inputName = "";
		mouseButtonNum = -1;
		isAxis = false;
		buttonState = false;
		justDown = false;
		justUp = false;
		axisValue = 0f;
	}
	
	/* storeed in formats
	 * for axis - "<inputName>,a,<axisvalue>"
	 * for button - "<inputName>,b,<buttonState>,<justDown>,<justUp>"
	 * for mousebutton "mousebutton<mouseButtonNum>,b,<buttonState>,<justDown>,<justUp>"
	 */
	
	// Parse input info from string
	#region Parsing
	public InputInfo( string inputString )
	{			
		string[] values = inputString.Split ( ",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries );
		if ( values.Length != 3 && values.Length != 5 )
			Debug.LogWarning ( "Invalid recorded input\n" + inputString );
		else
		{
			inputName = values[0];
			
			if ( inputName.StartsWith ( "mousebutton" ) )
				mouseButtonNum = int.Parse ( inputName.Substring ( inputName.Length - 1, 1 ) );
			
			if ( values[1] == "a" )
			{
				isAxis = true;
				axisValue = float.Parse ( values[2] );
			}
			else
			{
				isAxis = false;
				if ( values[2] == "d" )
				{
					buttonState = true;
					justDown = values[3] == "1";
					justUp = false;
				}
				else
				{
					buttonState = false;
					justDown = false;
					justUp = values[4] == "1";
				}
			}
		}
	}
	
	// convert to correctly formatted string
	public override string ToString()
	{		
		StringBuilder sb = new StringBuilder( );
		if ( mouseButtonNum < 0 )
			sb.Append( inputName );
		else
			sb.Append ( "mousebutton" + mouseButtonNum.ToString () );
		
		
		if ( isAxis )
		{
			sb.Append ( ",a," );
			sb.Append ( Input.GetAxis ( inputName ).ToString() );
		}
		else
		{
			sb.Append ( ",b" );
			if ( ( mouseButtonNum < 0 && Input.GetButton( inputName ) ) || ( mouseButtonNum >= 0 && Input.GetMouseButton ( mouseButtonNum ) ) )
			{
				sb.Append ( ",d" );
				if ( ( mouseButtonNum < 0 && Input.GetButtonDown( inputName ) ) || ( mouseButtonNum >= 0 && Input.GetMouseButtonDown ( mouseButtonNum ) ) ) 
					sb.Append ( ",1" );
				else
					sb.Append( ",0" );
				sb.Append( ",u" );
			}
			else
			{
				sb.Append ( ",u,u," );
				if ( ( mouseButtonNum < 0 && Input.GetButtonUp ( inputName ) ) || ( mouseButtonNum >= 0 && Input.GetMouseButtonUp( mouseButtonNum ) ) )
					sb.Append ( ",1" );
				else
					sb.Append ( ",0" );
			}
		}
		return sb.ToString ();
	}
	#endregion
}
