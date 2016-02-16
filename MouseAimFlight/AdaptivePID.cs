﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseAimFlight
{
    class AdaptivePID
    {
        public PID pitchPID;
        public PID rollPID;
        public PID yawPID;

        float pitchP = 0.2f, pitchI = 0.1f, pitchD = 0.08f;
        float rollP = 0.01f, rollI = 0.001f, rollD = 0.005f;
        float yawP = 0.035f, yawI = 0.1f, yawD = 0.04f;
        float upWeighting = 3f; //TODO: update external upweighting

        float pIntLimt = 0.2f, rIntLimit = 0.2f, yIntLimit = 0.2f; //initialize integral limits at 0.2

        float adaptationCoefficient;

        public AdaptivePID()
        {
            pitchPID = new PID(pitchP, pitchI, pitchD);
            rollPID = new PID(rollP, rollI, rollD);
            yawPID = new PID(yawP, yawI, yawD);
        }

        //The constructor below is an abomination and will be nuked as soon as the GUI as it is gets removed.
        public AdaptivePID(float pP, float pI, float pD, float rP, float rI, float rD, float yP, float yI, float yD)
        {
            pitchPID = new PID(pP, pI, pD);
            rollPID = new PID(rP, rI, rD);
            yawPID = new PID(yP, yI, yD);
        }

        public Steer Simulate(float pitchError, float rollError, float yawError, UnityEngine.Vector3 angVel, float altitude, float timestep)
        {
            float steerPitch = pitchPID.Simulate(pitchError, angVel.x, pIntLimt, timestep);
            float steerRoll = rollPID.Simulate(rollError, angVel.y, rIntLimit, timestep);
            float steerYaw = yawPID.Simulate(yawError, angVel.z, yIntLimit, timestep);

            Steer steer = new Steer (steerPitch, steerRoll, steerYaw);

            return steer;
        }
        
        void AdaptGains(float timeStep)
        {
            //There will be some cool code in here in the future.
        }

    }
    public struct Steer
    {
        public float pitch;
        public float roll;
        public float yaw;

        public Steer(float p, float r, float y)
        {
            pitch = p;
            roll = r;
            yaw = y;
        }
    }
}
