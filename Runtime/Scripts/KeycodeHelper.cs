/* KeycodeHelper.cs
 * Copyright Eddie Cameron 2019 (See readme for licence)
 * ----------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputVCR {
    public static class KeycodeHelper {
        /// <summary>
        /// Convert a KeyCode into the string that Unity uses in GetKeyString
        /// (ToString returns a different value -_-)
        /// </summary>
        /// <param name="kc"></param>
        /// <returns></returns>
        public static string KeycodeToKeyString( KeyCode kc ) {
            switch ( kc ) {
            // numerals
            case KeyCode.Alpha0:
                return "0";
            case KeyCode.Alpha1:
                return "1";
            case KeyCode.Alpha2:
                return "2";
            case KeyCode.Alpha3:
                return "3";
            case KeyCode.Alpha4:
                return "4";
            case KeyCode.Alpha5:
                return "5";
            case KeyCode.Alpha6:
                return "6";
            case KeyCode.Alpha7:
                return "7";
            case KeyCode.Alpha8:
                return "8";
            case KeyCode.Alpha9:
                return "9";

            case KeyCode.Keypad0:
                return "[0]";
            case KeyCode.Keypad1:
                return "[1]";
            case KeyCode.Keypad2:
                return "[2]";
            case KeyCode.Keypad3:
                return "[3]";
            case KeyCode.Keypad4:
                return "[4]";
            case KeyCode.Keypad5:
                return "[5]";
            case KeyCode.Keypad6:
                return "[6]";
            case KeyCode.Keypad7:
                return "[7]";
            case KeyCode.Keypad8:
                return "[8]";
            case KeyCode.Keypad9:
                return "[9]";
            case KeyCode.KeypadDivide:
                return "[/]";
            case KeyCode.KeypadEnter:
                return "enter";
            case KeyCode.KeypadEquals:
                return "equals";
            case KeyCode.KeypadMinus:
                return "[-]";
            case KeyCode.KeypadMultiply:
                return "[*]";
            case KeyCode.KeypadPeriod:
                return "[.]";
            case KeyCode.KeypadPlus:
                return "[+]";


            case KeyCode.UpArrow:
                return "up";
            case KeyCode.DownArrow:
                return "down";
            case KeyCode.LeftArrow:
                return "left";
            case KeyCode.RightArrow:
                return "right";

            case KeyCode.Mouse0:
                return "mouse 0";
            case KeyCode.Mouse1:
                return "mouse 1";
            case KeyCode.Mouse2:
                return "mouse 2";
            case KeyCode.Mouse3:
                return "mouse 3";
            case KeyCode.Mouse4:
                return "mouse 4";
            case KeyCode.Mouse5:
                return "mouse 5";
            case KeyCode.Mouse6:
                return "mouse 6";

            #region Joysticks
            case KeyCode.JoystickButton0:
                return "joystick button 0";
            case KeyCode.JoystickButton1:
                return "joystick button 1";
            case KeyCode.JoystickButton2:
                return "joystick button 2";
            case KeyCode.JoystickButton3:
                return "joystick button 3";
            case KeyCode.JoystickButton4:
                return "joystick button 4";
            case KeyCode.JoystickButton5:
                return "joystick button 5";
            case KeyCode.JoystickButton6:
                return "joystick button 6";
            case KeyCode.JoystickButton7:
                return "joystick button 7";
            case KeyCode.JoystickButton8:
                return "joystick button 8";
            case KeyCode.JoystickButton9:
                return "joystick button 9";
            case KeyCode.JoystickButton10:
                return "joystick button 10";
            case KeyCode.JoystickButton11:
                return "joystick button 11";
            case KeyCode.JoystickButton12:
                return "joystick button 12";
            case KeyCode.JoystickButton13:
                return "joystick button 13";
            case KeyCode.JoystickButton14:
                return "joystick button 14";
            case KeyCode.JoystickButton15:
                return "joystick button 15";
            case KeyCode.JoystickButton16:
                return "joystick button 16";
            case KeyCode.JoystickButton17:
                return "joystick button 17";
            case KeyCode.JoystickButton18:
                return "joystick button 18";
            case KeyCode.JoystickButton19:
                return "joystick button 19";

            case KeyCode.Joystick1Button0:
                return "joystick 1 button 0";
            case KeyCode.Joystick1Button1:
                return "joystick 1 button 1";
            case KeyCode.Joystick1Button2:
                return "joystick 1 button 2";
            case KeyCode.Joystick1Button3:
                return "joystick 1 button 3";
            case KeyCode.Joystick1Button4:
                return "joystick 1 button 4";
            case KeyCode.Joystick1Button5:
                return "joystick 1 button 5";
            case KeyCode.Joystick1Button6:
                return "joystick 1 button 6";
            case KeyCode.Joystick1Button7:
                return "joystick 1 button 7";
            case KeyCode.Joystick1Button8:
                return "joystick 1 button 8";
            case KeyCode.Joystick1Button9:
                return "joystick 1 button 9";
            case KeyCode.Joystick1Button10:
                return "joystick 1 button 10";
            case KeyCode.Joystick1Button11:
                return "joystick 1 button 11";
            case KeyCode.Joystick1Button12:
                return "joystick 1 button 12";
            case KeyCode.Joystick1Button13:
                return "joystick 1 button 13";
            case KeyCode.Joystick1Button14:
                return "joystick 1 button 14";
            case KeyCode.Joystick1Button15:
                return "joystick 1 button 15";
            case KeyCode.Joystick1Button16:
                return "joystick 1 button 16";
            case KeyCode.Joystick1Button17:
                return "joystick 1 button 17";
            case KeyCode.Joystick1Button18:
                return "joystick 1 button 18";
            case KeyCode.Joystick1Button19:
                return "joystick 1 button 19";

            case KeyCode.Joystick2Button0:
                return "joystick 2 button 0";
            case KeyCode.Joystick2Button1:
                return "joystick 2 button 1";
            case KeyCode.Joystick2Button2:
                return "joystick 2 button 2";
            case KeyCode.Joystick2Button3:
                return "joystick 2 button 3";
            case KeyCode.Joystick2Button4:
                return "joystick 2 button 4";
            case KeyCode.Joystick2Button5:
                return "joystick 2 button 5";
            case KeyCode.Joystick2Button6:
                return "joystick 2 button 6";
            case KeyCode.Joystick2Button7:
                return "joystick 2 button 7";
            case KeyCode.Joystick2Button8:
                return "joystick 2 button 8";
            case KeyCode.Joystick2Button9:
                return "joystick 2 button 9";
            case KeyCode.Joystick2Button10:
                return "joystick 2 button 10";
            case KeyCode.Joystick2Button11:
                return "joystick 2 button 11";
            case KeyCode.Joystick2Button12:
                return "joystick 2 button 12";
            case KeyCode.Joystick2Button13:
                return "joystick 2 button 13";
            case KeyCode.Joystick2Button14:
                return "joystick 2 button 14";
            case KeyCode.Joystick2Button15:
                return "joystick 2 button 15";
            case KeyCode.Joystick2Button16:
                return "joystick 2 button 16";
            case KeyCode.Joystick2Button17:
                return "joystick 2 button 17";
            case KeyCode.Joystick2Button18:
                return "joystick 2 button 18";
            case KeyCode.Joystick2Button19:
                return "joystick 2 button 19";

            case KeyCode.Joystick3Button0:
                return "joystick 3 button 0";
            case KeyCode.Joystick3Button1:
                return "joystick 3 button 1";
            case KeyCode.Joystick3Button2:
                return "joystick 3 button 2";
            case KeyCode.Joystick3Button3:
                return "joystick 3 button 3";
            case KeyCode.Joystick3Button4:
                return "joystick 3 button 4";
            case KeyCode.Joystick3Button5:
                return "joystick 3 button 5";
            case KeyCode.Joystick3Button6:
                return "joystick 3 button 6";
            case KeyCode.Joystick3Button7:
                return "joystick 3 button 7";
            case KeyCode.Joystick3Button8:
                return "joystick 3 button 8";
            case KeyCode.Joystick3Button9:
                return "joystick 3 button 9";
            case KeyCode.Joystick3Button10:
                return "joystick 3 button 10";
            case KeyCode.Joystick3Button11:
                return "joystick 3 button 11";
            case KeyCode.Joystick3Button12:
                return "joystick 3 button 12";
            case KeyCode.Joystick3Button13:
                return "joystick 3 button 13";
            case KeyCode.Joystick3Button14:
                return "joystick 3 button 14";
            case KeyCode.Joystick3Button15:
                return "joystick 3 button 15";
            case KeyCode.Joystick3Button16:
                return "joystick 3 button 16";
            case KeyCode.Joystick3Button17:
                return "joystick 3 button 17";
            case KeyCode.Joystick3Button18:
                return "joystick 3 button 18";
            case KeyCode.Joystick3Button19:
                return "joystick 3 button 19";

            case KeyCode.Joystick4Button0:
                return "joystick 4 button 0";
            case KeyCode.Joystick4Button1:
                return "joystick 4 button 1";
            case KeyCode.Joystick4Button2:
                return "joystick 4 button 2";
            case KeyCode.Joystick4Button3:
                return "joystick 4 button 3";
            case KeyCode.Joystick4Button4:
                return "joystick 4 button 4";
            case KeyCode.Joystick4Button5:
                return "joystick 4 button 5";
            case KeyCode.Joystick4Button6:
                return "joystick 4 button 6";
            case KeyCode.Joystick4Button7:
                return "joystick 4 button 7";
            case KeyCode.Joystick4Button8:
                return "joystick 4 button 8";
            case KeyCode.Joystick4Button9:
                return "joystick 4 button 9";
            case KeyCode.Joystick4Button10:
                return "joystick 4 button 10";
            case KeyCode.Joystick4Button11:
                return "joystick 4 button 11";
            case KeyCode.Joystick4Button12:
                return "joystick 4 button 12";
            case KeyCode.Joystick4Button13:
                return "joystick 4 button 13";
            case KeyCode.Joystick4Button14:
                return "joystick 4 button 14";
            case KeyCode.Joystick4Button15:
                return "joystick 4 button 15";
            case KeyCode.Joystick4Button16:
                return "joystick 4 button 16";
            case KeyCode.Joystick4Button17:
                return "joystick 4 button 17";
            case KeyCode.Joystick4Button18:
                return "joystick 4 button 18";
            case KeyCode.Joystick4Button19:
                return "joystick 4 button 19";

            case KeyCode.Joystick5Button0:
                return "joystick 5 button 0";
            case KeyCode.Joystick5Button1:
                return "joystick 5 button 1";
            case KeyCode.Joystick5Button2:
                return "joystick 5 button 2";
            case KeyCode.Joystick5Button3:
                return "joystick 5 button 3";
            case KeyCode.Joystick5Button4:
                return "joystick 5 button 4";
            case KeyCode.Joystick5Button5:
                return "joystick 5 button 5";
            case KeyCode.Joystick5Button6:
                return "joystick 5 button 6";
            case KeyCode.Joystick5Button7:
                return "joystick 5 button 7";
            case KeyCode.Joystick5Button8:
                return "joystick 5 button 8";
            case KeyCode.Joystick5Button9:
                return "joystick 5 button 9";
            case KeyCode.Joystick5Button10:
                return "joystick 5 button 10";
            case KeyCode.Joystick5Button11:
                return "joystick 5 button 11";
            case KeyCode.Joystick5Button12:
                return "joystick 5 button 12";
            case KeyCode.Joystick5Button13:
                return "joystick 5 button 13";
            case KeyCode.Joystick5Button14:
                return "joystick 5 button 14";
            case KeyCode.Joystick5Button15:
                return "joystick 5 button 15";
            case KeyCode.Joystick5Button16:
                return "joystick 5 button 16";
            case KeyCode.Joystick5Button17:
                return "joystick 5 button 17";
            case KeyCode.Joystick5Button18:
                return "joystick 5 button 18";
            case KeyCode.Joystick5Button19:
                return "joystick 5 button 19";

            case KeyCode.Joystick6Button0:
                return "joystick 6 button 0";
            case KeyCode.Joystick6Button1:
                return "joystick 6 button 1";
            case KeyCode.Joystick6Button2:
                return "joystick 6 button 2";
            case KeyCode.Joystick6Button3:
                return "joystick 6 button 3";
            case KeyCode.Joystick6Button4:
                return "joystick 6 button 4";
            case KeyCode.Joystick6Button5:
                return "joystick 6 button 5";
            case KeyCode.Joystick6Button6:
                return "joystick 6 button 6";
            case KeyCode.Joystick6Button7:
                return "joystick 6 button 7";
            case KeyCode.Joystick6Button8:
                return "joystick 6 button 8";
            case KeyCode.Joystick6Button9:
                return "joystick 6 button 9";
            case KeyCode.Joystick6Button10:
                return "joystick 6 button 10";
            case KeyCode.Joystick6Button11:
                return "joystick 6 button 11";
            case KeyCode.Joystick6Button12:
                return "joystick 6 button 12";
            case KeyCode.Joystick6Button13:
                return "joystick 6 button 13";
            case KeyCode.Joystick6Button14:
                return "joystick 6 button 14";
            case KeyCode.Joystick6Button15:
                return "joystick 6 button 15";
            case KeyCode.Joystick6Button16:
                return "joystick 6 button 16";
            case KeyCode.Joystick6Button17:
                return "joystick 6 button 17";
            case KeyCode.Joystick6Button18:
                return "joystick 6 button 18";
            case KeyCode.Joystick6Button19:
                return "joystick 6 button 19";

            case KeyCode.Joystick7Button0:
                return "joystick 7 button 0";
            case KeyCode.Joystick7Button1:
                return "joystick 7 button 1";
            case KeyCode.Joystick7Button2:
                return "joystick 7 button 2";
            case KeyCode.Joystick7Button3:
                return "joystick 7 button 3";
            case KeyCode.Joystick7Button4:
                return "joystick 7 button 4";
            case KeyCode.Joystick7Button5:
                return "joystick 7 button 5";
            case KeyCode.Joystick7Button6:
                return "joystick 7 button 6";
            case KeyCode.Joystick7Button7:
                return "joystick 7 button 7";
            case KeyCode.Joystick7Button8:
                return "joystick 7 button 8";
            case KeyCode.Joystick7Button9:
                return "joystick 7 button 9";
            case KeyCode.Joystick7Button10:
                return "joystick 7 button 10";
            case KeyCode.Joystick7Button11:
                return "joystick 7 button 11";
            case KeyCode.Joystick7Button12:
                return "joystick 7 button 12";
            case KeyCode.Joystick7Button13:
                return "joystick 7 button 13";
            case KeyCode.Joystick7Button14:
                return "joystick 7 button 14";
            case KeyCode.Joystick7Button15:
                return "joystick 7 button 15";
            case KeyCode.Joystick7Button16:
                return "joystick 7 button 16";
            case KeyCode.Joystick7Button17:
                return "joystick 7 button 17";
            case KeyCode.Joystick7Button18:
                return "joystick 7 button 18";
            case KeyCode.Joystick7Button19:
                return "joystick 7 button 19";

            case KeyCode.Joystick8Button0:
                return "joystick 8 button 0";
            case KeyCode.Joystick8Button1:
                return "joystick 8 button 1";
            case KeyCode.Joystick8Button2:
                return "joystick 8 button 2";
            case KeyCode.Joystick8Button3:
                return "joystick 8 button 3";
            case KeyCode.Joystick8Button4:
                return "joystick 8 button 4";
            case KeyCode.Joystick8Button5:
                return "joystick 8 button 5";
            case KeyCode.Joystick8Button6:
                return "joystick 8 button 6";
            case KeyCode.Joystick8Button7:
                return "joystick 8 button 7";
            case KeyCode.Joystick8Button8:
                return "joystick 8 button 8";
            case KeyCode.Joystick8Button9:
                return "joystick 8 button 9";
            case KeyCode.Joystick8Button10:
                return "joystick 8 button 10";
            case KeyCode.Joystick8Button11:
                return "joystick 8 button 11";
            case KeyCode.Joystick8Button12:
                return "joystick 8 button 12";
            case KeyCode.Joystick8Button13:
                return "joystick 8 button 13";
            case KeyCode.Joystick8Button14:
                return "joystick 8 button 14";
            case KeyCode.Joystick8Button15:
                return "joystick 8 button 15";
            case KeyCode.Joystick8Button16:
                return "joystick 8 button 16";
            case KeyCode.Joystick8Button17:
                return "joystick 8 button 17";
            case KeyCode.Joystick8Button18:
                return "joystick 8 button 18";
            case KeyCode.Joystick8Button19:
                return "joystick 8 button 19";
            #endregion

            case KeyCode.AltGr:
                return "alt gr";
            case KeyCode.Ampersand:
                return "&";
            case KeyCode.Asterisk:
                return "*";
            case KeyCode.At:
                return "@";
            case KeyCode.BackQuote:
                return "`";
            case KeyCode.Backslash:
                return @"\";
            case KeyCode.CapsLock:
                return "caps lock";
            case KeyCode.Caret:
                return "^";
            case KeyCode.Colon:
                return ":";
            case KeyCode.Comma:
                return ",";
            case KeyCode.Dollar:
                return "$";
            case KeyCode.DoubleQuote:
                return "\"";
            case KeyCode.Equals:
                return "=";
            case KeyCode.Exclaim:
                return "!";
            case KeyCode.Greater:
                return ">";
            case KeyCode.Hash:
                return "#";
            case KeyCode.LeftAlt:
                return "left alt";
            case KeyCode.LeftBracket:
                return "[";
            case KeyCode.LeftCommand:
                return "left cmd";
            case KeyCode.LeftControl:
                return "left ctrl";
            case KeyCode.LeftCurlyBracket:
                return "{";
            case KeyCode.LeftParen:
                return "(";
            case KeyCode.LeftShift:
                return "left shift";
            case KeyCode.Less:
                return "<";
            case KeyCode.Minus:
                return "-";
            case KeyCode.PageDown:
                return "page down";
            case KeyCode.PageUp:
                return "page up";
            case KeyCode.Percent:
                return "%";
            case KeyCode.Period:
                return ".";
            case KeyCode.Pipe:
                return "|";
            case KeyCode.Plus:
                return "+";
            case KeyCode.Print:
                return "print screen";
            case KeyCode.Question:
                return "?";
            case KeyCode.Quote:
                return "'";
            case KeyCode.RightAlt:
                return "right alt";
            case KeyCode.RightBracket:
                return "]";
            case KeyCode.RightCommand:
                return "right cmd";
            case KeyCode.RightControl:
                return "right ctrl";
            case KeyCode.RightCurlyBracket:
                return "}";
            case KeyCode.RightParen:
                return ")";
            case KeyCode.RightShift:
                return "right shift";
            case KeyCode.ScrollLock:
                return "scroll lock";
            case KeyCode.Semicolon:
                return ";";
            case KeyCode.Slash:
                return "/";
            case KeyCode.SysReq:
                return "sys req";
            case KeyCode.Tilde:
                return "~";
            case KeyCode.Underscore:
                return "_";

            case KeyCode.LeftWindows:
                return "left super";        // guessing
            case KeyCode.RightWindows:
                return "right super";

            default:
                return kc.ToString().ToLower();
            }
        }
    }
}
