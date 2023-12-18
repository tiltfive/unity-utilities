//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TiltFiveUtils
{

    [DefaultExecutionOrder(-199)]
    [RequireComponent(typeof(WandIdent)), RequireComponent(typeof(WandAimPoseFilter)), RequireComponent(typeof(GlassesPoseFilter))]
    public class WandRaycaster : MonoBehaviour
    {
        public enum RaycastDirection
        {
            Forward,
            ViewDirection,
        }

        [Tooltip("The mode determining in which direction the picking ray is cast")]
        public RaycastDirection raycastDirection = RaycastDirection.Forward;

        [Tooltip("The distance in real-world meters beyond the wand aim point from which to cast the ray")]
        public float distanceFromWandAim = 0.03f;

        [Tooltip("The radius of the sphere in real-world meters that is to be cast when the initial ray cast hits nothing")]
        public float sphereCastRadius = 0.02f;

        [Tooltip("The maximum distance at which an object may be picked")]
        public float maxPickDistance = 100.0f;

        [Tooltip("The layers from which objects can be picked")]
        public LayerMask pickLayerMask = ~0;

        [Tooltip("The amount of time in seconds that a raycast hit is reused if no other hit is made")]
        public float hitStickyDuration = 0.35f;

        [Tooltip("The angle below which a raycast hit is reused regardless of the time")]
        public float hitStickyAngleThreshold = 15.0f;

        private WandIdent _wandIdent;
        private WandAimPoseFilter _wandAimPoseFilter;
        private GlassesPoseFilter _glassesPoseFilter;

        private RaycastHit? _cachedRaycastHit;
        private int _cachedRaycastHitFrameNum = -1;
        private float _cachedRaycastHitTime = 0.0f;

        public Vector3 RayWandPoint { get; private set; }

        void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
            _wandAimPoseFilter = GetComponent<WandAimPoseFilter>();
            _glassesPoseFilter = GetComponent<GlassesPoseFilter>();
        }

        private void Update()
        {
            float distanceInUnityUnits = distanceFromWandAim * PlayerUtils.GetScaleToUnityFromMeters(_wandIdent.playerIndex);
            RayWandPoint = _wandAimPoseFilter.FilteredWandAimPose.position
                + distanceInUnityUnits * _wandAimPoseFilter.FilteredWandAimPose.forward;
        }

        public RaycastHit? GetRaycastHit()
        {
            TiltFive.PlayerIndex playerIndex = _wandIdent.playerIndex;
            TiltFive.ControllerIndex controllerIndex = _wandIdent.controllerIndex;

            if (!TiltFive.Wand.IsTracked(controllerIndex, playerIndex))
            {
                return null;
            }

            if (Time.frameCount != _cachedRaycastHitFrameNum)
            {
                Vector3 rayOrigin = ComputeRayOrigin();
                Vector3 pickDirection = ComputePickDirection();
                if (rayOrigin != Vector3.zero && pickDirection != Vector3.zero)
                {
                    Ray ray = new Ray(rayOrigin, pickDirection);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, maxPickDistance, pickLayerMask))
                    {
                        _cachedRaycastHit = hit;
                        _cachedRaycastHitTime = Time.time;
                    }
                    else if (ShouldReuseCachedHit(pickDirection))
                    {
                        // Do nothing.
                    }
                    else if (Physics.SphereCast(ray,
                                                sphereCastRadius * PlayerUtils.GetScaleToUnityFromMeters(_wandIdent.playerIndex),
                                                out hit, maxPickDistance, pickLayerMask))
                    {
                        _cachedRaycastHit = hit;
                        _cachedRaycastHitTime = Time.time;
                    }
                    else
                    {
                        _cachedRaycastHit = null;
                    }
                }
                else
                {
                    if (!ShouldReuseCachedHit(pickDirection))
                    {
                        _cachedRaycastHit = null;
                    }
                }

                _cachedRaycastHitFrameNum = Time.frameCount;
            }

            return _cachedRaycastHit;
        }

        private Vector3 ComputeRayOrigin()
        {
            switch (raycastDirection)
            {
                case RaycastDirection.Forward:
                    return RayWandPoint;

                case RaycastDirection.ViewDirection:
                    return 0.5f * (RayWandPoint + _glassesPoseFilter.FilteredGlassesPose.position);
            }

            return Vector3.zero;
        }

        private Vector3 ComputePickDirection()
        {
            switch (raycastDirection)
            {
                case RaycastDirection.Forward:
                    Quaternion wandRot = TiltFive.Wand.GetRotation();
                    return wandRot * Vector3.forward;

                case RaycastDirection.ViewDirection:
                    return Vector3.Normalize(RayWandPoint - _glassesPoseFilter.FilteredGlassesPose.position);
            }

            return Vector3.zero;
        }

        private bool ShouldReuseCachedHit(Vector3 pickDirection)
        {
            bool isAngleOk = false;
            if (_cachedRaycastHit != null)
            {
                Vector3 cachedHitDirection = _cachedRaycastHit.Value.point - RayWandPoint;
                float angle = Vector3.Angle(cachedHitDirection, pickDirection);
                isAngleOk = angle < hitStickyAngleThreshold;
            }
            bool isDurationOk = Time.time - _cachedRaycastHitTime <= hitStickyDuration;

            return isAngleOk || isDurationOk;
        }
    }
}