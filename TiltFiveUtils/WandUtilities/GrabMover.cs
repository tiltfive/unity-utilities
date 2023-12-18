//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    public class GrabMover : RaycastGrabber
    {
        [Tooltip("The extension and retraction speed in real-world m/s")]
        public float extensionSpeed = 1.0f;

        [Tooltip("The maximum linear speed (in Unity Units per second) to apply to the grabbed object when using physics")]
        public float maxSpeed = 8.0f;

        [Tooltip("The maximum angular speed (in radians per second) to apply to the grabbed object when using physics")]
        public float maxAngularSpeed = 8.0f;

        [Tooltip("The deadzone to apply to the Y axis when moving objects with the joystick")]
        public float stickYDeadzone = 0.05f;

        private Pose _grabbedRelativeToGrabber;
        private Vector3 _grabDirectionRelativeToGrabber;
        private float _distanceToGrabPoint;
        private float _initialDistanceToGrabPoint;

        private Vector3 _desiredPos;
        private Quaternion _desiredRot;

        protected override void OnRaycastGrab()
        {
            Quaternion inverseInitialGrabberRot = Quaternion.Inverse(InitialGrabberPose.rotation);
            _grabbedRelativeToGrabber.position = inverseInitialGrabberRot * (InitialGrabbedPose.position - InitialGrabberPose.position);
            _grabbedRelativeToGrabber.rotation = inverseInitialGrabberRot * InitialGrabbedPose.rotation;

            Vector3 grabPointRelativeToGrabber = inverseInitialGrabberRot * (GrabPoint - InitialGrabberPose.position);
            _initialDistanceToGrabPoint = grabPointRelativeToGrabber.magnitude;
            _distanceToGrabPoint = _initialDistanceToGrabPoint;
            _grabDirectionRelativeToGrabber = grabPointRelativeToGrabber / _distanceToGrabPoint;

            _desiredPos = GrabbedTransform.position;
            _desiredRot = GrabbedTransform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (GrabbedTransform != null)
            {
                if (TiltFive.Input.TryGetStickTilt(out Vector2 stickTilt, wandIdent.controllerIndex, wandIdent.playerIndex)
                    && (stickTilt.y > stickYDeadzone || stickTilt.y < -stickYDeadzone))
                {
                    float distanceDeltaMeters = Time.deltaTime * extensionSpeed * stickTilt.y;
                    float scaleToUnityFromMeters = PlayerUtils.GetScaleToUnityFromMeters(wandIdent.playerIndex);
                    float distanceDelta = scaleToUnityFromMeters * distanceDeltaMeters;

                    float clampedDistanceDelta = System.Math.Max(distanceDelta, -_distanceToGrabPoint);
                    _distanceToGrabPoint += clampedDistanceDelta;

                    float minDistanceToGrabPoint = raycaster.distanceFromWandAim
                        * PlayerUtils.GetScaleToUnityFromMeters(wandIdent.playerIndex);
                    _distanceToGrabPoint = System.Math.Max(minDistanceToGrabPoint, _distanceToGrabPoint);
                }

                Vector3 grabberPosition = wandAimPoseFilter.FilteredWandAimPose.position;
                Quaternion grabberRotation = wandAimPoseFilter.FilteredWandAimPose.rotation;
                Quaternion desiredRot = grabberRotation * _grabbedRelativeToGrabber.rotation;
                Vector3 extensionDirection = grabberRotation * _grabDirectionRelativeToGrabber;
                Vector3 desiredPos = grabberPosition + grabberRotation * _grabbedRelativeToGrabber.position
                    + (_distanceToGrabPoint - _initialDistanceToGrabPoint) * extensionDirection;

                if (usePhysics)
                {
                    _desiredPos = desiredPos;
                    _desiredRot = desiredRot;
                }
                else
                {
                    GrabbedTransform.SetPositionAndRotation(desiredPos, desiredRot);
                }
            }
        }

        private void FixedUpdate()
        {
            if (GrabbedRigidBody != null && usePhysics)
            {
                Vector3 velocity = (_desiredPos - GrabbedTransform.position) / Time.deltaTime;
                if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
                {
                    velocity *= maxSpeed / velocity.magnitude;
                }
                GrabbedRigidBody.velocity = Vector3.zero;
                GrabbedRigidBody.AddForce(velocity, ForceMode.VelocityChange);

                Quaternion deltaRot = Quaternion.Inverse(GrabbedTransform.rotation) * _desiredRot;
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);

                float angularSpeed = Mathf.Clamp(angle * (Mathf.PI / 180.0f) / Time.deltaTime, -maxAngularSpeed, maxAngularSpeed);
                Vector3 angularVelocity = GrabbedTransform.TransformDirection(axis) * angularSpeed;

                GrabbedRigidBody.angularVelocity = Vector3.zero;
                GrabbedRigidBody.AddTorque(angularVelocity, ForceMode.VelocityChange);
            }
        }
    }
}