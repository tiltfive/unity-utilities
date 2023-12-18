//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public class ObjectRotatorHelper
    {
        public enum RotationStyle
        {
            Orientation,
            Position,
        }

        private Transform grabbedTransform;
        private Vector3 _centerOfRotation;
        private Pose _initialGrabbedPose;
        private Pose _initialGrabberPose;
        private Vector3 _initialCorToWandAimRelativeToGrabbed;
        private Quaternion _prevGrabberRotation;
        private float _accumulatedGrabberRoll = 0.0f;

        private RotationStyle _rotationStyle = RotationStyle.Position;
        private float _rotationFactor = 0.0f;

        public void BeginRotation(Vector3 centerOfRotation, Pose initialGrabbedPose, Pose initialGrabberPose,
                                  RotationStyle rotationStyle, float rotationFactor)
        {
            _centerOfRotation = centerOfRotation;
            _initialGrabbedPose = initialGrabbedPose;
            _initialGrabberPose = initialGrabberPose;
            _prevGrabberRotation = initialGrabberPose.rotation;
            _accumulatedGrabberRoll = 0.0f;

            _rotationStyle = rotationStyle;
            _rotationFactor = rotationFactor;

            _initialCorToWandAimRelativeToGrabbed = Quaternion.Inverse(initialGrabbedPose.rotation)
                * (initialGrabberPose.position - centerOfRotation).normalized;
        }

        public Pose ComputeDesiredPose(Pose grabberTransform)
        {
            Quaternion rotToWRLD_DD = Quaternion.identity;
            Quaternion rotToWRLD_ID = _initialGrabbedPose.rotation;

            switch (_rotationStyle)
            {
                case RotationStyle.Position:
                    {
                        Quaternion rotToID_WRLD = Quaternion.Inverse(rotToWRLD_ID);
                        Vector3 centerOfRotation_ID = rotToID_WRLD * _centerOfRotation;
                        Vector3 initialGrabber_ID = rotToID_WRLD * _initialGrabberPose.position;
                        Vector3 currentGrabber_ID = rotToID_WRLD * grabberTransform.position;

                        Vector3 corToInitialGrabber_ID = initialGrabber_ID - centerOfRotation_ID;
                        Vector3 corToCurrentGrabber_ID = currentGrabber_ID - centerOfRotation_ID;

                        Quaternion unscaledOffAxisRotToID_DD = Quaternion.FromToRotation(corToInitialGrabber_ID,
                            corToCurrentGrabber_ID);
                        Quaternion offAxisRotToID_DD = RotationUtils.ScaleRotation(_rotationFactor,
                            unscaledOffAxisRotToID_DD);

                        Quaternion rotToWRLD_PR = _prevGrabberRotation;
                        Quaternion rotToWRLD_DR = grabberTransform.rotation;
                        Quaternion rotToDR_PR = Quaternion.Inverse(rotToWRLD_DR) * rotToWRLD_PR;
                        _accumulatedGrabberRoll += RotationUtils.RotationComponentAbout(rotToDR_PR, Vector3.forward);

                        Vector3 corToWandAim_ID = _initialCorToWandAimRelativeToGrabbed;
                        Quaternion onAxisRotToID_DD = Quaternion.AngleAxis(_accumulatedGrabberRoll * _rotationFactor,
                            corToWandAim_ID);
                        rotToWRLD_DD = rotToWRLD_ID * offAxisRotToID_DD * onAxisRotToID_DD;

                        _prevGrabberRotation = rotToWRLD_DR;
                    }
                    break;

                // Reference frames:
                // WRLD: Unity World-space
                // DR: Desired grabbeR
                // IR: Initial grabbeR
                // DD: Desired grabbeD
                // ID: Initial grabbeD
                //
                // Decl: rotToDR_DD == rotToIR_ID
                // Want: rotToWRLD_DD
                //
                // Decl: posCenterOfRot_DD == posCenterOfRot_ID
                // Want: posDD_WRLD

                case RotationStyle.Orientation:
                    {
                        Quaternion rotToWRLD_IR = _initialGrabberPose.rotation;
                        Quaternion rotToWRLD_DR = grabberTransform.rotation;
                        Quaternion rotToDR_IR = Quaternion.Inverse(rotToWRLD_DR) * rotToWRLD_IR;
                        Quaternion scaledRotToDR_IR = RotationUtils.ScaleRotation(_rotationFactor, rotToDR_IR);
                        Quaternion rotToIR_ID = Quaternion.Inverse(rotToWRLD_IR) * rotToWRLD_ID;
                        Quaternion rotToDR_DD = rotToIR_ID;
                        Quaternion rotToDD_ID = Quaternion.Inverse(rotToDR_DD) * scaledRotToDR_IR * rotToIR_ID;
                        rotToWRLD_DD = rotToWRLD_ID * Quaternion.Inverse(rotToDD_ID);
                    }
                    break;
            }

            Quaternion desiredRot = rotToWRLD_DD;
            Vector3 desiredPos = _initialGrabbedPose.position;

            if (_centerOfRotation != desiredPos)
            {
                Vector3 posCenterOfRot_WRLD = _centerOfRotation;
                Vector3 posID_WRLD = _initialGrabbedPose.position;
                Vector3 posCenterOfRot_ID = Quaternion.Inverse(rotToWRLD_ID) * (posCenterOfRot_WRLD - posID_WRLD);
                Vector3 posCenterOfRot_DD = posCenterOfRot_ID;
                Vector3 ddToCenterOfRot_WRLD = rotToWRLD_DD * posCenterOfRot_DD;
                Vector3 posDD_WRLD = posCenterOfRot_WRLD - ddToCenterOfRot_WRLD;

                desiredPos = posDD_WRLD;
            }

            return new Pose(desiredPos, desiredRot);
        }
    }
}