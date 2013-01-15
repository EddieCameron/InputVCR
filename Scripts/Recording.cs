/* Recording.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 * Class for transferring and parsing InputVCR Recordings. Think of it as a VHS cassete, but fancier
 * 
 */ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LitJson;

public class Recording
{
	public int frameRate;
	public List<RecordingFrame> frames = new List<RecordingFrame>();
	
	public int totalFrames{ get{ return frames.Count; } }
	public float recordingLength{ get{ return totalFrames / frameRate; } }
	
	public Recording()
	{
		this.frameRate = 60;
		frames = new List<RecordingFrame>();
	}
	
	public Recording( int frameRate )
	{
		this.frameRate = Mathf.Max ( 1, frameRate );
		frames = new List<RecordingFrame>();
	}
	
	/// <summary>
	/// Copies the data in oldRecoding to a new instance of the <see cref="Recording"/> class.
	/// </summary>
	/// <param name='oldRecording'>
	/// Recording to be copied
	/// </param>
	public Recording( Recording oldRecording )
	{
		frameRate = oldRecording.frameRate;
		frames = new List<RecordingFrame>( oldRecording.frames );
	}
	
	/// <summary>
	/// Parses a Recording from saved JSON string
	/// </summary>
	/// <returns>
	/// The recording.
	/// </returns>
	/// <param name='jsonRecording'>
	/// JSON recording
	/// </param>
	public static Recording ParseRecording( string jsonRecording )
	{
		ImporterFunc<double, float> importer = delegate( double input) {
			return (float)input;
		};
		JsonMapper.RegisterImporter<double, float>( importer );	// let float values be parsed
		
		Recording rec = JsonMapper.ToObject<Recording>( jsonRecording );
		
		JsonMapper.UnregisterImporters ();
		
		return rec;
	}
	
	/// <summary>
	/// Gets the closest frame index to a provided time
	/// </summary>
	/// <returns>
	/// The closest frame.
	/// </returns>
	/// <param name='toTime'>
	/// To time.
	/// </param>/
	public int GetClosestFrame( float toTime )
	{
		return (int)( toTime * frameRate );
	}
	
	/// <summary>
	/// Adds the supplied input info to given frame
	/// </summary>
	/// <param name='atFrame'>
	/// At frame.
	/// </param>
	/// <param name='inputInfo'>
	/// Input info.
	/// </param>
	public void AddInput( int atFrame, InputInfo inputInfo )
	{
		CheckFrame ( atFrame );
		
		for( int i = 0; i < frames[atFrame].inputs.Count; i++ )
		{
			// no duplicate properties
			if ( frames[atFrame].inputs[i].inputName == inputInfo.inputName )
			{
				frames[atFrame].inputs[i] = new InputInfo( inputInfo );
				return;
			}
		}
		
		frames[atFrame].inputs.Add ( new InputInfo( inputInfo ) );
	}
	
	/// <summary>
	/// Adds a custom property to a given frame
	/// </summary>
	/// <param name='atFrame'>
	/// At frame.
	/// </param>
	/// <param name='propertyInfo'>
	/// Property info.
	/// </param>
	public void AddProperty( int atFrame, FrameProperty propertyInfo )
	{
		CheckFrame( atFrame );
		
		for( int i = 0; i < frames[atFrame].syncedProperties.Count; i++ )
		{
			// no duplicate properties
			if ( frames[atFrame].syncedProperties[i].name == propertyInfo.name )
			{
				frames[atFrame].syncedProperties[i] = propertyInfo;
				return;
			}
		}
		
		frames[atFrame].syncedProperties.Add ( propertyInfo );
	}
	
