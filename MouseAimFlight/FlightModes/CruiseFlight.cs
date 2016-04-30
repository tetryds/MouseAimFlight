using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    class CruiseFlight : Flight
    {
        private static string flightMode = "Cruise Flight";

        public CruiseFlight()
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

            targetDirection = vesselTransform.InverseTransformDirection(targetPosition - velocityTransform.position).normalized;
            targetDirectionYaw = targetDirection;
            
            float pitchError;
            float rollError;
            float yawError;

            float sideslip;

            Vector3d target = (targetPosition - velocityTransform.position).normalized;

            sideslip = (float)Math.Asin(Vector3.Dot(vesselTransform.right, vessel.srf_velocity.normalized)) * Mathf.Rad2Deg;
            
            pitchError = ((float)Math.Acos(Vector3.Dot(vesselTransform.up, vessel.upAxis)) - (float)Math.Acos(Vector3.Dot(target, vessel.upAxis))) * Mathf.Rad2Deg;

            yawError = (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            rollTarget = (targetPosition + Mathf.Clamp(upWeighting * (100f - Math.Abs(yawError * 2.5f)), 0, float.PositiveInfinity) * upDirection) - vessel.CoM;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right) - sideslip * (float)Math.Sqrt(vessel.srf_velocity.magnitude) / 5;

            float pitchDownFactor = pitchError * (100 / ((float)Math.Pow(yawError, 2) + 100));
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
