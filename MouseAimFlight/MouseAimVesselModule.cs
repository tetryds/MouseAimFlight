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
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class MouseAimVesselModule : VesselModule
    {
        Transform vesselTransform;

        AdaptivePID pilot;
        FlightBehavior flightMode;

        static Vessel prevActiveVessel = null;
        bool mouseAimActive = false;
        static bool freeLook = false;
        static bool prevFreeLook = false;
        static bool forceCursorResetNextFrame = false;
        static bool pitchYawOverrideMouseAim = false;
        static FieldInfo freeLookKSPCameraField = null;
        
        Vector3 upDirection;
        Vector3 targetPosition;
        Vector3 mouseAimScreenLocation;
        Vector3 vesselForwardScreenLocation;

        GameObject vobj;
        Transform velocityTransform
        {
            get
            {
                if (!vobj)
                {
                    vobj = new GameObject("velObject");
                    vobj.transform.position = vessel.ReferenceTransform.position;
                    vobj.transform.parent = vessel.ReferenceTransform;
                }

                return vobj.transform;
            }
        }

        void ToggleMouseAim() //Mouse aim must not be toggled by anything other than this function
        {
            mouseAimActive = !mouseAimActive;
            if (mouseAimActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ScreenMessages.PostScreenMessage("MAF Enabled: " + flightMode.GetBehaviorName());
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ScreenMessages.PostScreenMessage("MAF Disabled");
            }
            targetPosition = vesselTransform.up * 5000f;     //if it's activated, set it to the baseline
            UpdateCursorScreenLocation();
            TweakControlSurfaces(mouseAimActive); //Remove when stock control surfaces are fixed
        }

        void Start()
        {
            vessel.OnAutopilotUpdate += MouseAimPilot;

            pilot = new AdaptivePID();
            flightMode = new FlightBehavior();

            vesselTransform = vessel.ReferenceTransform;
            targetPosition = vesselTransform.up * 5000;     //if it's activated, set it to the baseline

            FieldInfo[] cameraMouseLookStaticFields = typeof(CameraMouseLook).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            freeLookKSPCameraField = cameraMouseLookStaticFields[0];
            
        }

        void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && mouseAimActive && !MapView.MapIsEnabled)
            {
                MouseAimFlightSceneGUI.DisplayMouseAimReticles(mouseAimScreenLocation, vesselForwardScreenLocation);
            }
        }

        void Update()
        {
            if ((vessel != FlightGlobals.ActiveVessel) || vessel.isEVA)
            {
                if (mouseAimActive)
                    ToggleMouseAim();
                return;
            }

            if (PauseMenu.isOpen)
            {
                if (mouseAimActive)
                    ToggleMouseAim();
                //forceCursorResetNextFrame = true;
                return;
            } 
            
            bool enableHotkeys = !MapView.MapIsEnabled && !InputLockManager.IsAllLocked(ControlTypes.KEYBOARDINPUT);
            if (vessel == FlightGlobals.ActiveVessel && vessel != prevActiveVessel)
            {
                prevActiveVessel = vessel;
                if (mouseAimActive)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else if (enableHotkeys && Input.GetKeyDown(MouseAimSettings.ToggleKeyCode))
            {
                ToggleMouseAim();
            }

            if (enableHotkeys && Input.GetKeyDown(MouseAimSettings.FlightModeKeyCode))
            {
                flightMode.NextBehavior();
                ScreenMessages.PostScreenMessage("Flight Mode: " + flightMode.GetBehaviorName());
            }

            if (!mouseAimActive)
                return;

            UpdateMouseCursorForCameraRotation();
            UpdateVesselScreenLocation();
            UpdateCursorScreenLocation();
        }

        void LateUpdate()
        {
            if (vessel == FlightGlobals.ActiveVessel)
                CheckResetCursor();
        }

        void MouseAimPilot(FlightCtrlState s)
        {
            if (vessel != FlightGlobals.ActiveVessel || !mouseAimActive || PauseMenu.isOpen) //Now this depends only on if mouse aim is active or not, but will leave it this way for now
                return;

            vesselTransform = vessel.ReferenceTransform;

            if (s.pitch != s.pitchTrim || s.yaw != s.yawTrim)
            {
                pitchYawOverrideMouseAim = true;
                return;
            }
            else
                pitchYawOverrideMouseAim = false;

            upDirection = VectorUtils.GetUpDirection(vesselTransform.position);

            FlyToPosition(s, targetPosition + vessel.CoM);
        }

        void UpdateMouseCursorForCameraRotation()
        {
            if (pitchYawOverrideMouseAim)
            {
                targetPosition = vesselTransform.up * 5000f;
            }
            else
            {
                Vector3 mouseDelta;

                if (freeLook)
                    mouseDelta = Vector3.zero;
                else
                    mouseDelta = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseAimSettings.MouseSensitivity;

                if (MouseAimSettings.InvertXAxis)
                    mouseDelta.x *= -1;
                if (MouseAimSettings.InvertYAxis)
                    mouseDelta.y *= -1;

                Transform cameraTransform = FlightCamera.fetch.mainCamera.transform;

                Vector3d localTarget = cameraTransform.InverseTransformDirection(targetPosition);
                localTarget += mouseDelta;
                localTarget.Normalize();
                localTarget *= 5000f;

                targetPosition = cameraTransform.TransformDirection(localTarget);
            }
        }

        void UpdateCursorScreenLocation()
        {
            mouseAimScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(targetPosition + vessel.CoM);
        }

        void UpdateVesselScreenLocation()
        {
            vesselForwardScreenLocation = vesselTransform.up * 5000f;
            vesselForwardScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(vesselForwardScreenLocation + vessel.CoM);
        }

        void CheckResetCursor()
        {
            if (MapView.MapIsEnabled || PauseMenu.isOpen)
                return;

            prevFreeLook = freeLook;

            if (Mouse.Right.GetButton())
                freeLook = true;
            else if (freeLook)
                freeLook = false;


            freeLook |= (bool)freeLookKSPCameraField.GetValue(null);

            if ((freeLook != prevFreeLook || forceCursorResetNextFrame) && mouseAimActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                forceCursorResetNextFrame = false;
            }
        }

        void FlyToPosition(FlightCtrlState s, Vector3 targetPosition)
        {
            Vector3 localAngVel = vessel.angularVelocity * Mathf.Rad2Deg;

            float terrainAltitude;
            float dynPressure;
            float velocity;

            //Setup
            terrainAltitude = (float)vessel.heightFromTerrain;
            dynPressure = (float)vessel.dynamicPressurekPa;
            velocity = (float)vessel.srfSpeed;

            float upWeighting = pilot.UpWeighting(terrainAltitude, velocity);

            //Calculating errors
            ErrorData behavior = flightMode.Simulate(vesselTransform, velocityTransform, targetPosition, upDirection, upWeighting, vessel);

            //Controlling
            Steer steer = pilot.Simulate(behavior.pitchError, behavior.rollError, behavior.yawError, localAngVel, terrainAltitude, TimeWarp.fixedDeltaTime, dynPressure, velocity);

            //Piloting
            s.pitch = Mathf.Clamp(steer.pitch, -1, 1);
            if (s.roll == s.rollTrim)
                s.roll = Mathf.Clamp(steer.roll, -1, 1);
            s.yaw = Mathf.Clamp(steer.yaw, -1, 1);
        }

        void TweakControlSurfaces(bool mouseFlightActive) //Tweak stock control surfaces for sane behavior
        {
            if (!MouseAimSettings.FARLoaded)
            {
                if (mouseFlightActive)
                {
                    foreach (var ctrlSurface in vessel.FindPartModulesImplementing<ModuleControlSurface>()) //Only use if not performance critical, really.
                    {
                        ctrlSurface.useExponentialSpeed = true;
                        ctrlSurface.actuatorSpeed *= 3.5f;
                    }
                    Debug.Log("[MAF]: MAF Enabled, Control Surfaces Tweaked");
                }
                else
                {
                    foreach (var ctrlSurface in vessel.FindPartModulesImplementing<ModuleControlSurface>()) //Only use if not performance critical, really.
                    {
                        ctrlSurface.useExponentialSpeed = false;
                        ctrlSurface.actuatorSpeed /= 3.5f;
                    }
                    Debug.Log("[MAF]: MAF Disabled, Control Surfaces Reverted");
                }
            }
        }

        void OnDestroy()
        {
            if (vessel)
                vessel.OnAutopilotUpdate -= MouseAimPilot;
        }
    }
}
