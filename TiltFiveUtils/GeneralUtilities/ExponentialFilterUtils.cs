//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public class ExponentialFilterUtils
    {
        const float SecondsPerMillisecond = 1.0f / 1000.0f;
        const float MillisecondsPerSecond = 1000.0f;

        public static float ComputeDecayFactor(float filterConstant)
        {
            return Mathf.Exp(Time.deltaTime * ExponentialFilterUtils.MillisecondsPerSecond * filterConstant);
        }

        public static float ComputeFilterConstant(float cutoffFreqHz)
        {
            float x = 1.0f - Mathf.Cos(2.0f * Mathf.PI * cutoffFreqHz * SecondsPerMillisecond);
            float y = Mathf.Sqrt(x * x + 2.0f * x) - x;
            float filterConstant = Mathf.Log(1.0f - y);
            return filterConstant;
        }
    }

}