/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseAimFlight
{
    class MouseAimSettings
    {
        static MouseAimSettings instance;
        public static MouseAimSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new MouseAimSettings();

                return instance;
            }
        }

        static KeyCode toggleKeyCode = KeyCode.P;
        public static KeyCode ToggleKeyCode
        {
            get { return toggleKeyCode; }
        }
        static string toggleKeyString = "P";
        public static string ToggleKeyString
        {
            get { return toggleKeyString; }
            set
            {
                string tmp = value.ToUpperInvariant();
                string oldKeyString = toggleKeyString;
                KeyCode oldKeyCode = toggleKeyCode;
                if(tmp != toggleKeyString && tmp.Length == 1)
                {
                    try {
                        toggleKeyString = tmp;
                        toggleKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), toggleKeyString);
                    } catch (ArgumentException) {
                        // invalid keycode
                        toggleKeyString = oldKeyString;
                        toggleKeyCode = oldKeyCode;
                        Debug.Log("[MAF]: Invalid keycode entered for toggle");
                    }
                    Instance.SaveSettings();
                }
            }
        }

        static KeyCode flightModeKeyCode = KeyCode.O;
        public static KeyCode FlightModeKeyCode
        {
            get { return flightModeKeyCode; }
        }
        static string flightModeKeyString = "O";
        public static string FlightModeKeyString
        {
            get { return flightModeKeyString; }
            set
            {
                string tmp = value.ToUpperInvariant();
                string oldKeyString = flightModeKeyString;
                KeyCode oldKeyCode = flightModeKeyCode;
                if (tmp != flightModeKeyString && tmp.Length == 1)
                {
                    try {
                        flightModeKeyString = tmp;
                        flightModeKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), flightModeKeyString);
                    } catch (ArgumentException) {
                        // invalid keycode
                        flightModeKeyString = oldKeyString;
                        flightModeKeyCode = oldKeyCode;
                        Debug.Log("[MAF]: Invalid keycode entered for flightMode");
                    }
                    Instance.SaveSettings();
                }
            }
        }

        public enum CursorStyle
        {
            FULL,
            DOT,
            NONE
        }
        static CursorStyle cursor = CursorStyle.FULL;
        public static CursorStyle Cursor
        {
            get { return cursor; }
            set
            {
                if (value != cursor)
                {
                    cursor = value;
                    Instance.SaveSettings();
                }
            }
        }

        static int mouseSensitivity = 100;
        public static int MouseSensitivity
        {
            get { return mouseSensitivity; }
            set
            {
                if (value != mouseSensitivity)
                {
                    mouseSensitivity = value;
                    if (mouseSensitivity >= 98 && mouseSensitivity <= 102) //Making sure you can get it back to 100
                        mouseSensitivity = 100;
                    Instance.SaveSettings();
                }
            }
        }

        static float cursorOpacity = 1;
        public static float CursorOpacity
        {
            get { return cursorOpacity; }
            set
            {
                if (value != cursorOpacity)
                {
                    cursorOpacity = (float)Math.Round(value, 2);
                    Instance.SaveSettings();
                }
            }
        }

        static float cursorSize = 1;
        public static float CursorSize
        {
            get { return cursorSize; }
            set
            {
                if (value != cursorSize)
                {
                    cursorSize = (float)Math.Round(value, 2);
                    Instance.SaveSettings();
                }
            }
        }

        static bool invertY = false;
        static bool invertX = false;
        public static bool InvertYAxis
        {
            get { return invertY; }
            set
            {
                if (value != invertY)
                {
                    invertY = value;
                    Instance.SaveSettings();
                }
            }
        }

        public static bool InvertXAxis
        {
            get { return invertX; }
            set
            {
                if (value != invertX)
                {
                    invertX = value;
                    Instance.SaveSettings();
                }
            }
        }

        static bool fARLoaded;
        public static bool FARLoaded
        {
            get { return fARLoaded; }
            set
            {
                fARLoaded = value;
            }
        }

        MouseAimSettings()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            foreach(ConfigNode node in GameDatabase.Instance.GetConfigNodes("MAFSettings"))
                if((object)node != null)
                {
                    if (node.HasValue("toggleKey"))
                    {
                        try {
                            toggleKeyString = ((string)node.GetValue("toggleKey")).ToUpperInvariant();
                            toggleKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), toggleKeyString);
                        } catch (ArgumentException) {
                            Debug.Log("[MAF]: Invalid keycode in toggleKey config node; skipped");
                        }
                    }
                    if (node.HasValue("flightModeKey"))
                    {
                        try {
                            flightModeKeyString = ((string)node.GetValue("flightModeKey")).ToUpperInvariant();
                            flightModeKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), flightModeKeyString);
                        } catch (ArgumentException) {
                            Debug.Log("[MAF]: Invalid keycode in flightModeKey config node; skipped");
                        }
                    }
                    if (node.HasValue("cursorStyle"))
                    {
                        object temp = Enum.Parse(typeof(CursorStyle), (string)node.GetValue("cursorStyle"));
                        if (temp != null)
                            cursor = (CursorStyle)temp;
                    }
                    if (node.HasValue("mouseSensitivity"))
                    {
                        int.TryParse(node.GetValue("mouseSensitivity"), out mouseSensitivity);
                    }
                    if (node.HasValue("cursorOpacity"))
                    {
                        float.TryParse(node.GetValue("cursorOpacity"), out cursorOpacity);
                    }
                    if (node.HasValue("cursorSize"))
                    {
                        float.TryParse(node.GetValue("cursorSize"), out cursorSize);
                    }
                    if (node.HasValue("invertX"))
                    {
                        bool.TryParse(node.GetValue("invertX"), out invertX);
                    }
                    if (node.HasValue("invertY"))
                    {
                        bool.TryParse(node.GetValue("invertY"), out invertX);
                    }
                }
            DetectFARLoaded();
        }

        public void SaveSettings()
        {
            ConfigNode node = new ConfigNode("MAFSettings");
            node.AddValue("name", "default");
            node.AddValue("toggleKey", toggleKeyCode.ToString());
            node.AddValue("flightModeKey", flightModeKeyCode.ToString());
            node.AddValue("cursorStyle", cursor.ToString());
            node.AddValue("mouseSensitivity", mouseSensitivity.ToString());
            node.AddValue("cursorOpacity", cursorOpacity.ToString());
            node.AddValue("cursorSize", CursorSize.ToString());
            node.AddValue("invertX", invertX.ToString());
            node.AddValue("invertY", invertY.ToString());

            ConfigNode saveNode = new ConfigNode();
            saveNode.AddNode(node);
            saveNode.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/MouseAimFlight/MAFSettings.cfg");
        }

        void DetectFARLoaded()
        {
            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == "FerramAerospaceResearch")
                {
                    FARLoaded = true;
                    break;
                }
                else
                    FARLoaded = false;
            }

            if(FARLoaded)
                Debug.Log("[MAF]: FAR loaded, disabling control surfaces tweaks");
            else
                Debug.Log("[MAF]: Stock aero model loaded, ready to tweak control surfaces");
        }
    }
}
