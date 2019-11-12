/* VCRControls.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------
 */
using System.Collections;
using System.Collections.Generic;
using InputVCR;
using UnityEngine;
using UnityEngine.UI;

namespace InputVCRExamples {
    public class VCRControls : MonoBehaviour {

        public Button recordButton, playButton, pauseButton, rewindButton;
        public Text timeText;

        public InputVCRRecorder vcr;

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            bool recordEnabled = vcr.Mode != InputVCRMode.Record;
            recordButton.interactable = recordEnabled;

            bool playEnabled = vcr.Mode == InputVCRMode.Passthru || vcr.IsPaused;
            playButton.gameObject.SetActive( playEnabled );
            pauseButton.gameObject.SetActive( !playEnabled );

            if ( vcr.CurrentRecording == null )
                timeText.text = "no recording";
            else
                timeText.text = $"{vcr.CurrentPlaybackTime:f2}/{vcr.CurrentRecording.Length:f2}";
        }
    }
}
