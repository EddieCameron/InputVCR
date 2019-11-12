# InputVCR README

## Record and playback player actions without the fuss.

### What

Why write a custom recording class for your game, when much of it is identical? InputVCR records 
inputs you choose to text, then, when you want to replay, will automatically send these inputs to any 
Gameobject, without having to effect the others.
You can use InputVCR to record player motion and any actions they take, for match replays, kill cams, 
puzzles, companions, and more.

### How

#### InputVCRRecorder
InputVCRRecorder is the main class of the package, think of it like a VHS player.

Inputs that should be recorded should be added to the Recorded Buttons/Axes/Keys lists (Buttons and Axes use the names as defined in the Unity Input Manager)
Mouse inputs (position + button states) can also be recorded, if the recordMouseEvents flag is true
Touches coming soon maybe!

To start recording, call Record() or RecordNew()
While recording, input values are saved to a Recording object each frame.
To save additional arbitrary properties to a frame, call SaveProperty, with a key and string value

To stop recording, call Stop() or Rewind()

To start playing back a recording, call Play(), if no recording is provided, the last used recording will be used. Note that recordings will start playing back from their current playback point unless otherwise specified. Be kind, rewind!

Code that needs playback Input values needs to call Input methods via the VCRRecorder rather than Input directly:
e.g: Change `Input.GetButtonDown( "Jump" )` to `myInputVCRRecorder.GetButtonDown( "Jump" )`. This function will return the live Input values when stopped or recording, but will return values as saved in the recording while in playback
Saved property values can be retrieved with TryGetProperty

#### Recording
This is the VHS tape. It holds the recorded input values for each frame for the length of the recording.
You shouldn't need to call any methods on this directly, although the Length property (in seconds) might be useful.
This object can be passed around to be played back in other InputVCRRecorders, or saved for later. 
If you want to save a Recording when playback ends, it must first be Serialized so it can be saved to disk. Call .ToJson() to get a json string.
The InputVCRRecorder class also provides utility methods to save/load the current recording to and from disk

#### InputVCRTextRecordingLoader
This is a helper class to automatically load and optionally start playing a Recording saved in Json form.

#### InputVCRTransformSyncer
Helper class to automatically save the transform state of an object to a recording, and to sync it up. This is to prevent "drift" of objects due to different frame times or physics.


See the Examples folder for a demo, and check the code for more detailed documentation

Easy!
 
### Info
More information and tools at eddiecameron.fun, @eddiecameron, or support@grapefruitgames.com
