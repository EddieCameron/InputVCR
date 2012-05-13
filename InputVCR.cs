/* InputVCR.cs - copyright Eddie Cameron 2012
 * ----------
 * Place on any object you wish to use to record or playback any inputs for
 * Switch modes to change current behaviour
 *   - Passthru : object will use live input commands from player
 *   - Record : object will use, as well as record, live input commands from player
 *   - Playback : object will use either provided input string or last recorded string rather than live input
 *   - Pause : object will take no input (buttons/axis will be frozen in last positions)
 * 
 * -----------
 * Recordings are all saved to the 'currentRecording' member, which you can get with GetRecording(). This can then be copied 
 * to a new Recording object to be saved and played back later.
 * Call ToString() on these recordings to get a text version of this if you want to save a recording after the program exits.
 * -----------
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
    Transform root = transform;
	while ( root.parent != null )
		root = root.parent;
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
 * Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
 * Easy! 
 * -------------
 * More information and tools at grapefruitgames.com, @eddiecameron, or support@grapefruitgames.com
 * 
 * This script is open source under the GNU LGPL licence. Do what you will with it! 
 * http://www.gnu.org/licenses/lgpl.txt
 * 
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
	
	public bool syncRecordLocations = true;
	float nextSyncTime = -1f;
	public bool snapToSyncedLocation = true;	// if SyncLocation is called during recording, object will snap to that pos/rot during playback. Otherwise you can handle interpolation if needed
	
	private Recording currentRecording;		// the actual recording. Copy or ToString() this to save.
	private float recordingTime;
	
	private float _currentPlaybackTime;
	public float currentPlaybackTime{ get { return _currentPlaybackTime; } }
	private int _currentPlaybackFrame;
	public int currentPlaybackFrame{ get { return _currentPlaybackFrame; } }
	
	private Queue<string> nextPropertiesToRecord;	// if SyncLocation or SyncProperty are called, this will hold their results until the recordstring is next written to
		
	public Dictionary<string, InputInfo> watchedInputs;	// list of inputs currently being recorded, or contains most recent inputs during playback
	private Dictionary<string, InputInfo> lastInputs;		// list of inputs from last frame
	private Dictionary<string, string> watchedProperties;	// list of properties that were recorded this frame (during playback)
	private bool mouseIsWatched;
	private Vector3 lastMousePosition;						// holds mouse position if was or is being recorded
	
	[SerializeField]
	private InputVCRMode _mode = InputVCRMode.Passthru; // mode that vcr is operating in
	public InputVCRMode mode
	{
		get { return _mode; }
	}
	
	public event System.Action finishedPlayback;	// sent when playback finishes
	
	void Awake()
	{
		watchedInputs = new Dictionary<string, InputInfo>();
		watchedProperties = new Dictionary<string, string>();
	}
	
	public void Record()
	{
		if ( currentRecording == null || currentRecording.recordingLength == 0 )
			NewRecording();
		else
			_mode = InputVCRMode.Record;
	}
	
	public void NewRecording()
	{
		// start recording live input
		currentRecording = new Recording("");
		recordingTime = 0f;
		nextSyncTime = -1f;
		nextPropertiesToRecord = new Queue<string>();
		
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
	}
	
	public void Play()
	{
		// if currently paused during playback, will continue
		if ( mode == InputVCRMode.Pause )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recording
			Play ( currentRecording );
		}
	}
	
	public void Play( string inputString )
	{
		// restart playback with given input string
		Play ( new Recording( inputString ) );
	}
	
	public void Play( Recording recording, int startRecordingFromFrame = 0 )
	{
		StopCoroutine ( "PlayRecording" );
		_mode = InputVCRMode.Playback;
		_currentPlaybackFrame = startRecordingFromFrame;
		_currentPlaybackTime = recording.frameTimes[startRecordingFromFrame];
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
	
	/// <summary>
	/// Adds a custom property to the recording, so you can sync other (non-input) events as well
	/// </summary>
	/// <param name='propertyName'>
	/// Property name.
	/// </param>
	/// <param name='propertyValue'>
	/// Property value.
	/// </param>
	public void SyncProperty( string propertyName, string propertyValue )
	{
		string propString = "{" + propertyName + "=" + propertyValue + "}";
		if ( !nextPropertiesToRecord.Contains ( propString ) )
			nextPropertiesToRecord.Enqueue ( propString );
	}
	
	public Recording GetRecording()
	{
		return currentRecording;
	}
	
	public int GetCurrentRecordingFrame()
	{
		return currentRecording.frameTimes.Count;
	}
	
	void LateUpdate()
	{	
		if ( _mode == InputVCRMode.Record )
		{
			// record any inputs that should be
			StringBuilder sb = new StringBuilder();
			
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
		while( _currentPlaybackTime < toPlay.recordingLength && currentPlaybackFrame < Mathf.Min ( toPlay.frameProperties.Count, toPlay.frameTimes.Count ) )
		{
			if ( mode == InputVCRMode.Pause )
				yield return 0;
			else
			{
				float nextFrameTime = toPlay.frameTimes[currentPlaybackFrame];			
				
				// allow to catch up to recording if it is further ahead
				if ( _currentPlaybackTime < nextFrameTime - Time.deltaTime * 5 )
				{
					_currentPlaybackTime += Time.deltaTime;
					yield return 0;
					continue;
				}
				
				// if playback is more than about 5 frames ahead, of recording, drop some recording frames
				if ( _currentPlaybackTime > nextFrameTime + Time.deltaTime * 5 )	
				{
					_currentPlaybackTime += Time.deltaTime;
					_currentPlaybackFrame += 3;
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
					// separate properties & input
					Dictionary<string, string> frameProperties = toPlay.GetFrame ( currentPlaybackFrame, watchedInputs );
					
					foreach( KeyValuePair<string, string> property in frameProperties )
					{	
						switch( property.Key )
						{
						case "mousepos":
							mouseIsWatched = true;
							string[] mouse = property.Value.Split( ",".ToCharArray() );
							lastMousePosition.x = float.Parse ( mouse[0] );
							lastMousePosition.y = float.Parse ( mouse[1] );
							break;
						case "position":
							watchedProperties.Add ( "position", property.Value );
							if ( snapToSyncedLocation )
								transform.position = ParseVector3 ( property.Value );
							break;
						case "rotation":
							watchedProperties.Add ( "rotation", property.Value );
							if ( snapToSyncedLocation )
								transform.eulerAngles = ParseVector3 ( property.Value );
							break;
						default:
							watchedProperties.Add ( property.Key, property.Value );
							break;
						}
					}
				}
				catch ( System.Exception e )
				{
					Debug.LogWarning ( e.Message );
					Debug.LogWarning ( currentPlaybackFrame );
					Debug.LogWarning ( toPlay.frameProperties[currentPlaybackFrame] );
					watchedInputs = new Dictionary<string, InputInfo>();
					mouseIsWatched = false;
					
					if ( finishedPlayback != null )
						finishedPlayback( );
					Stop ();
				}
			
				_currentPlaybackTime += Time.deltaTime;
				_currentPlaybackFrame++;
				yield return 0;		
			}
		}
		
		// end of recording
		if ( finishedPlayback != null )
			finishedPlayback( );
		Stop ();
	}
	
	// These methods replace those in Input, so that this object can ignore whether it is record
	#region Input replacements
	public bool GetButton( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( buttonName ) )
			return watchedInputs[buttonName].buttonState;
		else
			return Input.GetButton ( buttonName );
	}
	
	public bool GetButtonDown( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return ( watchedInputs[buttonName].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( buttonName ) || !lastInputs[buttonName].buttonState ) );
		else
			return Input.GetButtonDown ( buttonName );
	}
	
	public bool GetButtonUp( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( buttonName ) )
			return ( !watchedInputs[buttonName].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( buttonName ) || lastInputs[buttonName].buttonState ) );
		else
			return Input.GetButtonUp ( buttonName );
	}
	
	public float GetAxis( string axisName )
	{
		if ( _mode == InputVCRMode.Pause )
			return 0;
		
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey( axisName ) )
			return watchedInputs[axisName].axisValue;
		else
			return Input.GetAxis ( axisName );
	}
	
	public bool GetMouseButton( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( mouseButtonName ) )
			return watchedInputs[mouseButtonName].buttonState;
		else
			return Input.GetMouseButton( buttonNum );
	}
	
	public bool GetMouseButtonDown( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( mouseButtonName ) )
			return ( watchedInputs[ mouseButtonName ].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( mouseButtonName ) || !lastInputs[mouseButtonName].buttonState ) );
		else
			return Input.GetMouseButtonDown( buttonNum );
	}
	
	public bool GetMouseButtonUp( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && watchedInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return ( !watchedInputs[ mouseButtonName ].buttonState && ( lastInputs == null || !lastInputs.ContainsKey ( mouseButtonName ) || lastInputs[mouseButtonName].buttonState ) );
		else
			return Input.GetMouseButtonUp( buttonNum );
	}
	
	public Vector3 mousePosition
	{	
		get {
			if ( _mode == InputVCRMode.Pause )
				return Vector3.zero;
			
			if ( _mode == InputVCRMode.Playback && mouseIsWatched )
				return lastMousePosition;
			else
				return Input.mousePosition;
		}
	}
	
	public string GetProperty( string propertyName )
	{
		string property;
		if ( watchedInputs == null || !watchedProperties.TryGetValue ( propertyName, out property ) )
			return "";
		
		return property;
	}
	#endregion
	
	public static Vector3 ParseVector3( string vectorString )
	{
		string[] splitVecString = vectorString.Split( ",".ToCharArray () );
		float x,y,z;
		if( splitVecString.Length == 3 && float.TryParse ( splitVecString[0], out x ) && float.TryParse ( splitVecString[1], out y ) && float.TryParse ( splitVecString[2], out z ) )
			return new UnityEngine.Vector3( x, y, z );
		
		return Vector3.zero;
	}
}

public enum InputVCRMode
{
	Passthru,	// normal input
	Record,
	Playback,
	Pause
}

public class Recording
{
	public List<float> frameTimes;
	public List<string> frameProperties;
	
	public float recordingLength;
	
	/// <summary>
	/// Copies the data in oldRecoding to a new instance of the <see cref="Recording"/> class.
	/// </summary>
	/// <param name='oldRecording'>
	/// Recording to be copied
	/// </param>
	public Recording( Recording oldRecording )
	{
		frameTimes = new List<float>( oldRecording.frameTimes );
		frameProperties = new List<string>( oldRecording.frameProperties );
		recordingLength = oldRecording.recordingLength;
	}
	
	public Recording( string s )
	{
		frameTimes = new List<float>();
		frameProperties = new List<string>();
		
		if ( s == "" )
			return;
		
		int line = 0;
		int index = 0;
		StringBuilder thisLine;
		
		while ( index < s.Length )
		{
			thisLine = new StringBuilder();
			char c;
			while ( index < s.Length && ( c = s[index++] ) != '\n' && c != '\r' )
			{
				thisLine.Append ( c );
			}
			
			if ( line == 0 )
				recordingLength = float.Parse ( thisLine.ToString () );
			else if ( line % 2 != 0 )
				frameTimes.Add ( float.Parse ( thisLine.ToString () ) );
			else
				frameProperties.Add ( thisLine.ToString () );
			line++;
		}
	}
	
	/// <summary>
	/// Parses the recording. 
	/// Don't use in Unity! Mono has a bug where this is crazy slow with unix line endings
	/// </summary>
	/// <param name='sr'>
	/// String reader
	/// </param>
	void ParseRecording( StringReader sr )
	{
		Debug.Log ( "Parsing recording " + Time.realtimeSinceStartup );
		string currentLine = "";
		try
		{
			currentLine = sr.ReadLine();
			recordingLength = float.Parse( currentLine );
			
			while ( ( currentLine = sr.ReadLine() ) != null )
			{
				frameTimes.Add ( float.Parse ( currentLine ) );
				frameProperties.Add ( sr.ReadLine() );
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
		Debug.Log ( "Finished parsing recording " + Time.realtimeSinceStartup );
	}
	
	public int GetClosestFrame( float toTime )
	{
		int closestFrame = 0;
		while ( closestFrame < frameTimes.Count && frameTimes[closestFrame] < toTime )
			closestFrame++;
		
		return closestFrame;
	}
	
	public Dictionary<string, string> GetFrame( int frameInd, Dictionary<string, InputInfo> inputDict = null )
	{
		Dictionary< string, string> frame = new Dictionary<string, string>();
		string[] properties = frameProperties[frameInd].Split ( "{".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries );
		
		for( int i = 0; i < properties.Length; i++ )
		{
			string propertyString = properties[i].TrimEnd ( "}".ToCharArray() );
			
			int equalInd = propertyString.IndexOf( '=' );
			if ( equalInd == -1 )
				continue;
			
			string recordType = propertyString.Remove ( equalInd );		
			
			if ( recordType == "input" )
			{
				if ( inputDict != null )
				{
					InputInfo newInput = new InputInfo( propertyString.Substring ( equalInd + 1 ) );
					inputDict.Add ( newInput.inputName, newInput );
				}
			}
			else
				frame.Add ( recordType, propertyString.Substring ( equalInd + 1 ) );
		}
		
		return frame;
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
	public float axisValue;	// not raw value
	
	// make blank info object
	public InputInfo()
	{
		inputName = "";
		mouseButtonNum = -1;
		isAxis = false;
		buttonState = false;
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
					buttonState = true;
				else
					buttonState = false;
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
				sb.Append ( ",d" );
			else
				sb.Append ( ",u" );
		}
		return sb.ToString ();
	}
	#endregion
}
