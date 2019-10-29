/* InputVCR.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
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
using System;

namespace InputVCR {
    public class InputVCR : MonoBehaviour {
        #region Inspector properties
        [Tooltip( "Button names (from Input manager) that should be recorded" )]
        [SerializeField]
        List<string> _recordedButtons = new List<string>();

        [Tooltip( "Axis names (from Input manager) that should be recorded" )]
        [SerializeField]
        List<string> _recordedAxes = new List<string>();

        [Tooltip( "Key buttons that should be recorded" )]
        [SerializeField]
        List<KeyCode> _recordedKeys = new List<KeyCode>();

        [Tooltip( "Whether mouse position/button states should be recorded each frame (mouse axes are separate from this)" )]
        public bool recordMouseEvents;

        [Tooltip( "Whether touch positions should be recorded each frame" )]
        public bool recordTouchEvents;

        [SerializeField]
        private InputVCRMode _mode = InputVCRMode.Passthru; // initial mode that vcr is operating in
        /// <summary>
        /// Is this VCR:
        /// - Recording: Taking live input and writing to a Recording and passing it on to callers
        /// - Playback: Replaying input from a previous Recording
        /// - Passthrough: Taking live input and passing on to callers
        /// </summary>
        public InputVCRMode Mode => _mode;

        /// <summary>
        /// Whether current playback or recording is paused. If there is no current recording or playback, Input is not affected
        /// </summary>
        /// <value></value>
        public bool IsPaused { get; set; }
        #endregion

        RecordingState currentRecord;     // the recording currently in the VCR. Copy or ToString() this to save.
        public Record CurrentRecord => currentRecord == null ? null : currentRecord.targetRecording;

        public float CurrentPlaybackTime => currentRecord == null ? 0 : currentRecord.Time;


        Queue<Record.FrameProperty> nextPropertiesToRecord = new Queue<Record.FrameProperty>();   // if SyncLocation or SyncProperty are called, this will hold their results until the recordstring is next written to

        Dictionary<string, Record.InputState> lastFrameInputs = new Dictionary<string, Record.InputState>();    // list of inputs from last frame (for seeing what buttons have changed state)
        Dictionary<string, Record.InputState> thisFrameInputs = new Dictionary<string, Record.InputState>();
        Dictionary<string, Record.FrameProperty> thisFrameProperties = new Dictionary<string, Record.FrameProperty>();

        public event System.Action finishedPlayback;    // sent when playback finishes

        /// <summary>
        /// Start recording. Will append to current Record, wiping all data after current playback time
        /// </summary>
        public void Record() {
            Record( forceNewRecording: false );
        }

        /// <summary>
        /// Start recording, any current Record will be wiped and a new recording begun
        /// </summary>
        public void RecordNew() {
            Record( forceNewRecording: true );
        }

        void Record( bool forceNewRecording ) {
            if ( forceNewRecording || currentRecord != null ) {
                currentRecord = new RecordingState( new Record() );
                nextPropertiesToRecord.Clear();
            }
            else {
                currentRecord.targetRecording.ClearFrames( currentRecord.FrameIdx + 1 );
            }

            _mode = InputVCRMode.Record;
            IsPaused = false;
            ClearInput();
        }

        /// <summary>
        /// Start or resume playing back the current Record, if present.
        /// </summary>
        public void Play() {
            if ( _mode != InputVCRMode.Playback ) {
                ClearInput(); // dont' clear if just resuming playback
                _mode = InputVCRMode.Playback;
            }
            IsPaused = false;
        }

        /// <summary>
        /// Play the specified Record, from optional specified time
        /// </summary>
        /// <param name='record'>
        /// Recording.
        /// </param>
        /// <param name='startPlaybackFromTime'>
        /// </param>
        public void PlayNew( Record record, float startPlaybackFromTime = 0 ) {
            currentRecord = new RecordingState( record );
            currentRecord.SkipToTime( startPlaybackFromTime );

            ClearInput();

            _mode = InputVCRMode.Playback;
        }

        /// <summary>
        /// Pause recording or playback
        /// </summary>
        public void Pause() {
            IsPaused = true;
        }

        /// <summary>
        /// Stop recording or playback. Live input will be passed through
        /// </summary>
        public void RevertToLive() {
            _mode = InputVCRMode.Passthru;
            ClearInput();
        }

        /// <summary>
        /// Adds a custom property to the recording, so you can sync other (non-input) events as well.
        /// eg: doors opening, enemy spawning, etc
        /// </summary>
        /// <param name='propertyName'>
        /// Property name.
        /// </param>
        /// <param name='propertyValue'>
        /// Property value.
        /// </param>
        public void SaveProperty( string propertyName, string propertyValue ) {
            // duplicates dealt with when recorded
            var frameProp = new Record.FrameProperty( propertyName, propertyValue );
            if ( !nextPropertiesToRecord.Contains( frameProp ) )
                nextPropertiesToRecord.Enqueue( frameProp );
        }

        #region Recording & Playback
        void Update() {
            if ( IsPaused )
                return;

            if ( _mode == InputVCRMode.Playback ) {
                AdvancePlayback( Time.deltaTime );
            }
            else if ( _mode == InputVCRMode.Record ) {
                RecordCurrentFrame();
            }
        }

        List<Record.InputState> _inputStateCache = new List<Record.InputState>();
        List<Record.FrameProperty> _propCache = new List<Record.FrameProperty>();
        void AdvancePlayback( float delta ) {
            int lastFrameIdx = currentRecord.FrameIdx;
            currentRecord.AdvanceByTime( delta );
            if ( currentRecord.Time > currentRecord.targetRecording.Length ) {
                // reached end of recording
                finishedPlayback?.Invoke();
                RevertToLive();
                return;
            }

            if ( currentRecord.FrameIdx == lastFrameIdx )
                return; // no new playback data TODO clear deltas if saved

            // duplicate this input to previous frame
            lastFrameInputs.Clear();
            foreach (var input in thisFrameInputs)
            {
                lastFrameInputs[input.Key] = input.Value;
            }

            // update inputs with changes from each playback frame since last real frame
            for ( int checkFrame = lastFrameIdx + 1; checkFrame <= currentRecord.FrameIdx; checkFrame++ ) {
                // Inputs
                _inputStateCache.Clear();
                currentRecord.targetRecording.GetInputs( checkFrame, _inputStateCache );
                foreach ( var input in _inputStateCache ) {
                    // if a button was pressed and released within a single playback frame, key up/down flags could be missed
                    // TODO fix by either using events to signal key up/downs (not ideal) or buffering up/downs over subsequent frames (less ideal)
                    thisFrameInputs[input.inputId] = input;
                }

                // Properties
                _propCache.Clear();
                currentRecord.targetRecording.GetProperties( checkFrame, _propCache );
                foreach ( var prop in _propCache ) {
                    thisFrameProperties[prop.name] = prop;
                }
            }
        }

        void RecordCurrentFrame() {
            currentRecord.AppendNewRecordingFrame( Time.deltaTime );

            // mouse
            if ( recordMouseEvents ) {
                currentRecord.AddInputToCurrentFrame( new Record.InputState( GetMouseButtonId( 0 ), Input.GetMouseButton( 0 ) ) );
                currentRecord.AddInputToCurrentFrame( new Record.InputState( GetMouseButtonId( 1 ), Input.GetMouseButton( 1 ) ) );
                currentRecord.AddInputToCurrentFrame( new Record.InputState( GetMouseButtonId( 2 ), Input.GetMouseButton( 2 ) ) );
                currentRecord.AddInputToCurrentFrame( new Record.InputState( _MOUSE_POS_X_ID, Input.mousePosition.x ) );
                currentRecord.AddInputToCurrentFrame( new Record.InputState( _MOUSE_POS_Y_ID, Input.mousePosition.y ) );
            }

            // buttons
            foreach ( var buttonName in _recordedButtons ) {
                currentRecord.AddInputToCurrentFrame( new Record.InputState( buttonName, Input.GetButton( buttonName ) ) );
            }

            // axes
            foreach ( var axisName in _recordedAxes ) {
                currentRecord.AddInputToCurrentFrame( new Record.InputState( axisName, Input.GetAxis( axisName ) ) );
            }

            // keys
            foreach ( var keyCode in _recordedKeys ) {
                currentRecord.AddInputToCurrentFrame( new Record.InputState( keyCode.ToString(), Input.GetKey( keyCode ) ) );
            }

            // properties
            while ( nextPropertiesToRecord.Count > 0 ) {
                currentRecord.AddPropertyToCurrentFrame( nextPropertiesToRecord.Dequeue() );
            }
        }


        #endregion
        /// <summary>
        /// Wipe current playback data
        /// </summary>
        void ClearInput() {
            thisFrameInputs.Clear();
            lastFrameInputs.Clear();
            thisFrameProperties.Clear();
        }

        // These methods replace those in Input, so that this object can ignore whether it is record
        #region Input replacements
        public bool GetKey( KeyCode key ) => GetKey( key.ToString() );

        public bool GetKey( string keyName ) {
            if ( _mode == InputVCRMode.Playback ) {
                return thisFrameInputs.TryGetValue( keyName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
            }

            return Input.GetKey( keyName );
        }

        public bool GetKeyDown( KeyCode key ) => GetKeyDown( key.ToString() );
        public bool GetKeyDown( string keyName ) {
            if ( _mode == InputVCRMode.Playback ) {
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( keyName, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( keyName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return thisFrameButtonDown && !lastFrameButtonDown;
            }

            return Input.GetKeyDown( keyName );
        }

        public bool GetKeyUp( KeyCode key ) => GetKeyUp( key.ToString() );
        public bool GetKeyUp( string keyName ) {
            if ( _mode == InputVCRMode.Playback ) {
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( keyName, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( keyName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return !thisFrameButtonDown && lastFrameButtonDown;
            }

            return Input.GetKeyUp( keyName );
        }

        public bool GetButton( string buttonName ) {
            if ( _mode == InputVCRMode.Playback ) {
                return thisFrameInputs.TryGetValue( buttonName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
            }

            return Input.GetButton( buttonName );
        }

        public bool GetButtonDown( string buttonName ) {
            if ( _mode == InputVCRMode.Playback ) {
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( buttonName, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( buttonName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return thisFrameButtonDown && !lastFrameButtonDown;
            }

            return Input.GetKeyDown( buttonName );
        }

        public bool GetButtonUp( string buttonName ) {
            if ( _mode == InputVCRMode.Playback ) {
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( buttonName, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( buttonName, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return !thisFrameButtonDown && lastFrameButtonDown;
            }

            return Input.GetKeyUp( buttonName );
        }

        public float GetAxis( string axisName ) {
            if ( _mode == InputVCRMode.Playback ) {
                if ( thisFrameInputs.TryGetValue( axisName, out Record.InputState thisFrameState ) )
                    return thisFrameState.axisValue;
                else
                    return 0;
            }

            return Input.GetAxis( axisName );
        }

        public bool GetMouseButton( int buttonNum ) {
            if ( _mode == InputVCRMode.Playback ) {
                return thisFrameInputs.TryGetValue( GetMouseButtonId( buttonNum ), out Record.InputState thisFrameState ) && thisFrameState.buttonState;
            }

            return Input.GetMouseButton( buttonNum );
        }

        public bool GetMouseButtonDown( int buttonNum ) {
            if ( _mode == InputVCRMode.Playback ) {
                string buttonId = GetMouseButtonId( buttonNum );
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( buttonId, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( buttonId, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return thisFrameButtonDown && !lastFrameButtonDown;
            }

            return Input.GetMouseButtonDown( buttonNum );
        }

        public bool GetMouseButtonUp( int buttonNum ) {
            if ( _mode == InputVCRMode.Playback ) {
                string buttonId = GetMouseButtonId( buttonNum );
                bool lastFrameButtonDown = lastFrameInputs.TryGetValue( buttonId, out Record.InputState lastFrameState ) && lastFrameState.buttonState;
                bool thisFrameButtonDown = thisFrameInputs.TryGetValue( buttonId, out Record.InputState thisFrameState ) && thisFrameState.buttonState;
                return !thisFrameButtonDown && lastFrameButtonDown;
            }

            return Input.GetMouseButtonUp( buttonNum );
        }

        public Vector3 mousePosition {
            get {
                if ( _mode == InputVCRMode.Playback ) {
                    float mouseX = thisFrameInputs.TryGetValue( _MOUSE_POS_X_ID, out Record.InputState thisFrameXState ) ? thisFrameXState.axisValue : 0;
                    float mouseY = thisFrameInputs.TryGetValue( _MOUSE_POS_Y_ID, out Record.InputState thisFrameYState ) ? thisFrameYState.axisValue : 0;
                    return new Vector3( mouseX, mouseY, 0 );
                }

                return Input.mousePosition;
            }
        }

        public string GetProperty( string propertyName ) {
            if ( thisFrameProperties.TryGetValue( propertyName, out Record.FrameProperty frameProp ) )
                return frameProp.value;
            else
                return string.Empty;
        }
        #endregion

        private const string _MOUSE_POS_X_ID = "MOUSE_POSITION_X";
        private const string _MOUSE_POS_Y_ID = "MOUSE_POSITION_Y";
        private const string _MOUSE_BUTTON_ID = "MOUSE_BUTTON_";

        private string GetMouseButtonId( int buttonNumber ) {
            return _MOUSE_BUTTON_ID + buttonNumber;
        }

        /// <summary>
        /// Holds playback/record state info for a Recording
        /// </summary>
        class RecordingState {
            public readonly Record targetRecording;
            public float Time { get; private set; }
            public int FrameIdx { get; private set; }

            public RecordingState( Record recording ) {
                targetRecording = recording;
            }

            public void SkipToTime( float time ) {
                this.Time = time;
                this.FrameIdx = targetRecording.GetFrameForTime( time );
            }

            public void AdvanceByTime( float deltaTime ) {
                this.Time += deltaTime;
                this.FrameIdx = targetRecording.GetFrameForTime( Time );
            }

            public void AppendNewRecordingFrame( float deltaTime ) {
                this.Time += deltaTime;
                this.FrameIdx++;
                targetRecording.AddFrame( this.Time );
            }

            public void AddInputToCurrentFrame( Record.InputState inputState ) {
                targetRecording.AddInput( this.FrameIdx, inputState );
            }

            public void AddPropertyToCurrentFrame( Record.FrameProperty frameProperty ) {
                targetRecording.AddProperty( this.FrameIdx, frameProperty );
            }
        }
    }

    public enum InputVCRMode {
        Passthru,   // normal input
        Record,     // normal input & record it
        Playback,   // all input comes from recording
    }
}
