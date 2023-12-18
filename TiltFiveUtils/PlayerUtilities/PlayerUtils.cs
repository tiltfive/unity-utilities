//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿namespace TiltFiveUtils
{
    public class PlayerUtils
    {
        public static float GetScaleToUnityFromMeters(TiltFive.PlayerIndex playerIndex)
        {
            if (TiltFive.Player.TryGetSettings(playerIndex, out TiltFive.PlayerSettings playerSettings))
            {
                return playerSettings.scaleSettings.GetScaleToUWRLD_UGBD(playerSettings.gameboardSettings.gameBoardScale);
            }
            else
            {
                return 1.0f;
            }
        }
    }
}