	/// <summary>
	/// Gets the given input at given frame.
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
	public InputInfo GetInput( int atFrame, string inputName )
	{
		if ( atFrame < 0 || atFrame >= frames.Count )
		{
			Debug.LogWarning ( "Frame " + atFrame + " out of bounds" );
			return null;
		}
		else
		{
			// iterating to find. Could avoid repeat access time with pre-processing, but would be a waste of memory/GC slowdown? & list is small anyway
			foreach( InputInfo input in frames[atFrame].inputs )
				if ( input.inputName == inputName )
					return input;
		}
		
		Debug.LogWarning ( "Input " + inputName + " not found in frame " + atFrame );
		return null;
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
	public InputInfo[] GetInputs( int atFrame )
	{
		if ( atFrame < 0 || atFrame >= frames.Count )
		{
			Debug.LogWarning ( "Frame " + atFrame + " out of bounds" );
			return null;
		}
		else
			return frames[atFrame].inputs.ToArray ();
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
	public string GetProperty( int atFrame, string propertyName )
	{
		if ( atFrame < 0 || atFrame >= frames.Count )
		{
			Debug.LogWarning ( "Frame " + atFrame + " out of bounds" );
			return null;
		}
		else
		{
			// iterating to find. Could avoid repeat access time with pre-processing, but would be a waste of memory/GC slowdown? & list is small anyway
			foreach( FrameProperty prop in frames[atFrame].syncedProperties )
				if ( prop.name == propertyName )
					return prop.property;
		}
		
		return null;
	}
	
	// Make sure this frame has an entry
	void CheckFrame( int frame )
	{
		while ( frame >= frames.Count )
			frames.Add ( new RecordingFrame() );
	}
	
	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="Recording"/> using JSON
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/> that represents the current <see cref="Recording"/>.
	/// </returns>
	public override string ToString()
	{
		StringBuilder jsonB = new StringBuilder();
		JsonWriter writer = new JsonWriter( jsonB );
		
		writer.WriteObjectStart();
		//{
			writer.WritePropertyName ( "frameRate" );
			writer.Write ( frameRate );
		
			writer.WritePropertyName ( "frames" );
			writer.WriteArrayStart();
			//[
			foreach( RecordingFrame frame in frames )
			{
				writer.WriteObjectStart();
				//{
					writer.WritePropertyName ( "recordTime" );
					writer.Write ( (double)frame.recordTime );
					
					writer.WritePropertyName ( "inputs" );
					writer.WriteArrayStart();
					//[
					foreach( InputInfo input in frame.inputs )
					{
						writer.WriteObjectStart();
						//{
						writer.WritePropertyName ( "inputName" );
						writer.Write ( input.inputName );
						
						writer.WritePropertyName ( "isAxis" );
						writer.Write ( input.isAxis );
						
						writer.WritePropertyName ( "mouseButtonNum" );
						writer.Write ( input.mouseButtonNum );
						
						writer.WritePropertyName ( "buttonState" );
						writer.Write ( input.buttonState );
						
						writer.WritePropertyName ( "axisValue" );
						writer.Write ( input.axisValue );
						//}
						writer.WriteObjectEnd();
					}
					//]
					writer.WriteArrayEnd ();
					
					writer.WritePropertyName ( "syncedProperties" );
					writer.WriteArrayStart ();
					//[
					foreach( FrameProperty prop in frame.syncedProperties )
					{
						writer.WriteObjectStart();
						//{
						writer.WritePropertyName ( "name" );
						writer.Write ( prop.name );
						
						writer.WritePropertyName ( "property" );
						writer.Write ( prop.property );
						//}
						writer.WriteObjectEnd ();
					}
					//]
					writer.WriteArrayEnd ();
				//}
				writer.WriteObjectEnd ();
			}
			//]
			writer.WriteArrayEnd ();
		//}
		writer.WriteObjectEnd();
		
		return jsonB.ToString();
	}
}

public class RecordingFrame
{
	public float recordTime;
		
	public List<InputInfo> inputs = new List<InputInfo>();
	public List<FrameProperty> syncedProperties = new List<FrameProperty>();
}

[System.Serializable]
public class InputInfo	// represents state of certain input in one frame. Has to be class for inspector to serialize
{
	public string inputName;	// from InputManager
	public bool isAxis;
	
	[HideInInspector]
	public int mouseButtonNum = -1; // only positive if is mouse button
	
	[HideInInspector]
	public bool buttonState;
	
	[HideInInspector]
	public float axisValue;	// not raw value
	
	public InputInfo()
	{
		inputName = "";
		mouseButtonNum = -1;
		isAxis = false;
		buttonState = false;
		axisValue = 0f;
	}
	
	public InputInfo( InputInfo toCopy )
	{
		inputName = toCopy.inputName;
		isAxis = toCopy.isAxis;
		
		mouseButtonNum = toCopy.mouseButtonNum;
		
		buttonState = toCopy.buttonState;
		axisValue = toCopy.axisValue;
	}
	
	public override bool Equals (object obj)
	{
		InputInfo other = obj as InputInfo;
		return Equals ( other );
	}
	
	public bool Equals( InputInfo other )
	{
		if ( other == null )
			return false;
		
		if ( inputName != other.inputName || isAxis != other.isAxis || mouseButtonNum != other.mouseButtonNum )
			return false;
		
		if ( isAxis )
			return axisValue == other.axisValue;
		else
			return buttonState == other.buttonState;
	}
	
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}

public struct FrameProperty
{
	public string name;
	public string property;
	
	public FrameProperty( string name, string property )
	{
		this.name = name;
		this.property = property;
	}
}