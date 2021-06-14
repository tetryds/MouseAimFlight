/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MouseAimFlight
{
    class PID
    {
        public float kp, ki, kd;
        float initKp, initKi, initKd;

        float integral;
        public bool IntegralZeroed
        {
            get { return integral == 0; }
        }

        public PID(float initKp, float initKi, float initKd)
        {
            this.initKp = initKp;
            this.initKi = initKi;
            this.initKd = initKd;
            kp = initKp;
            ki = initKi;
            kd = initKd;

            integral = 0;
        }

        public void UpdateGains(float kp, float ki, float kd)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
        }

        public float Simulate(float error, float derivError, float integralLimit, float timeStep, float speedFactor)
        {
            //Setup
            float output = 0;

            integral += error * timeStep;

            if (ki != 0)
                Clamp(ref integral, integralLimit / (ki * speedFactor)); //limits outputI to integralLimit
            else
                ZeroIntegral();

            //Computing the outputs
            output += error * kp; //Proportional
            output += derivError * kd; //Derivative

            output *= speedFactor; //Speed factor

            if (output >= 1)
                ZeroIntegral();

            output += integral * ki * speedFactor; //Integral with speed factor

            Clamp(ref output, 1);

            return output;
        }

        public void Clamp(ref float value, float limit)
        {
            if (value > limit)
                value = limit;
            if (value < -limit)
                value = -limit;
        }

        public void ZeroIntegral()
        {
            integral = 0;
        }
    }
}
