//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(PlayerIdent))]
    public class GlassesPoseFilter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The 3dB attenuation cutoff frequency for the low-pass filter applied to glasses poses")]
        private float _filterCutoffHz = 12.0f;

        private PlayerIdent _playerIdent;

        private float _filterConstant = 0.0f;

        public Pose FilteredGlassesPose { get; private set; }

        private void Awake()
        {
            _playerIdent = GetComponent<PlayerIdent>();
            _filterConstant = ExponentialFilterUtils.ComputeFilterConstant(_filterCutoffHz);
        }

        private void Update()
        {
            float decayFactor = ExponentialFilterUtils.ComputeDecayFactor(_filterConstant);

            if (TiltFive.Glasses.TryGetPose(_playerIdent.playerIndex, out Pose pose))
            {
                Vector3 filteredPosition = Vector3.Lerp(FilteredGlassesPose.position, pose.position, 1.0f - decayFactor);
                Quaternion filteredRotation = Quaternion.Slerp(FilteredGlassesPose.rotation, pose.rotation, 1.0f - decayFactor);
                FilteredGlassesPose = new Pose(filteredPosition, filteredRotation);
            }
        }
    }

}