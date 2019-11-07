using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InputVCR;
using UnityEngine.UIElements;

namespace InputVCREditor {
    [CustomEditor( typeof( InputVCRRecorder ) )]
    public class InputVCRRecorderEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if ( EditorApplication.isPlayingOrWillChangePlaymode ) {
                var recorder = (InputVCRRecorder)target;
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
                        recorder.RevertToPassthrough();
                }
                GUI.enabled = true;

                if ( recorder.Mode == InputVCRMode.Record ) {
                    EditorGUILayout.LabelField( "Recording", EditorStyles.boldLabel );
                    EditorGUILayout.LabelField( "Length: " + recorder.CurrentPlaybackTime.ToString( "f2" ) );
                }
                else {
                    if ( recorder.Mode == InputVCRMode.Playback )
                        EditorGUILayout.LabelField( "Playing", EditorStyles.boldLabel );
                    else
                        EditorGUILayout.LabelField( "Stopped", EditorStyles.boldLabel );

                    var currentRecord = recorder.CurrentRecord;
                    if ( currentRecord == null ) {
                        EditorGUILayout.LabelField( "No record in player" );
                    }
                    else {
                        float time = recorder.CurrentPlaybackTime;
                        float length = currentRecord.Length;
                        EditorGUILayout.LabelField( "Length: " + length.ToString( "f2" ) );

                        GUI.enabled = false;
                        EditorGUILayout.Slider( time, 0, length, GUILayout.ExpandWidth( true ) );
                        GUI.enabled = true;
                    }
                }
            }
        }
    }
}
