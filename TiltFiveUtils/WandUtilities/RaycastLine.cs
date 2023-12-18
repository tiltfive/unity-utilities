//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{

    [RequireComponent(typeof(WandIdent)), RequireComponent(typeof(WandAimPoseFilter)), RequireComponent(typeof(WandRaycaster))]
    [RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(GrabManager))]
    public class RaycastLine : MonoBehaviour
    {
        [Tooltip("The width of the line in real-world meters")]
        public float lineWidthMeters = 0.001f;

        private WandIdent _wandIdent;
        private WandAimPoseFilter _wandAimPoseFilter;
        private WandRaycaster _raycaster;
        private LineRenderer _lineRenderer;
        private GrabManager _grabManager;

        // Start is called before the first frame update
        void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
            _wandAimPoseFilter = GetComponent<WandAimPoseFilter>();
            _raycaster = GetComponent<WandRaycaster>();
            _lineRenderer = GetComponent<LineRenderer>();
            _grabManager = GetComponent<GrabManager>();

            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
        }

        // Update is called once per frame
        void Update()
        {
            TiltFive.PlayerIndex playerIndex = _wandIdent.playerIndex;
            TiltFive.ControllerIndex controllerIndex = _wandIdent.controllerIndex;

            if (!TiltFive.Wand.IsTracked(controllerIndex, playerIndex))
            {
                _lineRenderer.enabled = false;
                return;
            }


            float lineWidth = PlayerUtils.GetScaleToUnityFromMeters(playerIndex) * lineWidthMeters;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
            _lineRenderer.SetPosition(0, _wandAimPoseFilter.FilteredWandAimPose.position);

            _lineRenderer.enabled = true;

            Grabber grabber = _grabManager.CurrentGrabber;
            if (grabber != null)
            {
                RaycastGrabber raycastGrabber = grabber as RaycastGrabber;
                if (raycastGrabber != null)
                {
                    Transform grabbedTransform = raycastGrabber.GrabbedTransform;
                    if (grabbedTransform != null)
                    {
                        _lineRenderer.SetPosition(1, grabbedTransform.TransformPoint(raycastGrabber.LocalGrabPoint));
                        return;
                    }
                }

                // If something has done a 'grab' but there's no grab-point, don't draw the line.
                _lineRenderer.enabled = false;
                return;
            }

            RaycastHit? hit = _raycaster.GetRaycastHit();
            if (hit != null)
            {
                _lineRenderer.SetPosition(1, ((RaycastHit) hit).point);
                return;
            }

            _lineRenderer.SetPosition(1, _raycaster.RayWandPoint);


        }
    }
}