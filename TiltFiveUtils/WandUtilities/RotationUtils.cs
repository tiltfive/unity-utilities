//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    class RotationUtils
    {
        public static Quaternion ScaleRotation(float scale, Quaternion q)
        {
            q.ToAngleAxis(out float angle, out Vector3 axis);
            return Quaternion.AngleAxis(scale * angle, axis);
        }

        // axis must be unit length
        public static float RotationComponentAbout(Quaternion q, Vector3 aboutAxis)
        {
            Vector3 rotationAxis = new Vector3(q.x, q.y, q.z);
            Vector3 projectedRotationAxis = Vector3.Project(rotationAxis, aboutAxis);
            Quaternion r = new Quaternion(projectedRotationAxis.x, projectedRotationAxis.y, projectedRotationAxis.z, q.w);
            r.Normalize();
            r.ToAngleAxis(out float angle, out Vector3 axis);
            if (Vector3.Dot(axis, aboutAxis) < 0)
            {
                return -angle;
            }
            return angle;
        }
    }
}