using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class ControlFilter
    {
        InputPid pitchPid;
        InputPid rollPid;
        InputPid yawPid;

        public ControlFilter()
        {
            pitchPid = new InputPid(0.2f, 0.1f, 0.08f);
            rollPid = new InputPid(0.01f, 0.0f, 0.005f);
            yawPid = new InputPid(0.035f, 0.1f, 0.04f);
        }

        public InputControls ComputeControls(TargetData targetData, float deltaTime)
        {
            return new InputControls
            {
                pitch = pitchPid.ComputeValue(targetData.pitchErr, deltaTime),
                roll = rollPid.ComputeValue(targetData.rollErr, deltaTime),
                yaw = yawPid.ComputeValue(targetData.yawErr, deltaTime)
            };
        }
    }
}
