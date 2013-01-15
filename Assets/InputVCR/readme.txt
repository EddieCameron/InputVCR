InputVCR README

----------

Record and playback player actions without the fuss.

----------

Licence:

The InputVCR.cs & Recording.cs scripts are open source under the MIT licence. Do what you will with them! 
LitJson is in the public domain

Copyright (C) 2013 Eddie Cameron

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

----------

Why write a custom recording class for your game, when much of it is identical? InputVCR records 
inputs you choose to text, then, when you want to replay, will automatically send these inputs to any 
Gameobject, without having to effect the others.
You can use InputVCR to record player motion and any actions they take, for match replays, kill cams, 
puzzles, companions, and more.

----------

Place on any object you wish to use to record or playback any inputs for
 Switch modes to change current behaviour
   - Passthru : object will use live input commands from player
   - Record : object will use, as well as record, live input commands from player
   - Playback : object will use either provided input string or last recorded string rather than live input
   - Pause : object will take no input (buttons/axis will be frozen in last positions)
 
To use, place in a gameobject, and have all scripts in the object refer to it instead of Input.

eg: instead of Input.GetButton( "Jump" ), you would use vcr.GetButton( "Jump" ), where vcr is a 
reference to the component in that object
If VCR is in playback mode, and the "Jump" input was recorded, it will give the recorded input state, 
otherwise it will just pass through the live input state

Note, InputVCR can't be statically referenced like Input, since you may have multiple objects playing
different recordings, or an object playing back while another is taking live input...

----------

Use this snippet in scripts you wish to replace Input with InputVCR, so they can be used in objects without a VCR as well:
 
  private bool useVCR;
  private InputVCR vcr;
  
  void Awake()
  {
    Transform root = transform;
	while ( root.parent != null )
		root = root.parent;
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
Easy!
 
-------------

More information and tools at grapefruitgames.com, @eddiecameron, or support@grapefruitgames.com

-------------

Version info:
- v1.0
Initial release

- v1.1
Added option to sync position/rotation
Added check to make sure recording stays close to realtime (in case recording/playback frame rates are different)
More robust Recording format

- v1.2 _MAJOR UPDATE. INCOMPATIBLE WITH OLDER VERSIONS! (but way easier to use)
Switched to JSON storage and parsing. WAAAAAAAAAAAY more reliable than mine
Recording and playback now completely framerate independent
General cleanup of code + commenting and other help
