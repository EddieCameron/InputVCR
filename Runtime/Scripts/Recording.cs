/* Recording.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------------------------
 * Class for transferring and parsing InputVCR Recordings. Think of it as a VHS cassete, but fancier
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace InputVCR {
    [Serializable]
    public class Recording : ISerializationCallbackReceiver {
        [SerializeField]
        private List<Frame> frames = new List<Frame>();

        [SerializeField]
        private int _schemaVersion = _CURRENT_JSON_SCHEMA_VERSION;      // make sure schema version is in the JSON
        private const int _CURRENT_JSON_SCHEMA_VERSION = 1;

        public int FrameCount => frames.Count;

        public float Length => frames.Count == 0 ? 0 : frames[frames.Count - 1].time;

        /// <summary>
        /// One recording frame's worth of input
        /// </summary>
        [System.Serializable]
        public struct Frame {
            public float time;
            public List<InputState> inputManagerStates;
            public List<FrameProperty> syncedProperties;

            public Frame( float time ) {
                this.time = time;
                this.inputManagerStates = new List<InputState>();
                this.syncedProperties = new List<FrameProperty>();
            }
        }

        /// <summary>
        /// Represents the state of a single Input source during a frame
        /// </summary>
        [System.Serializable]
        public struct InputState {
            public string inputId;

            public bool buttonState;

            public float axisValue; // not raw value

            public InputState( string inputId, bool buttonState ) {
                this.inputId = inputId;
                this.buttonState = buttonState;
                this.axisValue = 0;
            }

            public InputState( string inputId, float axisValue ) {
                this.inputId = inputId;
                this.buttonState = false;
                this.axisValue = axisValue;
            }
        }

        /// <summary>
        /// The value of an arbitrary property during this frame
        /// Use for adding additional info to the recording
        /// </summary>
        [System.Serializable]
        public struct FrameProperty {
            public string name;
            public string value;

            public FrameProperty( string name, string value ) {
                this.name = name;
                this.value = value;
            }
        }

        #region Constructors
        public Recording() {
        }

        public Recording( string jsonRecording ) {
            JsonUtility.FromJsonOverwrite( jsonRecording, this );
        }

        /// <summary>
        /// Copies the data in oldRecoding to a new instance of the <see cref="Recording"/> class.
        /// </summary>
        /// <param name='oldRecording'>
        /// Recording to be copied
        /// </param>
        public Recording( Recording oldRecording ) {
            if ( oldRecording == null )
                return;

            frames.AddRange( oldRecording.frames );
        }
        #endregion

        public int GetFrameForTime( float time ) {
            if ( FrameCount == 0 )
                throw new InvalidDataException( "Can't get frame for empty recording" );

            for ( int i = 1; i < frames.Count; i++ ) {
                if ( frames[i].time > time )
                    return i - 1;
            }

            return FrameCount - 1;  // freeze on end of recording
        }

        public void ClearFrames( int startFrame = 0 ) {
            if ( startFrame <= 0 )
                frames.Clear();
            else if ( startFrame < frames.Count )
                frames.RemoveRange( startFrame, frames.Count - startFrame );
        }

        /// <summary>
        /// Append a new frame to the recording.
        /// Must be at a time after the last frame
        /// </summary>
        /// <param name="atTime"></param>
        public void AddFrame( float atTime ) {
            if ( FrameCount > 0 ) {
                if ( atTime <= Length )
                    throw new ArgumentException( "Tried to add a frame at a time before the current end of the recording" );
            }

            Frame newFrame = new Frame( atTime );
            frames.Add( newFrame );
        }

        /// <summary>
        /// Adds the supplied input info to given frame.
        /// Overwrites any existing state for the same input at this frame
        /// </summary>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        /// <param name='inputInfo'>
        /// Input info.
        /// </param>
        public void AddInput( int atFrame, InputState inputState ) {
            Frame frame = GetFrame( atFrame );

            for ( int i = 0; i < frame.inputManagerStates.Count; i++ ) {
                // overwrite
                if ( frame.inputManagerStates[i].inputId == inputState.inputId ) {
                    frame.inputManagerStates[i] = inputState;
                    return;
                }
            }

            // new state
            frame.inputManagerStates.Add( inputState );
        }

        /// <summary>
        /// Adds a custom property to a given frame
        /// Overwrites any existing value for the same property name
        /// </summary>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        /// <param name='propertyName'>
        /// Property name.
        /// </param>
        /// <param name='propertyValue'>
        /// Property value as string.
        /// </param>
        public void AddProperty( int atFrame, string propertyName, string propertyValue ) => AddProperty( atFrame, new FrameProperty( propertyName, propertyValue ) );

        /// <summary>
        /// Adds a custom property to a given frame
        /// Overwrites any existing value for the same property name
        /// </summary>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        /// <param name='propertyName'>
        /// Property name.
        /// </param>
        /// <param name='propertyValue'>
        /// Property value as string.
        /// </param>
        public void AddProperty( int atFrame, FrameProperty frameProperty ) {
            Frame frame = GetFrame( atFrame );

            for ( int i = 0; i < frame.syncedProperties.Count; i++ ) {
                // overwrite
                if ( frame.syncedProperties[i].name == frameProperty.name ) {
                    frame.syncedProperties[i] = frameProperty;
                    return;
                }
            }

            frame.syncedProperties.Add( frameProperty );
        }

        /// <summary>
        /// Gets the input state at given frame.
        /// </summary>
        /// <returns>
        /// InputInfo
        /// </returns>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        /// <param name='inputName'>
        /// Input name.
        /// </param>
        public InputState GetInput( int atFrame, string inputName ) {
            Frame frame = GetFrame( atFrame );
            for ( int i = 0; i < frame.inputManagerStates.Count; i++ ) {
                if ( frame.inputManagerStates[i].inputId == inputName )
                    return frame.inputManagerStates[i];
            }

            Debug.LogWarning( "Input " + inputName + " not found in frame " + atFrame );
            return default( InputState );
        }

        /// <summary>
        /// Gets all inputs in a given frame.
        /// </summary>
        /// <returns>
        /// The inputs.
        /// </returns>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        public void GetInputs( int atFrame, List<InputState> outStates ) {
            outStates.AddRange( GetFrame( atFrame ).inputManagerStates );
        }

        /// <summary>
        /// Gets the given custom property if recorded in given frame
        /// </summary>
        /// <returns>
        /// The property.
        /// </returns>
        /// <param name='atFrame'>
        /// At frame.
        /// </param>
        /// <param name='propertyName'>
        /// Property name.
        /// </param>
        public string GetProperty( int atFrame, string propertyName ) {
            Frame frame = GetFrame( atFrame );

            for ( int i = 0; i < frame.syncedProperties.Count; i++ ) {
                if ( frame.syncedProperties[i].name == propertyName )
                    return frame.syncedProperties[i].value;
            }

            Debug.LogWarning( "Property " + propertyName + " not found in frame " + atFrame );
            return "";
        }

        /// <summary>
        /// Get all properties recorded into this frame
        /// </summary>
        /// <param name="atFrame"></param>
        /// <param name="outProps"></param>
        public void GetProperties( int atFrame, List<FrameProperty> outProps ) {
            outProps.AddRange( GetFrame( atFrame ).syncedProperties );
        }

        /// <summary>
        /// Make sure this frame has an entry
        /// </summary>
        /// <param name="frame"></param>
        Frame GetFrame( int frameIdx ) {
            if ( frameIdx < 0 || frameIdx >= frames.Count )
                throw new ArgumentException( "Tried to get frame outside recorded range" );
            return frames[frameIdx];
        }

        public string ToJson( bool prettyPrint = false ) {
            return JsonUtility.ToJson( this, prettyPrint );
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _schemaVersion = _CURRENT_JSON_SCHEMA_VERSION;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if ( _schemaVersion != _CURRENT_JSON_SCHEMA_VERSION ) {
                // TODO we need to ugprade!
            }
        }
    }
}
