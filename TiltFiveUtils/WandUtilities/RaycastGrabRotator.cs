//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{

    public class RaycastGrabRotator : RaycastGrabber
    {
        public enum RotateAbout
        {
            LocalOrigin,
            ColliderBoundsCenter,
            GrabPoint,
        }

        [Tooltip("The point about which to rotate objects")]
        public RotateAbout rotateAbout = RotateAbout.GrabPoint;

        [Tooltip("The style of rotation interaction")]
        public ObjectRotatorHelper.RotationStyle rotationStyle = ObjectRotatorHelper.RotationStyle.Position;

        [Tooltip("The amplification factor applied to the input rotation")]
        public float rotationFactor = 1.0f;

        private readonly ObjectRotatorHelper _objectRotatorHelper = new ObjectRotatorHelper();

        protected override void OnRaycastGrab()
        {
            Vector3 centerOfRotation = Vector3.zero;
            switch (rotateAbout)
            {
                case RotateAbout.LocalOrigin:
                    centerOfRotation = InitialGrabbedPose.position;
                    break;

                case RotateAbout.ColliderBoundsCenter:
                    centerOfRotation = GrabbedCollider.bounds.center;
                    break;

                case RotateAbout.GrabPoint:
                    centerOfRotation = GrabPoint;
                    break;
            }

            LocalGrabPoint = GrabbedTransform.InverseTransformPoint(centerOfRotation);

            _objectRotatorHelper.BeginRotation(centerOfRotation, InitialGrabbedPose, InitialGrabberPose, rotationStyle, rotationFactor);
        }

        void Update()
        {
            if (GrabbedCollider != null)
            {
                Pose desiredPose = _objectRotatorHelper.ComputeDesiredPose(wandAimPoseFilter.FilteredWandAimPose);
                GrabbedTransform.SetPositionAndRotation(desiredPose.position, desiredPose.rotation);
            }
        }
    }
}