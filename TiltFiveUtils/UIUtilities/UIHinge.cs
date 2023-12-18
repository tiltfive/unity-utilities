/*
 * Copyright (C) 2020 Tilt Five, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;

namespace TiltFiveDemos
{
    /// <summary>
    /// Class for the hinge-style UI.
    /// </summary>
    public class UIHinge : MonoBehaviour
    {
        /// <summary>
        /// The canvas transform.
        /// </summary>
        [SerializeField] private Transform _uICanvasTransform;

        /// <summary>
        /// The TiltFive camera transform.
        /// </summary>
        [SerializeField] private Transform _cameraTransform;

        /// <summary>
        /// Whether to auto select the closest position in the board.
        /// </summary>
        [SerializeField] private bool _chooseClosestPosition = false;

        /// <summary>
        /// The positions in the board
        /// </summary>
        [SerializeField] private Transform[] _positions;

        // Update is called once per frame
        void Update()
        {
            ChoosePosition();
            SetRotation();
        }

        /// <summary>
        /// Sets the rotation for the hinge to look at the player.
        /// </summary>
        private void SetRotation()
        {
            // Get the camera position and the canvas position.
            Vector3 cameraPosition = _cameraTransform.position;
            Vector3 canvasPosition = _uICanvasTransform.position;

            // Use the direction from the canvas to the camera to find the look rotation.
            Quaternion lookRotation = Quaternion.LookRotation(canvasPosition - cameraPosition);
            Vector3 lookRotationEuler = lookRotation.eulerAngles;

            // Find at which rotation over the board the hinge should stop moving, so that it's completely flat when the player is looking over it.
            Vector3 lookRotationFixed = lookRotationEuler;
            Vector3 objectiveRotation = _uICanvasTransform.rotation.eulerAngles; // Get the rotation according to Unity's conversion from the rotation Quaternion to Vector3.

            // Select the correct cutoff point depending on the Y rotation of the board, where the hinge will not rotate below 90 degrees.
            if (objectiveRotation.y >= 270f)
            {
                if (lookRotationEuler.y >= 0f && lookRotationEuler.y <= 180f)
                {
                    lookRotationFixed = new Vector3(90f,
                        lookRotationEuler.y,
                        lookRotationEuler.z);
                }
            }
            else if (objectiveRotation.y >= 180f)
            {
                if (lookRotationEuler.y <= 90f || lookRotationEuler.y >= 270f)
                {
                    lookRotationFixed = new Vector3(90f,
                        lookRotationEuler.y,
                        lookRotationEuler.z);
                }
            }
            else if (objectiveRotation.y >= 90f)
            {
                if (lookRotationEuler.y <= 0f || lookRotationEuler.y >= 180f)
                {
                    lookRotationFixed = new Vector3(90f,
                        lookRotationEuler.y,
                        lookRotationEuler.z);
                }
            }
            else if (objectiveRotation.y >= 0f)
            {
                if (lookRotationEuler.y >= 90f && lookRotationEuler.y <= 270f)
                {
                    lookRotationFixed = new Vector3(90f,
                        lookRotationEuler.y,
                        lookRotationEuler.z);
                }
            }

            // Use our rotation only for the X axis, and keep the other axis' rotations.
            Vector3 canvasRotation = _uICanvasTransform.rotation.eulerAngles;
            Vector3 goalRotationEuler = new Vector3(lookRotationFixed.x, canvasRotation.y, canvasRotation.z);
            Quaternion goalRotation = Quaternion.Euler(goalRotationEuler);

            _uICanvasTransform.rotation = goalRotation;
        }

        /// <summary>
        /// Choose the position closest to the player
        /// </summary>
        private void ChoosePosition()
        {
            // If we want to select the closest position to the player,
            if (!_chooseClosestPosition) return;
            
            // Find the closest position using Vector3.Distance()
            Transform closestPosition = _positions[0];

            for (int i = 0; i < _positions.Length; i++)
            {
                if (Vector3.Distance(_positions[i].position, _cameraTransform.position) <
                    Vector3.Distance(closestPosition.position, _cameraTransform.position))
                {
                    closestPosition = _positions[i];
                }
            }

            // Assign the closest position values to the hinge object
            _uICanvasTransform.position = closestPosition.position;
            _uICanvasTransform.rotation = closestPosition.rotation;
        }
    }
}
