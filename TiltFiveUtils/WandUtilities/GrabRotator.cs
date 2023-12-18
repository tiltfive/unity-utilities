//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(WandIdent)), RequireComponent(typeof(WandAimPoseFilter))]
    public class GrabRotator : Grabber
    {
        [Tooltip("The object that is to be rotated")]
        public Transform targetObject;

        [Tooltip("The style of rotation interaction")]
        public ObjectRotatorHelper.RotationStyle rotationStyle = ObjectRotatorHelper.RotationStyle.Position;

        [Tooltip("The amplification factor applied to the input rotation")]
        public float rotationFactor = 1.0f;

        private WandIdent _wandIdent;
        private WandAimPoseFilter _wandAimPoseFilter;

        private readonly ObjectRotatorHelper _objectRotatorHelper = new ObjectRotatorHelper();
        private bool _isGrabbed = false;

        public void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
            _wandAimPoseFilter = GetComponent<WandAimPoseFilter>();
        }

        public sealed override bool TryGrab()
        {
            Pose initialGrabbedPose = new Pose(targetObject.position, targetObject.rotation);
            Pose initialGrabberPose = _wandAimPoseFilter.FilteredWandAimPose;

            _objectRotatorHelper.BeginRotation(initialGrabbedPose.position, initialGrabbedPose,
                                               initialGrabberPose, rotationStyle, rotationFactor);

            _isGrabbed = true;

            return true;
        }

        public override void OnGrabRelease()
        {
            _isGrabbed = false;
        }

        void Update()
        {
            if (_isGrabbed)
            {
                Pose desiredPose = _objectRotatorHelper.ComputeDesiredPose(_wandAimPoseFilter.FilteredWandAimPose);
                targetObject.SetPositionAndRotation(desiredPose.position, desiredPose.rotation);
            }
        }
    }
}