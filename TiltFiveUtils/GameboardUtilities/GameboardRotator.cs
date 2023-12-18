using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFive.Utilities
{
    /// <summary>
    /// GameboardRotator automatically rotates the specified player's gameboard around in the scene
    /// depending on the player's position around the gameboard.
    /// </summary>
    /// <remarks>This component can be useful for applications that maintain fixed perspectives of the scene, allowing
    /// the content on the gameboard to rotate and face the player as they stand up and walk around.</remarks>
    [RequireComponent(typeof(GameBoard))]
    public class GameboardRotator : MonoBehaviour
    {
        /// <summary>
        /// The player that GameboardRotator will follow when deciding whether to rotate the gameboard.
        /// </summary>
        public PlayerIndex playerIndex = PlayerIndex.One;

        /// <summary>
        /// A Transform that UI canvases can be parented under to keep the UI in the scene facing the player
        /// as the gameboard rotates.
        /// </summary>
        public Transform uiRootTransform = null;

        [SerializeField]
        private TiltFiveManager2 tiltFiveManager2;

        [SerializeField]
        [Range(0, 45)]
        [Tooltip("The angle at the gameboard origin (in degrees) of the triangular region centered around each corner. " +
            "When a player's head is positioned in this region, GameboardRotator will refrain from rotating to avoid flickering.")]
        private float angleTolerance = 15f;

        [SerializeField]
        [Range(0, 5)]
        [Tooltip("The radius (in meters) of the circular region around center of the gameboard. " +
            "When a player's head is positioned in this region, GameboardRotator will refrain from rotating to avoid flickering.")]
        private float centralRadius = .15f;

        private GameBoard gameBoard;

        private GameBoard.Edge currentBoardEdge = GameBoard.Edge.Near;

        private bool configured => playerIndex != PlayerIndex.None
            && tiltFiveManager2 != null
            && gameBoard != null;

        private void Awake()
        {
            gameBoard = GetComponent<GameBoard>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (!configured
                || !Player.IsConnected(playerIndex)
                || !Glasses.TryGetPose(playerIndex, out var pose_UWRLD))
            {
                return;
            }

            RotateGameboard(pose_UWRLD);
        }

        private void RotateGameboard(Pose glassesPose_UWRLD)
        {
            var previousBoardEdge = currentBoardEdge;

            if (!TryGetNearestGameboardEdge(glassesPose_UWRLD, out currentBoardEdge, false)
                || previousBoardEdge == currentBoardEdge)
            {
                return;
            }

            var localRotationY = 0f;

            switch (currentBoardEdge)
            {
                case GameBoard.Edge.Near:
                    localRotationY = 0f;
                    break;
                case GameBoard.Edge.Far:
                    localRotationY = 180f;
                    break;
                case GameBoard.Edge.Left:
                    localRotationY = -90f;
                    break;
                case GameBoard.Edge.Right:
                    localRotationY = 90f;
                    break;
            }

            transform.localRotation = Quaternion.Euler(0f, localRotationY, 0f);

            if (uiRootTransform != null && uiRootTransform.IsChildOf(transform))
            {
                // Compensate for the UI rotating alongside the gameboard due to being a child GameObject.
                uiRootTransform.localRotation = Quaternion.Euler(0f, -localRotationY, 0f);
            }
        }

        private bool TryGetNearestGameboardEdge(Pose glassesPose_UWRLD, out GameBoard.Edge boardEdge, bool ignoreCentralDeadzone = false)
        {
            // Before actually checking for the nearest edge to the user's position, we will check if they are within a few regions of interest.
            // In particular, we care about the corners and a circle around gameboard center.
            //
            // When the player gets too close to a corner or the center of the board, there's a tendency for gameboard rotation scripts
            // to quickly "flicker" back and forth across the threshold between edges due to noise in the glasses position data.
            //
            // To fix this, we treat the corners and center of the gameboard as deadzones (or grace-zones?)
            // where the user is allowed to be present without triggering a gameboard rotation.
            //
            // Each corner zone is a triangular prism radiating outward and upward/downward from the gameboard origin forever,
            // with the angle at the origin corresponding to the angular tolerance field we define in the Unity inspector.
            //
            // The central zone is a cylinder above the center of the gameboard
            // (and technically below the gameboard as well, if the glasses were able to track underneath the table)
            // with a radius defined by the central radius field in the Unity inspector. This allows players to safely lean over
            // the center of the gameboard without triggering any flickering due to the converging edge/quadrant thresholds.
            //
            // If the player is in one of these zones, this function will fail to return the nearest edge due to the ambiguity
            // of which edge to pick. This means that the most recently detected edge (currentBoardEdge) does not change.
            // Once the player leaves these zones, we can safely calculate which edge they are near.

            if (gameBoard.IsWorldSpacePointWithinCentralRadius(glassesPose_UWRLD.position, centralRadius, playerIndex) && !ignoreCentralDeadzone)
            {
                boardEdge = currentBoardEdge;
                return false;
            }

            if (gameBoard.IsWorldSpacePointNearAnyCorner(glassesPose_UWRLD.position, angleTolerance, playerIndex) && !ignoreCentralDeadzone)
            {
                boardEdge = currentBoardEdge;
                return false;
            }

            if (!gameBoard.TryGetEdgeNearestWorldSpacePosition(glassesPose_UWRLD.position, playerIndex, out var nearestEdge))
            {
                boardEdge = currentBoardEdge;
                return false;
            }

            boardEdge = nearestEdge;
            return true;
        }
    }
}