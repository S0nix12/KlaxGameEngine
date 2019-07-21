namespace KlaxIO.Input
{
	/// <summary>
	/// Contains definitions for button across device boundaries, keyboard key names are the same as in DirectInput
	/// </summary>
	public enum EInputButton
	{
        //DIK_ESCAPE
        Escape = 1,	
        //DIK_1
        D1 = 2,	
        //DIK_2
        D2 = 3,
        //DIK_3
        D3 = 4,			
        //DIK_4
        D4 = 5,
        //DIK_5
        D5 = 6,		
        //DIK_6
        D6 = 7,	
        //DIK_7
        D7 = 8,	
        //DIK_8
        D8 = 9,
        //DIK_9
        D9 = 10,	
        //DIK_0
        D0 = 11,	
        //DIK_MINUS
        Minus = 12,
        //DIK_EQUALS	
        Equals = 13,
        //DIK_BACK	
        Back = 14,
        //DIK_TAB	
        Tab = 15,
        //DIK_Q	
        Q = 16,
        //DIK_W	
        W = 17,	
        //DIK_E	
        E = 18,
        //DIK_R	
        R = 19,
        //DIK_T	
        T = 20,
        //DIK_Y	
        Y = 21,	
        //DIK_U	
        U = 22,	
        //DIK_I	
        I = 23,	
        //DIK_O	
        O = 24,
        //DIK_P	
        P = 25,
        //DIK_LBRACKET	
        LeftBracket = 26,	
        //DIK_RBRACKET	
        RightBracket = 27,
        //DIK_RETURN	
        Return = 28,
        //DIK_LCONTROL	
        LeftControl = 29,
        //DIK_A	
        A = 30,
        //DIK_S	
        S = 31,	
        //DIK_D	
        D = 32,	
        //DIK_F	
        F = 33,	
        //DIK_G	
        G = 34,	
        //DIK_H	
        H = 35,	
        //DIK_J	
        J = 36,
        //DIK_K	
        K = 37,
        //DIK_L	
        L = 38,	
        //DIK_SEMICOLON	
        Semicolon = 39,	
        //DIK_APOSTROPHE	
        Apostrophe = 40,	
        //DIK_GRAVE	
        Grave = 41,	
        //DIK_LSHIFT	
        LeftShift = 42,	
        //DIK_BACKSLASH	
        Backslash = 43,
        //DIK_Z	
        Z = 44,	
        //DIK_X	
        X = 45,	
        //DIK_C	
        C = 46,	
        //DIK_V	
        V = 47,	
        //DIK_B	
        B = 48,	
        //DIK_N	
        N = 49,	
        //DIK_M	
        M = 50,	
        //DIK_COMMA	
        Comma = 51,	
        //DIK_PERIOD	
        Period = 52,	
        //DIK_SLASH	
        Slash = 53,
        //DIK_RSHIFT	
        RightShift = 54,
        //DIK_MULTIPLY	
        Multiply = 55,
        //DIK_LMENU	
        LeftAlt = 56,
        //DIK_SPACE	
        Space = 57,
        //DIK_CAPITAL	
        Capital = 58,
        //DIK_F1	
        F1 = 59,
        //DIK_F2	
        F2 = 60,	
        //DIK_F3	
        F3 = 61,
        //DIK_F4	
        F4 = 62,	
        //DIK_F5	
        F5 = 63,	
        //DIK_F6	
        F6 = 64,
        //DIK_F7	
        F7 = 65,
        //DIK_F8	
        F8 = 66,
        //DIK_F9	
        F9 = 67,	
        //DIK_F10	
        F10 = 68,
        //DIK_NUMLOCK	
        NumberLock = 69,
        //DIK_SCROLL	
        ScrollLock = 70,	
        //DIK_NUMPAD7	
        NumberPad7 = 71,
        //DIK_NUMPAD8	
        NumberPad8 = 72,
        //DIK_NUMPAD9	
        NumberPad9 = 73,
        //DIK_SUBTRACT	
        Subtract = 74,
        //DIK_NUMPAD4	
        NumberPad4 = 75,
        //DIK_NUMPAD5	
        NumberPad5 = 76,
        //DIK_NUMPAD6	
        NumberPad6 = 77,
        //DIK_ADD	
        Add = 78,
        //DIK_NUMPAD1	
        NumberPad1 = 79,
        //DIK_NUMPAD2	
        NumberPad2 = 80,
        //DIK_NUMPAD3	
        NumberPad3 = 81,	
        //DIK_NUMPAD0	
        NumberPad0 = 82,
        //DIK_DECIMAL	
        Decimal = 83,
        //DIK_OEM_102	
        Oem102 = 86,
        //DIK_F11	
        F11 = 87,
        //DIK_F12	
        F12 = 88,
        //DIK_F13	
        F13 = 100,
        //DIK_F14	
        F14 = 101,
        //DIK_F15	
        F15 = 102,
        //DIK_KANA	
        Kana = 112,
        //DIK_ABNT_C1	
        AbntC1 = 115,
        //DIK_CONVERT	
        Convert = 121,	
        //DIK_NOCONVERT	
        NoConvert = 123,
        //DIK_YEN	
        Yen = 125,
        //DIK_ABNT_C2	
        AbntC2 = 126,
        //DIK_NUMPADEQUALS	
        NumberPadEquals = 141,	
        //DIK_PREVTRACK	
        PreviousTrack = 144,
        //DIK_AT	
        AT = 145,
        //DIK_COLON	
        Colon = 146,
        //DIK_UNDERLINE	
        Underline = 147,
        //DIK_KANJI	
        Kanji = 148,
        //DIK_STOP	
        Stop = 149,	
        //DIK_AX	
        AX = 150,	
        //DIK_UNLABELED	
        Unlabeled = 151,
        //DIK_NEXTTRACK	
        NextTrack = 153,	
        //DIK_NUMPADENTER	
        NumberPadEnter = 156,
        //DIK_RCONTROL	
        RightControl = 157,
        //DIK_MUTE	
        Mute = 160,	
        //DIK_CALCULATOR	
        Calculator = 161,	
        //DIK_PLAYPAUSE	
        PlayPause = 162,	
        //DIK_MEDIASTOP	
        MediaStop = 164,	
        //DIK_VOLUMEDOWN	
        VolumeDown = 174,	
        //DIK_VOLUMEUP	
        VolumeUp = 176,
        //DIK_WEBHOME	
        WebHome = 178,
        //DIK_NUMPADCOMMA	
        NumberPadComma = 179,
        //DIK_DIVIDE	
        Divide = 181,
        //DIK_SYSRQ	
        PrintScreen = 183,
        //DIK_RMENU	
        RightAlt = 184,
        //DIK_PAUSE	
        Pause = 197,
        //DIK_HOME	
        Home = 199,
        //DIK_UP	
        Up = 200,
        //DIK_PRIOR	
        PageUp = 201,	
        //DIK_LEFT	
        Left = 203,
        //DIK_RIGHT	
        Right = 205,
        //DIK_END	
        End = 207,
        //DIK_DOWN	
        Down = 208,
        //DIK_NEXT	
        PageDown = 209,
        //DIK_INSERT	
        Insert = 210,
        //DIK_DELETE	
        Delete = 211,
        //DIK_LWIN	
        LeftWindowsKey = 219,
        //DIK_RWIN	
        RightWindowsKey = 220,
        //DIK_APPS	
        Applications = 221,
        //DIK_POWER	
        Power = 222,
        //DIK_SLEEP	
        Sleep = 223,
        //DIK_WAKE	
        Wake = 227,
        //DIK_WEBSEARCH	
        WebSearch = 229,
        //DIK_WEBFAVORITES	
        WebFavorites = 230,
        //DIK_WEBREFRESH	
        WebRefresh = 231,
        //DIK_WEBSTOP	
        WebStop = 232,
        //DIK_WEBFORWARD	
        WebForward = 233,
        //DIK_WEBBACK	
        WebBack = 234,
        //DIK_MYCOMPUTER	
        MyComputer = 235,
        //DIK_MAIL	
        Mail = 236,
        //DIK_MEDIASELECT	
        MediaSelect = 237,

		MouseFirst = 300,
		//Mouse button 0
		MouseLeftButton = 300,
		//Mouse button 1
		MouseRightButton = 301,
		//Mouse button 2
		MouseMiddleButton = 302,
		MouseButton3 = 303,
		MouseButton4 = 304,
		MouseButton5 = 305,
		MouseButton6 = 306,
		MouseButton7 = 307,

		// XInput Controller Buttons
		// These are not flags to query the state of the button from a XInput State
		ControllerFirst = 400,
		DPadUp = 401,
		DPadDown = 402,
		DPadLeft = 403,
		DPadRight = 404,
		Start = 405,
		Options = 406,
		LeftThumb = 407,
		RightThumb = 408,
		LeftShoulder = 409,
		RightShoulder = 410,
		ControllerActionUp = 411,
		ControllerActionDown = 412,
		ControllerActionLeft = 413,
		ControllerActionRight = 414,

        //DIK_UNKNOWN	
        Unknown = 0,
	}
}
