/* InputVCRRecorderEditor.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InputVCR;
using UnityEngine.UIElements;
using System;
using System.IO;

namespace InputVCREditor {
    [CustomEditor( typeof( InputVCRRecorder ) )]
    public class InputVCRRecorderEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var recorder = (InputVCRRecorder)target;
            if ( EditorApplication.isPlaying ) {
                InputVCRMode recordMode = recorder.Mode;

                // record controls
                EditorGUILayout.LabelField( "Controls", EditorStyles.boldLabel );

                // controls
                using ( var playGroup = new EditorGUILayout.HorizontalScope() ) {
                    bool playEnabled = recordMode != InputVCRMode.Playback || recorder.IsPaused;
                    GUI.enabled = playEnabled;
                    if ( GUILayout.Button( "PLAY >", EditorStyles.miniButtonLeft ) )
                        recorder.Play();

                    bool pauseEnabled = recordMode != InputVCRMode.Passthru && !recorder.IsPaused;
                    GUI.enabled = pauseEnabled;
                    if ( GUILayout.Button( "PAUSE ||", EditorStyles.miniButtonMid ) ) {
                        recorder.Pause();
                    }
                    GUI.enabled = true;
                    if ( GUILayout.Button( "REWIND <<", EditorStyles.miniButtonRight ) ) {
                        recorder.RewindToStart();
                    }
                }

                using ( var recordGroup = new EditorGUILayout.HorizontalScope() ) {
                    bool recordEnabled = recordMode != InputVCRMode.Record;
                    GUI.enabled = recordEnabled;
                    if ( GUILayout.Button( "RECORD O", EditorStyles.miniButtonLeft ) )
                        recorder.Record();


                    bool stopEnabled = recordMode != InputVCRMode.Passthru;
                    GUI.enabled = stopEnabled;
                    if ( GUILayout.Button( "STOP []", EditorStyles.miniButtonRight ) )
                        recorder.Stop();
                }
                GUI.enabled = true;

                var currentRecording = recorder.CurrentRecording;
                if ( recorder.Mode == InputVCRMode.Record ) {
                    EditorGUILayout.LabelField( "Recording", EditorStyles.boldLabel );
                    EditorGUILayout.LabelField( "Length: " + recorder.CurrentPlaybackTime.ToString( "f2" ) );
                }
                else {
                    if ( recorder.Mode == InputVCRMode.Playback )
                        EditorGUILayout.LabelField( "Playing", EditorStyles.boldLabel );
                    else
                        EditorGUILayout.LabelField( "Stopped", EditorStyles.boldLabel );

                    float time = recorder.CurrentPlaybackTime;
                    float length = currentRecording.Length;
                    EditorGUILayout.LabelField( "Length: " + length.ToString( "f2" ) );

                    GUI.enabled = false;
                    EditorGUILayout.Slider( time, 0, length, GUILayout.ExpandWidth( true ) );
                    GUI.enabled = true;
                }

                // recording save
                if ( currentRecording != null ) {
                    if ( GUILayout.Button( "Save Recording" ) ) {
                        string recordingName = $"VCRRecord_{DateTime.Now:yy-MM-dd_HHmmss}";
                        string path = EditorUtility.SaveFilePanelInProject( "Save Recording", recordingName, "txt", "Save current recording to disk as JSON" );
                        if ( !string.IsNullOrEmpty( path ) ) {
                            string json = currentRecording.ToJson();
                            try {
                                File.WriteAllText( path, json );
                                AssetDatabase.Refresh();
                            }
                            catch ( Exception e ) {
                                Exception error = new Exception( "Failed to write recording to disk", e );
                                Debug.LogException( e );
                            }
                        }
                    }
                }
            }

            // Recording load
            if ( GUILayout.Button( "Load Recording" ) ) {
                string jsonPath = EditorUtility.OpenFilePanel( "Load Recording", Application.dataPath, "txt" );
                if ( !string.IsNullOrEmpty( jsonPath ) ) {
                    try {
                        string recordJson = File.ReadAllText( jsonPath );
                        Recording r = new Recording( recordJson );
                        recorder.LoadRecording( r );
                    }
                    catch ( Exception e ) {
                        Exception error = new Exception( "Failed to load recording from disk", e );
                        Debug.LogException( e );
                    }
                }
            }
        }
    }
}
