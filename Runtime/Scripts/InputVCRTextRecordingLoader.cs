/* InputVCRTextRecordingLoader.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputVCR {
    /// <summary>
    /// Helper to load and play VCR Recordings from text assets
    /// </summary>
    [RequireComponent( typeof( InputVCRRecorder ) )]
    public class InputVCRTextRecordingLoader : MonoBehaviour {
        private InputVCRRecorder _recorder;

        public TextAsset loadRecordingOnStart;
        public bool playRecordingOnStart;

        void Awake() {
        }

        void Start() {
            _recorder = GetComponent<InputVCRRecorder>();

            if ( loadRecordingOnStart != null ) {
                Recording recording = new Recording( loadRecordingOnStart.text );
                _recorder.LoadRecording( recording );

                if ( playRecordingOnStart )
                    _recorder.Play();
            }
        }
    }
}
