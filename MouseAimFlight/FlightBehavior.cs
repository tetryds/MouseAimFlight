/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    class FlightBehavior
    {
        List<FlightModes.Flight> modes;

        //Hardcoded Behaviors
        FlightModes.NormalFlight normalFlight;
        FlightModes.CruiseFlight cruiseFlight;

        private int activeMode = 0;

        public FlightBehavior() //Hardcoded Behaviors listing, can be made dynamic
        {
            modes = new List<FlightModes.Flight>();

            normalFlight = new FlightModes.NormalFlight();
            AddBehavior(normalFlight);

            cruiseFlight = new FlightModes.CruiseFlight();
            AddBehavior(cruiseFlight);
        }

        void AddBehavior(FlightModes.Flight newBehavior) //Adds behavior to the behaviors list
        {
            modes.Add(newBehavior);
        }

        public ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            ErrorData errors = modes[activeMode].Simulate(vesselTransform, velocityTransform, targetPosition, upDirection, upWeighting, vessel);
            return errors;
        }

        public void SetBehavior(int mode)
        {
            this.activeMode = mode;
        }

        public void NextBehavior()
        {
            activeMode++;
            if (activeMode >= modes.Count)
                activeMode = 0;
        }

        int GetBehavior()
        {
            return activeMode;
        }

        public string GetBehaviorName()
        {
            return modes[activeMode].GetFlightMode();
        }
    }

    public struct ErrorData
    {
        public float pitchError;
        public float rollError;
        public float yawError;

        public ErrorData(float p, float r, float y)
        {
            pitchError = p;
            rollError = r;
            yawError = y;
        }
    }
}
