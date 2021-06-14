using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class FlightController
    {
        private const string DefaultMode = "Basic";

        Dictionary<string, IFlightAI> flightModes;
        IFlightAI current;

        public FlightController()
        {
            flightModes = new Dictionary<string, IFlightAI>
            {
                ["Basic"] = new BasicFlight()
            };

            SetFlightMode(DefaultMode);
        }

        public void SetFlightMode(string mode)
        {
            if (!flightModes.TryGetValue(mode, out IFlightAI flightAI))
                throw new Exception("Flight mode not found");

            current = flightAI;
        }

        public List<string> GetFlightModes() => flightModes.Keys.ToList();

        public TargetData ComputeAI(TargetData targetData)
        {
            return current.ComputeAI(targetData);
        }
    }
}
