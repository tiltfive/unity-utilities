//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{

    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(WandIdent))]
    public class WandAimPoseFilter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The 3dB attenuation cutoff frequency for the low-pass filter applied to wand poses")]
        private float _filterCutoffHz = 12.0f;

        private WandIdent _wandIdent;

        private float _filterConstant = 0.0f;

        public Pose FilteredWandAimPose { get; private set; }

        private void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
            _filterConstant = ExponentialFilterUtils.ComputeFilterConstant(_filterCutoffHz);
        }

        private void Update()
        {
            float decayFactor = ExponentialFilterUtils.ComputeDecayFactor(_filterConstant);

            Vector3 aimPosition = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, TiltFive.ControllerPosition.Aim,
                _wandIdent.playerIndex);
            Vector3 filteredAimPosition = Vector3.Lerp(FilteredWandAimPose.position, aimPosition, 1.0f - decayFactor);

            Quaternion aimRotation = TiltFive.Wand.GetRotation(_wandIdent.controllerIndex, _wandIdent.playerIndex);
            Quaternion filteredAimRotation = Quaternion.Slerp(FilteredWandAimPose.rotation, aimRotation, 1.0f - decayFactor);

            FilteredWandAimPose = new Pose(filteredAimPosition, filteredAimRotation);
        }
    }

}