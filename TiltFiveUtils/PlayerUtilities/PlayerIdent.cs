//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public class PlayerIdent : MonoBehaviour
    {
        [Tooltip("The player that this GameObject is associated with")]
        public TiltFive.PlayerIndex playerIndex;

#if (ENABLE_INPUT_SYSTEM)
        public UnityEngine.InputSystem.PlayerInput playerInput;

        private void Awake()
        {
            if (playerInput != null)
            {
                switch (playerInput.playerIndex)
                {
                    case 0:
                        playerIndex = TiltFive.PlayerIndex.One;
                        break;

                    case 1:
                        playerIndex = TiltFive.PlayerIndex.Two;
                        break;

                    case 2:
                        playerIndex = TiltFive.PlayerIndex.Three;
                        break;

                    case 3:
                        playerIndex = TiltFive.PlayerIndex.Four;
                        break;

                    default:
                        playerIndex = TiltFive.PlayerIndex.None;
                        break;
                }
            }
        }
#endif // ENABLE_INPUT_SYSTEM
    }
}