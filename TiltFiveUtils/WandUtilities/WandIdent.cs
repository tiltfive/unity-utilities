//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public class WandIdent : PlayerIdent
    {
        [Tooltip("The controller that this GameObject is associated with")]
        public TiltFive.ControllerIndex controllerIndex;
    }
}