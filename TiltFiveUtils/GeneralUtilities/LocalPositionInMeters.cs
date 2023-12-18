//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    public class LocalPositionInMeters : MonoBehaviour
    {
        [Tooltip("The player whose gameboard scale should be used to convert from meters to Unity world units")]
        TiltFive.PlayerIndex playerIndex = TiltFive.PlayerIndex.One;

        [Tooltip("The local position in real-world meters")]
        public Vector3 localPositionInMeters = Vector3.zero;

        void Update()
        {
            if (playerIndex != TiltFive.PlayerIndex.None)
            {
                float scaleToUnityFromMeters = PlayerUtils.GetScaleToUnityFromMeters(playerIndex);
                transform.localPosition = scaleToUnityFromMeters * localPositionInMeters;
            }
        }
    }

}