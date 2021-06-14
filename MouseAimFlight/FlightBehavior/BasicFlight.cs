using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class BasicFlight : IFlightAI
    {
        public TargetData ComputeAI(TargetData targetData)
        {
            float pitchErr = targetData.pitchErr;
            float yawErr = targetData.yawErr;

            float rollYawScale = Mathf.Sqrt(Mathf.Abs(yawErr));
            float rollErr = targetData.rollErr * rollYawScale;

            return new TargetData() { pitchErr = pitchErr, rollErr = rollErr, yawErr = yawErr };
        }
    }
}
