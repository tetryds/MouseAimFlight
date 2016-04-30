/*
Copyright (c) 2016, BahamutoD, ferram4, tetryds
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
 
* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

namespace MouseAimFlight
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class MouseAimFlightSceneGUI : MonoBehaviour
    {
        static MouseAimFlightSceneGUI instance;
        public static MouseAimFlightSceneGUI Instance { get { return instance; } }

        static bool ready = false;
        static ApplicationLauncherButton mAFButton = null;
        static bool showGUI = false;
        static Rect guiRect;

        static Texture2D vesselForwardReticle;
        static Texture2D mouseCursorReticle;

        static Texture2D vesselForwardCross;
        static Texture2D vesselForwardDot;
        static Texture2D vesselForwardBlank;

        void Start()
        {
            instance = this;
            if (mouseCursorReticle == null)
            {
                mouseCursorReticle = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/circle", false);
                mouseCursorReticle.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardCross == null)
            {
                vesselForwardCross = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/cross", false);
                vesselForwardCross.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardDot == null)
            {
                vesselForwardDot = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/dot", false);
                vesselForwardDot.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardBlank == null)
            {
                vesselForwardBlank = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/blank", false);
                vesselForwardBlank.filterMode = FilterMode.Trilinear;
            }
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            MouseAimSettings.Instance.SaveSettings();
            UpdateCursor(MouseAimSettings.Cursor);
        }

        public static void DisplayMouseAimReticles(Vector3 mouseAimScreenLocation, Vector3 vesselForwardScreenLocation)
        {
            float size = Screen.width / 32;
            if (mouseAimScreenLocation.z > 0)
            {
                Rect aimRect = new Rect(mouseAimScreenLocation.x - (0.5f * size), (Screen.height - mouseAimScreenLocation.y) - (0.5f * size), size, size);
                Color oldcolor = GUI.color;
                GUI.color = new Color(1, 1, 1, MouseAimSettings.CursorOpacity);
                GUI.DrawTexture(aimRect, mouseCursorReticle);
                GUI.color = oldcolor;
            }

            if (vesselForwardScreenLocation.z > 0)
            {
                Rect directionRect = new Rect(vesselForwardScreenLocation.x - (0.5f * size), (Screen.height - vesselForwardScreenLocation.y) - (0.5f * size), size, size);
                Color oldcolor = GUI.color;
                GUI.color = new Color(1, 1, 1, MouseAimSettings.CursorOpacity);
                GUI.DrawTexture(directionRect, vesselForwardReticle);
                GUI.color = oldcolor;
            }
        }

        void OnGUI()
        {
            if (showGUI)
            {
                GUI.skin = HighLogic.Skin;

                guiRect = GUILayout.Window(this.GetHashCode(), guiRect, GUIWindow, "MouseAim Settings");
            }
        }

        void GUIWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.Width(200));
            GUILayout.Label("Toggle MouseAim Key:  "); //Ugly cheap way of aligning input boxes by adding a space on the end
            MouseAimSettings.ToggleKeyString = GUILayout.TextField(MouseAimSettings.ToggleKeyString, 1, GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Width(200));
            GUILayout.Label("Toggle FlightMode Key: ");
            MouseAimSettings.FlightModeKeyString = GUILayout.TextField(MouseAimSettings.FlightModeKeyString, 1, GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal(GUILayout.Width(200));
            if(GUILayout.Button("Cursor: ", GUILayout.Width(100)))
                CycleCursor();
            Color oldcolor = GUI.color;
            GUI.color = new Color(1, 1, 1, MouseAimSettings.CursorOpacity);
            GUI.DrawTexture(new Rect(125, 100, 60, 60), vesselForwardReticle);
            GUI.color = oldcolor;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("Mouse Sensitivity: " + MouseAimSettings.MouseSensitivity);
            MouseAimSettings.MouseSensitivity = GUILayout.HorizontalSlider(MouseAimSettings.MouseSensitivity, 25, 500);
            GUILayout.Label("Cursor Opacity: " + MouseAimSettings.CursorOpacity);
            MouseAimSettings.CursorOpacity = GUILayout.HorizontalSlider(MouseAimSettings.CursorOpacity, 0, 1);
            MouseAimSettings.InvertXAxis = GUILayout.Toggle(MouseAimSettings.InvertXAxis, "Invert X Axis");
            MouseAimSettings.InvertYAxis = GUILayout.Toggle(MouseAimSettings.InvertYAxis, "Invert Y Axis");
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void CycleCursor()
        {
            if (MouseAimSettings.Cursor == MouseAimSettings.CursorStyle.FULL)
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.DOT;
            else if (MouseAimSettings.Cursor == MouseAimSettings.CursorStyle.DOT)
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.NONE;
            else
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.FULL;

            UpdateCursor(MouseAimSettings.Cursor);
        }
        
        void UpdateCursor(MouseAimSettings.CursorStyle cursor)
        {
            if(cursor == MouseAimSettings.CursorStyle.FULL)
                vesselForwardReticle = vesselForwardCross;
            else if (cursor == MouseAimSettings.CursorStyle.DOT)
                vesselForwardReticle = vesselForwardDot;
            else
                vesselForwardReticle = vesselForwardBlank;
        }

       #region AppLauncher
        public void OnGUIAppLauncherReady()
        {
            if (mAFButton == null)
            {
                mAFButton = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/MAF_icon", false));
            }
        }

        void onAppLaunchToggleOn()
        {
            showGUI = true;
        }

        void onAppLaunchToggleOff()
        {
            showGUI = false;
        }
        #endregion
    }
}
