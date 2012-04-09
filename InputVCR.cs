<<<<<<< HEAD
/* InputVCR.cs - copyright Eddie Cameron 2012
=======
/* InputVCR.cs - Eddie Cameron
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
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
<<<<<<< HEAD
    Transform root = transform;
	while ( root.parent != null )
		root = root.parent;
=======
    GameObject root = gameObject;
	while ( root.transform.parent != null )
		root = root.transform.parent;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
 * Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
 * Easy! 
<<<<<<< HEAD
 * -------------
 * More information and tools at grapefruitgames.com, @eddiecameron, or support@grapefruitgames.com
 * 
 * This script is open source under the GNU LGPL licence. Do what you will with it! 
 * http://www.gnu.org/licenses/lgpl.txt
 * 
=======
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
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
<<<<<<< HEAD
	
	public bool syncRecordLocations = true;
	float nextSyncTime = -1f;
	public bool snapToSyncedLocation = true;	// if SyncLocation is called during recording, object will snap to that pos/rot during playback. Otherwise you can handle interpolation if needed
	
	private Recording currentRecording;
	private float recordingTime;
	
	private float _currentPlaybackTime;
	public float currentPlaybackTime{ get { return _currentPlaybackTime; } }
	
	private Queue<string> nextPropertiesToRecord;	// if SyncLocation or SyncProperty are called, this will hold their results until the recordstring is next written to
		
	private Dictionary<string, InputInfo> watchedInputs;	// list of inputs currently being recorded, or contains most recent inputs during playback
	private Dictionary<string, InputInfo> lastInputs;		// list of inputs from last frame
	private Dictionary<string, string> watchedProperties;	// list of properties that were recorded this frame (during playback)
=======
	private string recordString;
	
	private StringReader playbackReader;
	
	private Dictionary<string, InputInfo> watchedInputs;	// list of inputs currently being recorded, or contains most recent inputs during playback
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	private bool mouseIsWatched;
	private Vector3 lastMousePosition;						// holds mouse position if was or is being recorded
	
	[SerializeField]
	private InputVCRMode _mode = InputVCRMode.Passthru; // mode that vcr is operating in
	public InputVCRMode mode
	{
		get { return _mode; }
	}
	
<<<<<<< HEAD
	public event System.Action finishedPlayback;	// sent when playback finishes
	
	void Awake()
	{
		watchedInputs = new Dictionary<string, InputInfo>();
	}
	
	public void Record()
	{
		// start recording live input
		currentRecording = new Recording("");
		recordingTime = 0f;
		nextSyncTime = -1f;
		nextPropertiesToRecord = new Queue<string>();
=======
	public void Record()
	{
		// start recording live input
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		
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
<<<<<<< HEAD
=======
		playbackReader = null;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	}
	
	public void Play()
	{
		// if currently paused during playback, will continue
<<<<<<< HEAD
		if ( mode == InputVCRMode.Pause )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recording
			Play ( currentRecording );
=======
		if ( playbackReader != null )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recorded string
			Play ( recordString );
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		}
	}
	
	public void Play( string inputString )
	{
		// restart playback with given input string
<<<<<<< HEAD
		Play ( new Recording( inputString ) );
	}
	
	public void Play( Recording recording, float startRecordingFrom = 0f )
	{
		StopCoroutine ( "PlayRecording" );
		_mode = InputVCRMode.Playback;
		_currentPlaybackTime = startRecordingFrom;
		StartCoroutine ( "PlayRecording", recording );
	}
	
	public void Pause()
	{
		// toggles pause
		if ( _mode == InputVCRMode.Pause )
			_mode = InputVCRMode.Playback;
		else
			_mode = InputVCRMode.Pause;
	}
	
	public void Stop()
	{			
		_mode = InputVCRMode.Passthru;
		watchedInputs = new Dictionary<string, InputInfo>();
	}
	
	/// <summary>
	/// Records the location/rotation of this object during a recording, so when it is played back, object is sure to be here.
	/// Use this if you have drift(and don't want it) in your recordings due to physics/other external inputs.
	/// </summary>
	public void SyncPosition()
	{
		if ( mode != InputVCRMode.Record )
		{
			Debug.LogWarning ( "Tried to record location, but VCR isn't recording" );
			return;
		}
		
		nextPropertiesToRecord.Enqueue ( "{position=" + transform.position.x.ToString () + "," + transform.position.y.ToString () + "," + transform.position.z.ToString () + "}" );
		nextPropertiesToRecord.Enqueue ( "{rotation=" + transform.eulerAngles.x.ToString () + "," + transform.eulerAngles.y.ToString () + "," + transform.eulerAngles.z.ToString() + "}" );
	}
	
	public Recording GetRecording()
	{
		return currentRecording;
	}
	
=======
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
	
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	void LateUpdate()
	{	
		if ( _mode == InputVCRMode.Record )
		{
			// record any inputs that should be
			StringBuilder sb = new StringBuilder();
			
<<<<<<< HEAD
			// record timestamp
			currentRecording.frameTimes.Add ( recordingTime );
			currentRecording.recordingLength = recordingTime;
			recordingTime += Time.deltaTime;
			
			// mouse position if required
			if ( recordMouseEvents )
				sb.Append ( "{mousepos=" + Input.mousePosition.x.ToString () + "," + Input.mousePosition.y + "}" );
			
			// and buttons
			foreach( InputInfo input in watchedInputs.Values )
				sb.Append ( "{input=" + input.ToString () + "}");
			
			if ( syncRecordLocations && recordingTime > nextSyncTime )
			{
				SyncPosition ();
				nextSyncTime = Time.time + 1f;
			}
			
			// and any other properties
			foreach( string propertyString in nextPropertiesToRecord )
				sb.Append ( propertyString );
			nextPropertiesToRecord.Clear ();
			
			currentRecording.frameProperties.Add ( sb.ToString () );
		}
	}
	
	IEnumerator PlayRecording( Recording toPlay )
	{
		// find frame to start from
		int recordingFrame = 0;
		while ( recordingFrame < toPlay.frameTimes.Count && toPlay.frameTimes[recordingFrame] < currentPlaybackTime )
			recordingFrame++;
		
		while( _currentPlaybackTime < toPlay.recordingLength && recordingFrame < toPlay.frameTimes.Count )
		{
			if ( mode == InputVCRMode.Pause )
				yield return 0;
			else
			{
				float nextFrameTime = toPlay.frameTimes[recordingFrame];			
				
				// allow to catch up to recording if it is further ahead
				if ( _currentPlaybackTime < nextFrameTime - Time.deltaTime * 5 )
				{
					Debug.Log ( "Recording ahead: Playtime " + _currentPlaybackTime + " Recordtime " + nextFrameTime );
					_currentPlaybackTime += Time.deltaTime;
					yield return 0;
					continue;
				}
				
				// if playback is more than about 5 frames ahead, of recording, drop some recording frames
				if ( _currentPlaybackTime > nextFrameTime + Time.deltaTime * 5 )	
				{
					Debug.Log ( "Recording behind, skipping frames: Playtime " + _currentPlaybackTime + " Recordtime " + nextFrameTime );
					_currentPlaybackTime += Time.deltaTime;
					recordingFrame += 3;
					yield return 0;
					continue;
				}
				
				// parse next frame from playbackreader and place info into watchedInputs to be read from  when needed
				if ( watchedInputs != null  && watchedInputs.Count > 0 )
					lastInputs = watchedInputs;
					
				watchedInputs = new Dictionary<string, InputInfo>();
				watchedProperties = new Dictionary<string, string>();
				mouseIsWatched = false;
				
				try
				{	
					// separate properties
					string[] inputs = toPlay.frameProperties[recordingFrame].Split ( "{".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries );
						
					for( int i = 0; i < inputs.Length; i++ )
					{
						string inputInfo = inputs[i].TrimEnd ( "}".ToCharArray() );
						
						int equalInd = inputInfo.IndexOf( '=' );
						string recordType = inputInfo.Remove ( equalInd );
						inputInfo = inputInfo.Substring ( equalInd + 1 );
						
						switch( recordType )
						{
						case "mousepos":
							mouseIsWatched = true;
							string[] mouse = inputInfo.Split( ",".ToCharArray() );
							lastMousePosition.x = float.Parse ( mouse[0] );
							lastMousePosition.y = float.Parse ( mouse[1] );
							break;
						case "input":
							InputInfo newInput = new InputInfo( inputInfo );
							watchedInputs.Add( newInput.inputName, newInput );
							break;
						case "position":
							watchedProperties.Add ( "position", inputInfo );
							if ( snapToSyncedLocation )
							{
								string[] pos = inputInfo.Split( ",".ToCharArray () );
								transform.position = new Vector3( float.Parse ( pos[0]), float.Parse ( pos[1] ), float.Parse ( pos[2] ) );
							}
							break;
						case "rotation":
							watchedProperties.Add ( "rotation", inputInfo );
							if ( snapToSyncedLocation )
							{
								Debug.Log ( "Snapping" );
								string[] rot = inputInfo.Split( ",".ToCharArray () );
								transform.eulerAngles = new Vector3( float.Parse ( rot[0]), float.Parse ( rot[1] ), float.Parse ( rot[2] ) );
							}
							break;
						default:
							watchedProperties.Add ( recordType, inputInfo );
							break;
						}
					}
				}
				catch ( System.Exception e )
				{
					Debug.LogWarning ( e.Message );
					Debug.LogWarning ( toPlay.frameProperties[recordingFrame] );
					watchedInputs = new Dictionary<string, InputInfo>();
					mouseIsWatched = false;
					
					if ( finishedPlayback != null )
						finishedPlayback( );
					Stop ();
				}
			
				_currentPlaybackTime += Time.deltaTime;
				recordingFrame++;
				yield return 0;		
			}
		}
		
		// end of recording
		if ( finishedPlayback != null )
			finishedPlayback( );
		Stop ();
=======
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
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	}
	
	// These methods replace those in Input, so that this object can ignore whether it is record
	#region Input replacements
	public bool GetButton( string buttonName )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
=======
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( buttonName ) )
			return watchedInputs[buttonName].buttonState;
		else
			return Input.GetButton ( buttonName );
	}
	
	public bool GetButtonDown( string buttonName )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return ( watchedInputs[buttonName].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( buttonName ) || !lastInputs[buttonName].buttonState ) );
=======
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return watchedInputs[buttonName].justDown;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		else
			return Input.GetButtonDown ( buttonName );
	}
	
	public bool GetButtonUp( string buttonName )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return ( !watchedInputs[buttonName].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( buttonName ) || lastInputs[buttonName].buttonState ) );
=======
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return watchedInputs[buttonName].justUp;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		else
			return Input.GetButtonUp ( buttonName );
	}
	
	public float GetAxis( string axisName )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return 0;
		
=======
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( axisName ) )
			return watchedInputs[axisName].axisValue;
		else
			return Input.GetAxis ( axisName );
	}
	
	public bool GetMouseButton( int buttonNum )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( mouseButtonName ) )
			return watchedInputs[mouseButtonName].buttonState;
=======
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].buttonState;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		else
			return Input.GetMouseButton( buttonNum );
	}
	
	public bool GetMouseButtonDown( int buttonNum )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( mouseButtonName ) )
			return ( watchedInputs[ mouseButtonName ].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( mouseButtonName ) || !lastInputs[mouseButtonName].buttonState ) );
=======
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].justDown;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		else
			return Input.GetMouseButtonDown( buttonNum );
	}
	
	public bool GetMouseButtonUp( int buttonNum )
	{
<<<<<<< HEAD
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return ( !watchedInputs[ mouseButtonName ].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( mouseButtonName ) || lastInputs[mouseButtonName].buttonState ) );
=======
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return watchedInputs["mousebutton" + buttonNum.ToString () ].justUp;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		else
			return Input.GetMouseButtonUp( buttonNum );
	}
	
	public Vector3 mousePosition
<<<<<<< HEAD
	{	
		get {
			if ( _mode == InputVCRMode.Pause )
				return Vector3.zero;
			
=======
	{
		get {
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
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

<<<<<<< HEAD
public struct Recording
{
	public List<float> frameTimes;
	public List<string> frameProperties;
	
	public float recordingLength;
	
	public Recording( string s )
	{
		frameTimes = new List<float>();
		frameProperties = new List<string>();
		
		if ( s == "" )
			return;
		
		string currentLine = "";
		try
		{
			StringReader sr = new StringReader( s );
			currentLine = sr.ReadLine();
			recordingLength = float.Parse( currentLine );
			
			while ( sr.Peek () != -1 )
			{
				currentLine = sr.ReadLine();
				frameTimes.Add ( float.Parse ( currentLine ) );
				currentLine = sr.ReadLine ();
				frameProperties.Add ( currentLine );
			}
		}
		catch( System.Exception e )
		{
			Debug.LogWarning ( "Error reading saved recording\n" + currentLine );
			Debug.LogWarning ( e.Message );
			frameTimes = new List<float>();
			frameProperties = new List<string>();
			recordingLength = 0f;
		}
	}
	
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine ( recordingLength.ToString() );
		for( int i = 0; i < frameTimes.Count; i++ )
		{
			sb.AppendLine ( frameTimes[i].ToString () );
			sb.AppendLine ( frameProperties[i] );
		}
		return sb.ToString ();
	}
}

=======
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
[System.Serializable]
public class InputInfo	// represents all input during one frame
{
	public string inputName;	// make sure this doesn't have any of { } , characters
	public bool isAxis;
	
	[HideInInspector]
	public int mouseButtonNum = -1; // only positive if is mouse button
	
	[HideInInspector]
	public bool buttonState;
<<<<<<< HEAD
=======
	[HideInInspector]
	public bool justDown;
	[HideInInspector]
	public bool justUp;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
	
	[HideInInspector]
	public float axisValue;	// not raw value
	
	// make blank info object
	public InputInfo()
	{
		inputName = "";
		mouseButtonNum = -1;
		isAxis = false;
		buttonState = false;
<<<<<<< HEAD
=======
		justDown = false;
		justUp = false;
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
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
<<<<<<< HEAD
					buttonState = true;
				else
					buttonState = false;
=======
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
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
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
<<<<<<< HEAD
				sb.Append ( ",d" );
			else
				sb.Append ( ",u" );
=======
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
>>>>>>> a4ca38c67cace94d54a4ce798b406ae4c5f1e5d8
		}
		return sb.ToString ();
	}
	#endregion
}
