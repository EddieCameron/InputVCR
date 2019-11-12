/* CharacterController2D.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputVCR;

namespace InputVCRExamples {
    public class CharacterController2D : MonoBehaviour {
        public InputVCRRecorder recorder;

        public float maxSpeed;
        public float damping;

        Vector2 velocity;
        void Update() {
            // Get input from recorder
            // Note this only doesn't use InputManager Axes/Button names because your project might have changed the default settings
            Vector2 inputDirection = Vector2.zero;
            if ( recorder.GetKey( KeyCode.W ) )
                inputDirection.y += 1f;
            if ( recorder.GetKey( KeyCode.S ) )
                inputDirection.y -= 1f;
            if ( recorder.GetKey( KeyCode.D ) )
                inputDirection.x += 1f;
            if ( recorder.GetKey( KeyCode.A ) )
                inputDirection.x -= 1f;

            if ( inputDirection.sqrMagnitude > 1 )
                inputDirection.Normalize();

            velocity = Vector2.Lerp( velocity, inputDirection * maxSpeed, damping * Time.deltaTime );
            transform.localPosition = transform.localPosition + (Vector3)velocity * Time.deltaTime;
        }
    }
}
