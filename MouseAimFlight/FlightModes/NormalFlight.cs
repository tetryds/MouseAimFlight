using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    class NormalFlight : Flight
    {
        private static string flightMode = "Normal Flight";

        public NormalFlight()
        {

        }

        public override ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            Vector3d srfVel = vessel.srf_velocity;
            if (srfVel != Vector3d.zero)
            {
                velocityTransform.rotation = Quaternion.LookRotation(srfVel, -vesselTransform.forward);
            }
            velocityTransform.rotation = Quaternion.AngleAxis(90, velocityTransform.right) * velocityTransform.rotation;
            Vector3 localAngVel = vessel.angularVelocity * Mathf.Rad2Deg;

            Vector3d targetDirection;
            Vector3d targetDirectionYaw;

            targetDirection = vesselTransform.InverseTransformDirection(targetPosition - vessel.CurrentCoM).normalized;
            targetDirectionYaw = targetDirection;

            float pitchError;
            float rollError;
            float yawError;

            float sideslip;

            sideslip = (float)Math.Asin(Vector3.Dot(vesselTransform.right, vessel.srf_velocity.normalized)) * Mathf.Rad2Deg;

            pitchError = (float)Math.Asin(Vector3d.Dot(Vector3d.back, VectorUtils.Vector3dProjectOnPlane(targetDirection, Vector3d.right))) * Mathf.Rad2Deg;
            yawError = (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            rollTarget = (targetPosition + Mathf.Clamp(upWeighting * (100f - Math.Abs(yawError * 1.6f) - (pitchError * 2.8f)), 0, float.PositiveInfinity) * upDirection) - vessel.CoM;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right) - sideslip * (float)Math.Sqrt(vessel.srf_velocity.magnitude) / 5;

            float pitchDownFactor = pitchError * (10 / ((float)Math.Pow(yawError, 2) + 10f) - 0.1f);
            rollError -= Mathf.Clamp(pitchDownFactor, -15, 0);

            ErrorData behavior = new ErrorData(pitchError, rollError, yawError);

            return behavior;
        }

        public override string GetFlightMode()
        {
            return flightMode;
        }
    }
}
