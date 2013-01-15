/* PlayButton.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 */ 

using UnityEngine;
using System.Collections;

public class PlayButton : MonoBehaviour 
{
	public InputVCR playbackCharacterPrefab;
	public InputVCR playerVCR;
	
	public RecordButton recordButton;
	
	public Texture pauseTex;
	
	private bool isRecording;
	private Vector3 recordingStartPos;
	private Quaternion recordingStartRot;
	
	private bool isPlaying;
	private InputVCR curPlayer;
	
	public GUIText instructions;
	
	void Awake()
	{
		Destroy( instructions, 5f );
	}
	
	public void StartRecording()
	{
		if ( isRecording )
			playerVCR.Stop ();
		else
		{
			recordingStartPos = playerVCR.transform.position;
			recordingStartRot = playerVCR.transform.rotation;
			playerVCR.NewRecording ();
		}
		
		isRecording = !isRecording;
	}
	
	void Update()
	{
		if ( Input.GetKeyDown ( KeyCode.P ) )
			StartPlay ();
	}
	
	void OnMouseDown()
	{
		StartPlay ();
	}
	
	private void StartPlay()	
	{
		if ( isPlaying )
		{
			// pause
			curPlayer.Pause();
			isPlaying = false;
			SwapTex();
		}
		else if ( curPlayer != null )
		{
			// unpause
			curPlayer.Play ();
			SwapTex ();
			isPlaying = true;
		}
		else
		{
			// try to start new playback
			if ( isRecording )
				recordButton.Record ();
			
			StartCoroutine ( Player () );
		}
	}
	
	private IEnumerator Player()
	{
		Recording recording = playerVCR.GetRecording ();
		if ( recording == null )
			yield break;
		
		Debug.Log ( recording.ToString () );
		curPlayer = (InputVCR)Instantiate ( playbackCharacterPrefab, recordingStartPos, recordingStartRot );
		curPlayer.Play ( Recording.ParseRecording( recording.ToString() ) );
		SwapTex ();
		
		float playTime = recording.recordingLength;
		float curTime = 0f;
		
		isPlaying = true;
		while ( curTime < playTime )
		{
			if ( isPlaying )
				curTime += Time.deltaTime;
			
			yield return 0;
		}
		
		// Play finished
		isPlaying = false;
		Destroy ( curPlayer.gameObject );
		curPlayer = null;
		SwapTex ();
	}
	
	private void SwapTex()
	{
		Texture playTex = guiTexture.texture;
		guiTexture.texture = pauseTex;
		pauseTex = playTex;
	}
}
