//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(WandIdent))]
    public class Panner : Grabber
    {
        // Reference frames:
        //   IGBD - The initial gameboard reference frame.
        //   CGBD - The current gameboard reference frame.
        //   PGBD - The previous gameboard reference frame.
        //   DGBD - The desired gameboard reference frame.
        //   IWND - The initial wand reference frame.
        //   CWND - The current wand reference frame.
        //   PWND - The previous wand reference frame.
        //   WRLD - The Unity world space reference frame.

        [Tooltip("The secondary button used to rotate the gameboard")]
        public GrabButton secondaryButton;

        [Tooltip("The controller position at which the grab takes place")]
        public TiltFive.ControllerPosition controllerPosition;

        [Tooltip("The amplification factor applied to the input rotation")]
        public float rotationFactor = 1.0f;

        [Tooltip("The minimum scale factor change that can be done with a single grab")]
        public float minScaleChange = 0.25f;

        [Tooltip("The maximum scale factor change that can be done with a single grab")]
        public float maxScaleChange = 4.0f;

        private WandIdent _wandIdent;

        TiltFive.PlayerSettings playerSettings;

        private bool _isGrabbed = false;
        private Vector3 _igbdToIWND_WRLD;
        private Vector3 _posIGBD_WRLD;

        private bool _isSecondaryGrabbed = false;
        private float _scaleToWRLD_IGBD;
        private float _initialLocalGameboardScale;
        private Quaternion _rotToWRLD_IGBD;
        private Quaternion _rotToWRLD_IWND;
        private Quaternion _rotToIGBD_IWND;
        private Quaternion _rotToPGBD_PWND;
        private float _accumulatedWandRoll = 0.0f;

        private void Awake()
        {
            _wandIdent = GetComponent<WandIdent>();
            TiltFive.Player.TryGetSettings(_wandIdent.playerIndex, out playerSettings);
        }

        public override bool TryGrab()
        {
            Vector3 wandPosition = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, controllerPosition,
                _wandIdent.playerIndex);
            _isGrabbed = true;
            _posIGBD_WRLD = playerSettings.gameboardSettings.currentGameBoard.transform.position;
            _igbdToIWND_WRLD = wandPosition - _posIGBD_WRLD;

            return true;
        }

        public override void OnGrabRelease()
        {
            _isGrabbed = false;
            _isSecondaryGrabbed = false;
        }

        public override void OnSecondaryGrab(GrabButton button)
        {
            if (button == secondaryButton)
            {
                _scaleToWRLD_IGBD = playerSettings.gameboardSettings.currentGameBoard.transform.lossyScale.x;
                _initialLocalGameboardScale = playerSettings.gameboardSettings.currentGameBoard.transform.localScale.x;
                _rotToWRLD_IGBD = playerSettings.gameboardSettings.currentGameBoard.transform.rotation;
                _rotToWRLD_IWND = TiltFive.Wand.GetRotation(_wandIdent.controllerIndex, _wandIdent.playerIndex);
                _rotToIGBD_IWND = Quaternion.Inverse(_rotToWRLD_IGBD) * _rotToWRLD_IWND;
                _rotToPGBD_PWND = _rotToIGBD_IWND;

                Vector3 posIWND_WRLD = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, controllerPosition,
                    _wandIdent.playerIndex);
                _posIGBD_WRLD = playerSettings.gameboardSettings.currentGameBoard.transform.position;
                _igbdToIWND_WRLD = posIWND_WRLD - _posIGBD_WRLD;

                _accumulatedWandRoll = 0.0f;
                _isSecondaryGrabbed = true;
            }
        }

        public override void OnSecondaryGrabRelease(GrabButton button)
        {
            if (button == secondaryButton)
            {
                _isSecondaryGrabbed = false;

                Vector3 posIWND_WRLD = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, controllerPosition,
                    _wandIdent.playerIndex);
                _posIGBD_WRLD = playerSettings.gameboardSettings.currentGameBoard.transform.position;
                _igbdToIWND_WRLD = posIWND_WRLD - _posIGBD_WRLD;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_isSecondaryGrabbed)
            {
                Vector3 posCWND_WRLD = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, controllerPosition,
                    _wandIdent.playerIndex);
                float scaleToWRLD_CGBD = playerSettings.gameboardSettings.currentGameBoard.transform.lossyScale.x;
                Vector3 posCGBD_WRLD = playerSettings.gameboardSettings.currentGameBoard.transform.position;
                Quaternion rotToCGBD_WRLD = Quaternion.Inverse(
                    playerSettings.gameboardSettings.currentGameBoard.transform.rotation);
                Vector3 posCWND_CGBD = rotToCGBD_WRLD * (posCWND_WRLD - posCGBD_WRLD) / scaleToWRLD_CGBD;
                Vector3 posIWND_IGBD = Quaternion.Inverse(_rotToWRLD_IGBD) * _igbdToIWND_WRLD / _scaleToWRLD_IGBD;
                Quaternion offAxisRotToIGBD_DGBD = RotationUtils.ScaleRotation(rotationFactor,
                    Quaternion.FromToRotation(posCWND_CGBD, posIWND_IGBD));

                Quaternion rotToWRLD_CWND = TiltFive.Wand.GetRotation(_wandIdent.controllerIndex, _wandIdent.playerIndex);
                Quaternion rotToCGBD_CWND = rotToCGBD_WRLD * rotToWRLD_CWND;
                Quaternion rotToCWND_PWND = Quaternion.Inverse(rotToCGBD_CWND) * _rotToPGBD_PWND;
                _accumulatedWandRoll += RotationUtils.RotationComponentAbout(rotToCWND_PWND, Vector3.forward);
                Quaternion onAxisRotToIGBD_DGBD = Quaternion.AngleAxis(-_accumulatedWandRoll * rotationFactor, posIWND_IGBD);

                Quaternion rotToWRLD_DGBD = _rotToWRLD_IGBD * onAxisRotToIGBD_DGBD * offAxisRotToIGBD_DGBD;

                playerSettings.gameboardSettings.currentGameBoard.transform.rotation = rotToWRLD_DGBD;

                float newScale = _initialLocalGameboardScale
                    * Mathf.Clamp(posIWND_IGBD.magnitude / posCWND_CGBD.magnitude, minScaleChange, maxScaleChange);
                playerSettings.gameboardSettings.currentGameBoard.transform.localScale
                    = new Vector3(newScale, newScale, newScale);

                _rotToPGBD_PWND = rotToCGBD_CWND;
            }
            else if (_isGrabbed)
            {
                Vector3 posCWND_WRLD = TiltFive.Wand.GetPosition(_wandIdent.controllerIndex, controllerPosition,
                    _wandIdent.playerIndex);
                Vector3 posCGBD_WRLD = playerSettings.gameboardSettings.currentGameBoard.transform.position;
                Vector3 cgbdToCWND_WRLD = posCWND_WRLD - posCGBD_WRLD;
                Vector3 deltaPosition = cgbdToCWND_WRLD - _igbdToIWND_WRLD;
                playerSettings.gameboardSettings.currentGameBoard.transform.position =
                    _posIGBD_WRLD - deltaPosition;
            }
        }
    }
}