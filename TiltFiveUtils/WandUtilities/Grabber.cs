//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(WandIdent))]
    public abstract class Grabber : MonoBehaviour
    {
        public abstract bool TryGrab();
        public abstract void OnGrabRelease();

        public virtual void OnSecondaryGrab(GrabButton button) { }
        public virtual void OnSecondaryGrabRelease(GrabButton button) { }

    }
}