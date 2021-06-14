using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class InputPid
    {
        float kp, ki, kd;

        float previousErr;

        public InputPid(float kp, float ki, float kd)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
        }

        public float ComputeValue(float error, float deltaTime)
        {
            float direct = error * kp;
            float derivErr = (error - previousErr) / deltaTime;
            float derivative = derivErr * kd;

            float value = 0;
            value += direct;
            //value += derivative;
            //value += integral;

            previousErr = error;

            return value;
        }
    }
}
