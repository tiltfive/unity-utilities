//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{

    public class PlayerColors : MonoBehaviour
    {
        public Color[] colors = {
            Color.red,
            Color.blue,
            Color.yellow,
            Color.magenta,
        };

        public Color GetPlayerColor(TiltFive.PlayerIndex playerIndex)
        {
            switch (playerIndex)
            {
                case TiltFive.PlayerIndex.One:
                    return colors[0];
                case TiltFive.PlayerIndex.Two:
                    return colors[1];
                case TiltFive.PlayerIndex.Three:
                    return colors[2];
                case TiltFive.PlayerIndex.Four:
                    return colors[3];
            }

            throw new System.ArgumentException("Invalid playerIndex argument");
        }
    }

}