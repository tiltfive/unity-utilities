//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(WandRaycaster))]
    public abstract class RaycastGrabber : Grabber
    {
        [Tooltip("Whether grabbed objects should be moved using physics")]
        public bool usePhysics = false;

        protected WandIdent wandIdent;
        protected WandAimPoseFilter wandAimPoseFilter;
        protected WandRaycaster raycaster;

        public Transform GrabbedTransform { get; private set ; }
        public Collider GrabbedCollider { get; private set; }
        public Rigidbody GrabbedRigidBody { get; private set; }
        public Vector3 GrabPoint { get; protected set; }
        public Vector3 LocalGrabPoint { get; protected set; }
        public Pose InitialGrabbedPose { get; private set; }
        public Pose InitialGrabberPose { get; private set; }

        private bool _grabbedUseGravity = false;
        private bool _grabbedIsKinematic = false;
        private CollisionDetectionMode _grabbedDetectionMode = CollisionDetectionMode.Discrete;

        public void Awake()
        {
            wandIdent = GetComponent<WandIdent>();
            wandAimPoseFilter = GetComponent<WandAimPoseFilter>();
            raycaster = GetComponent<WandRaycaster>();
        }

        public sealed override bool TryGrab() {
            RaycastHit? hit = raycaster.GetRaycastHit();
            if (hit != null)
            {
                Rigidbody rigidbody = hit.Value.rigidbody;
                if (usePhysics)
                {
                    if (rigidbody == null)
                    {
                        // We can only grab objects with a RigidBody component.
                        return false;
                    }

                    _grabbedUseGravity = rigidbody.useGravity;
                    rigidbody.useGravity = false;

                    _grabbedDetectionMode = rigidbody.collisionDetectionMode;
                    //rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                } else if (rigidbody != null)
                {
                    _grabbedIsKinematic = rigidbody.isKinematic;
                }

                OnRaycastHit(hit.Value);
                return true;
            }

            return false;
        }

        protected virtual void OnRaycastGrab() { }
        protected virtual void OnRaycastGrabRelease() { }

        public sealed override void OnGrabRelease()
        {
            OnRaycastGrabRelease();
            if (usePhysics)
            {
                GrabbedRigidBody.useGravity = _grabbedUseGravity;
                GrabbedRigidBody.collisionDetectionMode = _grabbedDetectionMode;
            }
            else if (GrabbedRigidBody != null)
            {
                GrabbedRigidBody.isKinematic = _grabbedIsKinematic;
            }
            GrabbedTransform = null;
            GrabbedCollider = null;
            GrabbedRigidBody = null;
            GrabPoint = Vector3.zero;
            LocalGrabPoint = Vector3.zero;
            InitialGrabbedPose = new Pose();
            InitialGrabberPose = new Pose();
        }

        private void OnRaycastHit(RaycastHit hit)
        {
            GrabbedTransform = hit.transform;
            GrabbedCollider = hit.collider;
            GrabbedRigidBody = hit.rigidbody;
            GrabPoint = hit.point;
            LocalGrabPoint = hit.transform.InverseTransformPoint(hit.point);
            InitialGrabbedPose = new Pose(hit.transform.position, hit.transform.rotation);
            InitialGrabberPose = wandAimPoseFilter.FilteredWandAimPose;
            OnRaycastGrab();
        }
    }
}