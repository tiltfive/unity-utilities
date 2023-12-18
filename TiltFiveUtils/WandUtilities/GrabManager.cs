//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    public enum GrabButton
    {
        Trigger,
        OneButton,
        TwoButton,
    }

    [RequireComponent(typeof(WandIdent))]
    public class GrabManager : MonoBehaviour
    {
        [Tooltip("The handler for grabs using the trigger")]
        public Grabber triggerHandler;

        [Tooltip("The handler for grabs using the one button")]
        public Grabber oneButtonHandler;

        [Tooltip("The handler for grabs using the two button")]
        public Grabber twoButtonHandler;

        private WandIdent _wandIdent;

        private const float TriggerPressedThreshold = 0.2f;
        private const float TriggerReleasedThreshold = 0.1f;

        private int _lastTriggerUpdateFrameCount;
        private bool _prevTriggerPressed = false;
        private bool _isTriggerDown = false;
        private bool _isTriggerUp = false;
        private bool _isOneButtonDown = false;
        private bool _isOneButtonUp = false;
        private bool _isTwoButtonDown = false;
        private bool _isTwoButtonUp = false;

        public Grabber CurrentGrabber { get; private set; }
        private GrabButton _primaryGrabButton;


        private void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
        }

        private void Update()
        {
            UpdateButtonState();


            if (CurrentGrabber != null)
            {
                if (IsButtonUp(_primaryGrabButton))
                {
                    CurrentGrabber.OnGrabRelease();
                    CurrentGrabber = null;
                }
                else
                {
                    if (_isTriggerUp)
                    {
                        CurrentGrabber.OnSecondaryGrabRelease(GrabButton.Trigger);
                    }

                    if (_isOneButtonUp)
                    {
                        CurrentGrabber.OnSecondaryGrabRelease(GrabButton.OneButton);
                    }

                    if (_isTwoButtonUp)
                    {
                        CurrentGrabber.OnSecondaryGrabRelease(GrabButton.TwoButton);
                    }

                    if (_isTriggerDown)
                    {
                        CurrentGrabber.OnSecondaryGrab(GrabButton.Trigger);
                    }

                    if (_isOneButtonDown)
                    {
                        CurrentGrabber.OnSecondaryGrab(GrabButton.OneButton);
                    }

                    if (_isTwoButtonDown)
                    {
                        CurrentGrabber.OnSecondaryGrab(GrabButton.TwoButton);
                    }
                }
            }

            if (CurrentGrabber == null)
            {
                bool _ = MaybeGrab(GrabButton.Trigger, _isTriggerDown, triggerHandler)
                    || MaybeGrab(GrabButton.OneButton, _isOneButtonDown, oneButtonHandler)
                    || MaybeGrab(GrabButton.TwoButton, _isTwoButtonDown, twoButtonHandler);
            }
        }

        private bool MaybeGrab(GrabButton button, bool isButtonDown, Grabber handler)
        {
            if (isButtonDown && handler != null)
            {
                if (handler.TryGrab())
                {
                    CurrentGrabber = handler;
                    _primaryGrabButton = button;
                    return true;
                }
            }

            return false;
        }

        private void UpdateButtonState()
        {
            _isOneButtonDown = IsWandButtonDown(TiltFive.Input.WandButton.One);
            _isOneButtonUp = IsWandButtonUp(TiltFive.Input.WandButton.One);
            _isTwoButtonDown = IsWandButtonDown(TiltFive.Input.WandButton.Two);
            _isTwoButtonUp = IsWandButtonUp(TiltFive.Input.WandButton.Two);

            if (_lastTriggerUpdateFrameCount == Time.frameCount)
            {
                return;
            }

            _lastTriggerUpdateFrameCount = Time.frameCount;

            bool triggerOk = TiltFive.Input.TryGetTrigger(out float value, _wandIdent.controllerIndex,
                                                          _wandIdent.playerIndex);
            if (!triggerOk)
            {
                _isTriggerDown = false;
                _isTriggerUp = _prevTriggerPressed;
                _prevTriggerPressed = false;
                return;
            }

            bool triggerPressed = value >= TriggerPressedThreshold;
            bool triggerReleased = value < TriggerReleasedThreshold;

            _isTriggerDown = triggerPressed && !_prevTriggerPressed;
            _isTriggerUp = triggerReleased && _prevTriggerPressed;

            _prevTriggerPressed = triggerPressed || (!triggerReleased && _prevTriggerPressed);
        }

        private bool IsButtonDown(GrabButton button)
        {
            switch (button)
            {
                case GrabButton.Trigger:
                    return _isTriggerDown;
                case GrabButton.OneButton:
                    return _isOneButtonDown;
                case GrabButton.TwoButton:
                    return _isTwoButtonDown;
            }

            throw new System.InvalidOperationException("Unknown GrabButton value");
        }
        private bool IsButtonUp(GrabButton button)
        {
            switch (button)
            {
                case GrabButton.Trigger:
                    return _isTriggerUp;
                case GrabButton.OneButton:
                    return _isOneButtonUp;
                case GrabButton.TwoButton:
                    return _isTwoButtonUp;
            }

            throw new System.InvalidOperationException("Unknown GrabButton value");
        }

        private bool IsWandButtonDown(TiltFive.Input.WandButton button)
        {
            return TiltFive.Input.TryGetButtonDown(button, out bool pressed, _wandIdent.controllerIndex,
                                                   _wandIdent.playerIndex)
                && pressed;
        }

        private bool IsWandButtonUp(TiltFive.Input.WandButton button)
        {
            return TiltFive.Input.TryGetButtonUp(button, out bool pressed,_wandIdent.controllerIndex, _wandIdent.playerIndex)
                && pressed;
        }
    }
}