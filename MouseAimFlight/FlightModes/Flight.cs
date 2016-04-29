using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    class Flight //This will inherit from a virtual base class
    {
        private static string flightMode = "No Mode"; //Name of the behavior

        public Flight()
        {

        }

        public virtual ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            ErrorData behavior = new ErrorData(0, 0, 0);
            return behavior;
        }

        public virtual string GetFlightMode()
        {
            return flightMode;
        }
    }
}
