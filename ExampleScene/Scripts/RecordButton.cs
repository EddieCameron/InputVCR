/* RecordButton.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 */ 

using UnityEngine;
using System.Collections;

public class RecordButton : MonoBehaviour 
{
	public PlayButton playButton;
	public Texture stopTex;
	
	void Update()
	{
		if ( Input.GetKeyDown ( KeyCode.R ) )
			Record ();
	}
	
	void OnMouseDown()
	{
		Record ();
	}
	
	public void Record()
	{
		playButton.StartRecording();
		Texture curTex = guiTexture.texture;
		guiTexture.texture = stopTex;
		stopTex = curTex;
	}
}
