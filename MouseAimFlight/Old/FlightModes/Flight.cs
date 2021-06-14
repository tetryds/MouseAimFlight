using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    abstract class Flight //This will inherit from a virtual base class
    {
        abstract public ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel);

        abstract public string GetFlightMode();
    }
}
