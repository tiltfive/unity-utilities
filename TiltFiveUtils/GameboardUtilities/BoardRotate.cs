//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(PlayerIdent))]
    public class BoardRotate : MonoBehaviour
    {
        public enum EaseFunction
        {
            Lerp,
            EaseIn,
            EaseOut,
            EaseInOut,
        }

        public enum RotationTrigger
        {
            Continuous,
            OneShot,
            OneShotOnEnable,
        }

        [Tooltip("The easing function to use for transitioning between facing directions")]
        public EaseFunction easeFunction = EaseFunction.EaseInOut;

        [Tooltip("The rotation transition duration in seconds"), Min(0.001f)]
        public float transitionDuration = 0.4f;

        [Tooltip("The threshold angle in degrees that triggers a rotation")]
        public float rotationThreshold = 60.0f;

        [Tooltip("The radius in real-world meters of the circular zone centered at the gameboard center in which rotations will not be triggered")]
        public float deadZoneRadius = 0.4f;

        [Tooltip("When the gameboard rotation should be performed")]
        public RotationTrigger rotationTrigger = RotationTrigger.Continuous;

        public Quaternion rotation
        {
            get
            {
                Quaternion rot = transform.rotation;

                if (_playerIdent.playerIndex != TiltFive.PlayerIndex.None)
                {
                    float deltaAngle = _currentAngle;
                    if (gameObject == playerSettings.gameboardSettings.currentGameBoard.gameObject)
                    {
                        // We're attached to the gameboard itself, so we need to reverse all rotations that we apply.
                        deltaAngle = -deltaAngle;
                    }
                    rot *= Quaternion.AngleAxis(deltaAngle, playerSettings.gameboardSettings.currentGameBoard.transform.up);
                }

                return rot;
            }

            set
            {
                transform.rotation = value;
                ApplyCurrentAngle();
            }
        }

        public Quaternion localRotation
        {
            get
            {
                Quaternion rot = transform.localRotation;

                if (_playerIdent.playerIndex != TiltFive.PlayerIndex.None)
                {
                    float deltaAngle = _currentAngle;
                    if (gameObject == playerSettings.gameboardSettings.currentGameBoard.gameObject)
                    {
                        // We're attached to the gameboard itself, so we need to reverse all rotations that we apply.
                        deltaAngle = -deltaAngle;
                    }
                    Vector3 up = playerSettings.gameboardSettings.currentGameBoard.transform.up;
                    rot *= Quaternion.AngleAxis(deltaAngle, transform.InverseTransformDirection(up));
                }

                return rot;
            }

            set
            {
                transform.localRotation = value;
                ApplyCurrentAngle();
            }
        }

        private PlayerIdent _playerIdent;

        TiltFive.PlayerSettings playerSettings;

        private float _currentAngle = 0.0f;
        private float _targetAngle = 0.0f;
        private float _transitionStartAngle = 0.0f;
        private float _transitionProgress = 1.0f;

        private bool _poseIsEstablished = false;

        private void Awake()
        {
            _playerIdent = GetComponent<PlayerIdent>();
            TiltFive.Player.TryGetSettings(_playerIdent.playerIndex, out playerSettings);
        }

        public void ResetOneShot()
        {
            _poseIsEstablished = false;
        }

        private void OnEnable()
        {
            if (rotationTrigger == RotationTrigger.OneShotOnEnable)
            {
                ResetOneShot();
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            ApplyCurrentAngle();
        }

        public void SetLocalPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            ApplyCurrentAngle();
        }

        private void ApplyCurrentAngle() {
            if (_playerIdent.playerIndex == TiltFive.PlayerIndex.None)
            {
                return;
            }

            float deltaAngle = -_currentAngle;
            if (gameObject == playerSettings.gameboardSettings.currentGameBoard.gameObject)
            {
                // We're attached to the gameboard itself, so we need to reverse all rotations that we apply.
                deltaAngle = -deltaAngle;
            }

            transform.RotateAround(playerSettings.gameboardSettings.gameBoardCenter,
                    playerSettings.gameboardSettings.currentGameBoard.transform.up,
                    deltaAngle);
        }

        // Update is called once per frame
        void Update()
        {
            if (_playerIdent.playerIndex == TiltFive.PlayerIndex.None)
            {
                return;
            }

            if (TiltFive.Glasses.IsTracked(_playerIdent.playerIndex) &&
                (rotationTrigger == RotationTrigger.Continuous || !_poseIsEstablished))
            {
                _poseIsEstablished = true;

                TiltFive.Glasses.TryGetPose(_playerIdent.playerIndex, out Pose pose);

                Vector3 glassesPositionWrtGameboard = playerSettings.gameboardSettings.currentGameBoard
                    .transform.InverseTransformPoint(pose.position);

                float scaleToMetersFromGameboard = playerSettings.gameboardSettings.currentGameBoard.transform.lossyScale.x
                        / PlayerUtils.GetScaleToUnityFromMeters(_playerIdent.playerIndex);

                float sqDistanceFromGameboardCenter =
                    (glassesPositionWrtGameboard.x * glassesPositionWrtGameboard.x
                        + glassesPositionWrtGameboard.z * glassesPositionWrtGameboard.z)
                    * (scaleToMetersFromGameboard * scaleToMetersFromGameboard);

                if (sqDistanceFromGameboardCenter > deadZoneRadius * deadZoneRadius)
                {
                    float playerAngle = Mathf.Atan2(glassesPositionWrtGameboard.x, -glassesPositionWrtGameboard.z)
                        * Mathf.Rad2Deg;

                    float angleDiff = Mathf.DeltaAngle(_targetAngle, playerAngle);

                    float targetAngleDelta = 0.0f;
                    if (angleDiff > 90 + rotationThreshold || angleDiff < -90 - rotationThreshold)
                    {
                        targetAngleDelta = -180.0f;
                    }
                    else if (angleDiff > rotationThreshold)
                    {
                        targetAngleDelta = +90.0f;
                    }
                    else if (angleDiff < -rotationThreshold)
                    {
                        targetAngleDelta = -90.0f;
                    }

                    if (targetAngleDelta != 0.0f)
                    {
                        _targetAngle = Mathf.DeltaAngle(0.0f, _targetAngle + targetAngleDelta);
                        _transitionStartAngle = _currentAngle;
                        _transitionProgress = 0.0f;
                    }
                }
            }

            if (_transitionProgress < 1.0f)
            {
                _transitionProgress += Time.deltaTime / transitionDuration;
                _transitionProgress = Mathf.Clamp01(_transitionProgress);
                float newAngle = Mathf.LerpAngle(_transitionStartAngle, _targetAngle, DoEasing(_transitionProgress));
                float deltaAngle = Mathf.DeltaAngle(newAngle, _currentAngle);
                if (gameObject == playerSettings.gameboardSettings.currentGameBoard.gameObject)
                {
                    // We're attached to the gameboard itself, so we need to reverse all rotations that we apply.
                    deltaAngle = -deltaAngle;
                }
                transform.RotateAround(playerSettings.gameboardSettings.gameBoardCenter,
                    playerSettings.gameboardSettings.currentGameBoard.transform.up,
                    deltaAngle);
                _currentAngle = Mathf.DeltaAngle(0.0f, newAngle);
            }
        }

        private float DoEasing(float t)
        {
            switch (easeFunction)
            {
                case EaseFunction.Lerp:
                    return t;

                case EaseFunction.EaseIn:
                    return EaseUtils.EaseIn(t);

                case EaseFunction.EaseOut:
                    return EaseUtils.EaseOut(t);

                case EaseFunction.EaseInOut:
                    return EaseUtils.EaseInOut(t);
            }

            return 1.0f;
        }
    }
}
