/*
Copyright (c) 2016, BahamutoD, ferram4, tetryds
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
 
* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseAimFlight
{
    public static class VectorUtils
    {
        public static Vector3d Vector3dProjectOnPlane(Vector3d vector, Vector3d planeNormal)
        {
            double projectionVal = Vector3d.Dot(vector, planeNormal) / planeNormal.sqrMagnitude;
            Vector3d projected = projectionVal * planeNormal;
            return vector - projected;
        }


        public static float SignedAngle(Vector3d fromDirection, Vector3d toDirection, Vector3d referenceRight)
        {
            double angle = Vector3d.Angle(fromDirection, toDirection);
            double sign = Math.Sign(Vector3.Dot(toDirection, referenceRight));
            double finalAngle = sign * angle;
            return (float)finalAngle;
        }



        //from howlingmoonsoftware.com
        //calculates how long it will take for a target to be where it will be when a bullet fired now can reach it.
        //delta = initial relative position, vr = relative velocity, muzzleV = bullet velocity.
        public static float CalculateLeadTime(Vector3 delta, Vector3 vr, float muzzleV)
        {
            // Quadratic equation coefficients a*t^2 + b*t + c = 0
            float a = Vector3.Dot(vr, vr) - muzzleV * muzzleV;
            float b = 2f * Vector3.Dot(vr, delta);
            float c = Vector3.Dot(delta, delta);

            float det = b * b - 4f * a * c;

            // If the determinant is negative, then there is no solution
            if (det > 0f)
            {
                return 2f * c / (Mathf.Sqrt(det) - b);
            }
            else
            {
                return -1f;
            }
        }

        /// <summary>
        /// Returns a value between -1 and 1 via Perlin noise.
        /// </summary>
        /// <returns>Returns a value between -1 and 1 via Perlin noise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public static float FullRangePerlinNoise(float x, float y)
        {
            float perlin = Mathf.PerlinNoise(x, y);

            perlin -= 0.5f;
            perlin *= 2;

            return perlin;
        }


        public static Vector3 RandomDirectionDeviation(Vector3 direction, float maxAngle)
        {
            return Vector3.RotateTowards(direction, UnityEngine.Random.rotation * direction, UnityEngine.Random.Range(0, maxAngle * Mathf.Deg2Rad), 0).normalized;
        }

        public static Vector3 WeightedDirectionDeviation(Vector3 direction, float maxAngle)
        {
            float random = UnityEngine.Random.Range(0f, 1f);
            float maxRotate = maxAngle * (random * random);
            maxRotate = Mathf.Clamp(maxRotate, 0, maxAngle) * Mathf.Deg2Rad;
            return Vector3.RotateTowards(direction, Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, direction), maxRotate, 0).normalized;
        }

        /// <summary>
        /// Converts world position to Lat,Long,Alt form.
        /// </summary>
        /// <returns>The position in geo coords.</returns>
        /// <param name="worldPosition">World position.</param>
        /// <param name="body">Body.</param>
        public static Vector3d WorldPositionToGeoCoords(Vector3d worldPosition, CelestialBody body)
        {
            if (!body)
            {
                //Debug.Log ("BahaTurret.VectorUtils.WorldPositionToGeoCoords body is null");
                return Vector3d.zero;
            }

            double lat = body.GetLatitude(worldPosition);
            double longi = body.GetLongitude(worldPosition);
            double alt = body.GetAltitude(worldPosition);
            return new Vector3d(lat, longi, alt);
        }

        public static Vector3 RotatePointAround(Vector3 pointToRotate, Vector3 pivotPoint, Vector3 axis, float angle)
        {
            Vector3 line = pointToRotate - pivotPoint;
            line = Quaternion.AngleAxis(angle, axis) * line;
            return pivotPoint + line;
        }

        public static Vector3 GetNorthVector(Vector3 position, CelestialBody body)
        {
            Vector3 geoPosA = VectorUtils.WorldPositionToGeoCoords(position, body);
            Vector3 geoPosB = new Vector3(geoPosA.x + 1, geoPosA.y, geoPosA.z);
            Vector3 north = GetWorldSurfacePostion(geoPosB, body) - GetWorldSurfacePostion(geoPosA, body);
            return Vector3.ProjectOnPlane(north, body.GetSurfaceNVector(geoPosA.x, geoPosA.y)).normalized;
        }

        public static Vector3 GetWorldSurfacePostion(Vector3d geoPosition, CelestialBody body)
        {
            if (!body)
            {
                return Vector3.zero;
            }
            return body.GetWorldSurfacePosition(geoPosition.x, geoPosition.y, geoPosition.z);
        }

        public static Vector3 GetUpDirection(Vector3 position)
        {
            return (position - FlightGlobals.currentMainBody.transform.position).normalized;
        }
    }
}
