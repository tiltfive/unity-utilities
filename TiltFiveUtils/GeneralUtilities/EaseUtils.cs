//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public class EaseUtils
    {
        public static float EaseIn(float t)
        {
            return t * t;
        }

        public static float EaseOut(float t)
        {
            float u = 1.0f - t;
            return (1.0f - (u * u));
        }

        public static float EaseInOut(float t)
        {
            return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
        }
    }
}
