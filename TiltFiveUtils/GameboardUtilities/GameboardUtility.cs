using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiltFive;

using Edge = TiltFive.GameBoard.Edge;
using Corner = TiltFive.GameBoard.Corner;

namespace TiltFive.Utilities
{
    /// <summary>
    /// Functions that augment the functionality of the Gameboard class.
    /// </summary>
    public static class GameboardUtility
    {
        /// <summary>
        /// Whether or not the indicated world space position is near a gameboard corner,
        /// within a given angular tolerance.
        /// </summary>
        /// <param name="pos_UWRLD"></param>
        /// <param name="angularTolerance">The angle of the corner-adjacent region,
        /// measured at the gameboard origin, in degrees.</param>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public static bool IsWorldSpacePointNearAnyCorner(this GameBoard gameboard,
            Vector3 pos_UWRLD, float angularTolerance, PlayerIndex playerIndex)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                return false;
            }
            return gameboard.InverseTransformPoint(pos_UWRLD, playerIndex, out var pos_UGBD)
                && gameboard.IsGameboardSpacePointNearAnyCorner(pos_UGBD, angularTolerance, playerIndex);
        }

        /// <summary>
        /// Whether or not the indicated gameboard space position is near a gameboard corner,
        /// within a given angular tolerance.
        /// </summary>
        /// <param name="pos_UGBD"></param>
        /// <param name="angularTolerance">The angle of the corner-adjacent region,
        /// <param name="playerIndex"></param>
        /// measured at the gameboard origin, in degrees.</param>
        /// <returns></returns>
        public static bool IsGameboardSpacePointNearAnyCorner(this GameBoard gameboard,
            Vector3 pos_UGBD, float angularTolerance, PlayerIndex playerIndex)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                return false;
            }

            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);

            // The coordinates in UGBD above are relative to the tracking origin,
            // but we need them to be relative to the physical XZ center of the gameboard.
            // For instance, the XE flat gameboard's physical XZ center is a bit further
            // forward relative to its tracking origin.
            var gameboardCenter_UGBD = gameboardExtents.GetPhysicalCenterPositionInGameboardSpace();
            var farRightCornerPos_UGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(Corner.FarRight) - gameboardCenter_UGBD;
            var farLeftCornerPos_UGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(Corner.FarLeft) - gameboardCenter_UGBD;
            var nearRightCornerPos_UGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(Corner.NearRight) - gameboardCenter_UGBD;
            var nearLeftCornerPos_UGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(Corner.NearLeft) - gameboardCenter_UGBD;
            var posXZ_UGBDPhysical = posXZ_UGBD - gameboardCenter_UGBD;

            // Now we can just check the direction vectors between each corner and the physical center
            // (which is just their position in UGBDPhysical) and compare against the direction vector
            // of the provided point, obtaining an angle between the two directions.
            // We can then return whether the smallest resulting angle is within the specified angular tolerance.
            var angleDifferenceFromNearestCorner = Mathf.Min(
                Vector3.Angle(posXZ_UGBDPhysical, farRightCornerPos_UGBDPhysical),
                Vector3.Angle(posXZ_UGBDPhysical, farLeftCornerPos_UGBDPhysical),
                Vector3.Angle(posXZ_UGBDPhysical, nearRightCornerPos_UGBDPhysical),
                Vector3.Angle(posXZ_UGBDPhysical, nearLeftCornerPos_UGBDPhysical));
            return angleDifferenceFromNearestCorner < angularTolerance / 2f;
        }

        /// <summary>
        /// Whether or not the indicated gameboard space position is near the specified gameboard corner,
        /// within a given angular tolerance.
        /// </summary>
        /// <param name="gameboard"></param>
        /// <param name="pos_UGBD"></param>
        /// <param name="corner"></param>
        /// <param name="angularTolerance">The angle of the corner-adjacent region,
        /// measured at the gameboard origin, in degrees.</param>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public static bool IsGameboardSpacePointNearCorner(this GameBoard gameboard,
            Vector3 pos_UGBD, Corner corner, float angularTolerance, PlayerIndex playerIndex)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                return false;
            }
            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);

            // The coordinates in UGBD above are relative to the tracking origin,
            // but we need them to be relative to the physical XZ center of the gameboard.
            // For instance, the XE flat gameboard's physical XZ center is a bit further
            // forward relative to its tracking origin.
            var gameboardCenter_UGBD = gameboardExtents.GetPhysicalCenterPositionInGameboardSpace();
            var cornerPos_UGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(corner) - gameboardCenter_UGBD;
            var posXZ_UGBDPhysical = posXZ_UGBD - gameboardCenter_UGBD;

            var angleDifferenceFromCorner = Vector3.Angle(posXZ_UGBDPhysical, cornerPos_UGBDPhysical);

            return angleDifferenceFromCorner < angularTolerance / 2f;
        }

        /// <summary>
        /// Determines whether or not the indicated world space position is within a cylindrical
        /// region above the physical center of the gameboard.
        /// </summary>
        /// <remarks>
        /// Note that this is a radius around the physical center of the gameboard,
        /// which may be distinct from the tracking origin at [0, 0, 0] in gameboard space.
        /// </remarks>
        /// <param name="pos_UWRLD"></param>
        /// <param name="centralRadius"></param>
        /// <param name="playerIndex"></param>
        /// <returns>Returns true if the indicated position in world space is within the indicated
        /// cylindrical volume above the physical center of the gameboard.</returns>
        public static bool IsWorldSpacePointWithinCentralRadius(this GameBoard gameboard,
            Vector3 pos_UWRLD, float centralRadius, PlayerIndex playerIndex)
        {
            return gameboard.InverseTransformPoint(pos_UWRLD, playerIndex, out var pos_UGBD)
                && gameboard.IsGameboardSpacePointWithinCentralRadius(pos_UGBD, centralRadius, playerIndex);
        }

        /// <summary>
        /// Determines whether or not the indicated gameboard space position is within a cylindrical
        /// region above the physical center of the gameboard.
        /// </summary>
        /// <remarks>
        /// Note that this is a radius around the physical center of the gameboard,
        /// which may be distinct from the tracking origin at [0, 0, 0] in gameboard space.
        /// </remarks>
        /// <param name="pos_UGBD"></param>
        /// <param name="centralRadius">The radius of the cylindrical region, in meters.</param>
        /// <returns>Returns true if the indicated position in gameboard space is within the indicated
        /// cylindrical volume above the physical center of the gameboard.</returns>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public static bool IsGameboardSpacePointWithinCentralRadius(this GameBoard gameboard,
            Vector3 pos_UGBD, float centralRadius, PlayerIndex playerIndex)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                return false;
            }

            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);

            // The provided coordinate in UGBD is relative to the tracking origin,
            // but we'd like it to be relative to the physical XZ center of the gameboard.
            // For instance, the XE flat gameboard's physical XZ center is a bit further
            // forward relative to its tracking origin.
            var gameboardCenter_UGBD = gameboardExtents.GetPhysicalCenterPositionInGameboardSpace();
            var posXZ_UGBDPhysical = posXZ_UGBD - gameboardCenter_UGBD;

            return posXZ_UGBDPhysical.magnitude <= centralRadius;
        }

        /// <summary>
        /// Obtains the gameboard <see cref="Edge"/> that faces the provided point in world space.
        /// </summary>
        /// <param name="gameboard"></param>
        /// <param name="pos_UWRLD"></param>
        /// <param name="playerIndex"></param>
        /// <param name="edge"></param>
        /// <returns>Returns true if the nearest edge to the specified position was successfully determined; false otherwise.
        /// Use this return value to determine whether <paramref name="edge"/> is valid</returns>
        public static bool TryGetEdgeNearestWorldSpacePosition(this GameBoard gameboard,
            Vector3 pos_UWRLD, PlayerIndex playerIndex, out Edge edge)
        {
            if (!gameboard.InverseTransformPoint(pos_UWRLD, playerIndex, out var pos_UGBD))
            {
                edge = Edge.Near;
                return false;
            }

            return gameboard.TryGetEdgeNearestGameboardSpacePosition(pos_UGBD, playerIndex, out edge);
        }

        /// <summary>
        /// Obtains the gameboard <see cref="Edge"/> that faces the provided point in gameboard space.
        /// </summary>
        /// <param name="pos_UGBD"></param>
        /// <param name="gameboardExtents"></param>
        /// <returns>Returns true if the nearest edge to the specified position was successfully determined; false otherwise.
        /// Use this return value to determine whether <paramref name="edge"/> is valid</returns>
        public static bool TryGetEdgeNearestGameboardSpacePosition(this GameBoard gameboard,
            Vector3 pos_UGBD, PlayerIndex playerIndex, out Edge edge)
        {
            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);

            for(int i = 0; i < 4; i++)
            {
                var currentEdge = (Edge)i;
                if(gameboard.IsGameboardSpacePositionOnEdge(posXZ_UGBD, currentEdge, playerIndex))
                {
                    edge = currentEdge;
                    return true;
                }
            }

            // If we get this far, then all of the above checks failed and we should also fail here.
            edge = Edge.Near;
            return false;
        }

        /// <summary>
        /// Determines whether the specified position in Unity world space is within the projected
        /// quadrant of the gameboard associated with a particular gameboard edge.
        /// </summary>
        /// <param name="pos_UWRLD"></param>
        /// <param name="edge"></param>
        /// <param name="gameboardExtents"></param>
        /// <param name="scaleSettings"></param>
        /// <param name="gameBoardSettings"></param>
        /// <returns></returns>
        public static bool IsWorldSpacePositionOnEdge(this GameBoard gameboard,
            Vector3 pos_UWRLD, Edge edge, PlayerIndex playerIndex)
        {
            return gameboard.InverseTransformPoint(pos_UWRLD, playerIndex, out var pos_UGBD)
                && gameboard.IsGameboardSpacePositionOnEdge(pos_UGBD, edge, playerIndex);
        }

        /// <summary>
        /// Determines whether the specified position in gameboard space is within the projected
        /// quadrant of the gameboard associated with a particular gameboard edge.
        /// </summary>
        /// <param name="pos_UGBD"></param>
        /// <param name="edge"></param>
        /// <param name="gameboardExtents"></param>
        /// <returns></returns>
        public static bool IsGameboardSpacePositionOnEdge(this GameBoard gameboard, Vector3 pos_UGBD, Edge edge, PlayerIndex playerIndex)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                return false;
            }

            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);

            // Get the corner nearest to the specified position. If the corner closest to the specified
            // point is not adjacent to the edge we care about, we can instantly rule out this edge.
            if (!gameboard.TryGetCornerNearestGameboardPosition(posXZ_UGBD, playerIndex, out var nearestCorner, out var angleToPointFromNearestCorner)
                || (edge == Edge.Near && nearestCorner != Corner.NearLeft && nearestCorner != Corner.NearRight)
                || (edge == Edge.Left && nearestCorner != Corner.NearLeft && nearestCorner != Corner.FarLeft)
                || (edge == Edge.Far && nearestCorner != Corner.FarLeft && nearestCorner != Corner.FarRight)
                || (edge == Edge.Right && nearestCorner != Corner.FarRight && nearestCorner != Corner.NearRight))
            {
                return false;
            }

            var gameboardCenter_UGBD = gameboardExtents.GetPhysicalCenterPositionInGameboardSpace();

            // Lets convert some important positions to modified UGBD, in which the physical gameboard center is the origin.
            var pos_UGBDPhysical = posXZ_UGBD - gameboardCenter_UGBD;
            var edgeCenterPositionUGBDPhysical = gameboardExtents.GetEdgeCenterPositionInGameboardSpace(edge) - gameboardCenter_UGBD;
            var nearestCornerPosUGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(nearestCorner) - gameboardCenter_UGBD;

            // Now check the angle from the center of the edge we're checking to the specified point's nearest corner.
            // If this angle is greater than the angle from the center of the edge to the specified point itself, return true.
            var angleToNearestCornerFromEdgeCenter = Vector3.Angle(edgeCenterPositionUGBDPhysical, nearestCornerPosUGBDPhysical);
            var angleToPointFromEdgeCenter = Vector3.Angle(edgeCenterPositionUGBDPhysical, pos_UGBDPhysical);

            // Pick a vector parallel to the gameboard edge.
            // The direction it points will be determined by cross product argument order,
            // and it isn't particularly important as long as we're consistent.
            var biasVector = Vector3.Cross(edgeCenterPositionUGBDPhysical, Vector3.up);

            // Now check if the vector from the origin to our corner of interest is pointing toward or away from the bias vector.
            // If our corner is approximately aligned with the bias vector, we'll use an inclusive operator.
            // This ensures that we don't introduce a discontinuity / literal corner case between edges/quadrants.
            if (Vector3.Dot(nearestCornerPosUGBDPhysical, biasVector) > 0)
            {
                return angleToPointFromEdgeCenter <= angleToNearestCornerFromEdgeCenter;
            }
            else
            {
                return angleToPointFromEdgeCenter < angleToNearestCornerFromEdgeCenter;
            }
        }

        /// <summary>
        /// Obtains the gameboard <see cref="Corner"/> angled toward the provided point in gameboard space.
        /// </summary>
        /// <param name="pos_UWRLD"></param>
        /// <param name="gameboardExtents"></param>
        /// <param name="scaleSettings"></param>
        /// <param name="gameboardSettings"></param>
        /// <param name="nearestCorner"></param>
        /// <param name="angleToPointFromNearestCorner"></param>
        /// <returns>Returns true if the nearest corner to the specified position was successfully determined; false otherwise.
        /// Use this return value to determine whether <paramref name="nearestCorner"/> and <paramref name="angleToPointFromNearestCorner"/> are valid</returns>
        public static bool TryGetCornerNearestWorldSpacePosition(this GameBoard gameboard, Vector3 pos_UWRLD,
            PlayerIndex playerIndex, out Corner nearestCorner, out float angleToPointFromNearestCorner)
        {
            if (!gameboard.InverseTransformPoint(pos_UWRLD, playerIndex, out var pos_UGBD))
            {
                nearestCorner = Corner.NearLeft;
                angleToPointFromNearestCorner = 0f;
                return false;
            }

            return gameboard.TryGetCornerNearestGameboardPosition(pos_UGBD, playerIndex, out nearestCorner, out angleToPointFromNearestCorner);
        }

        /// <summary>
        /// Obtains the gameboard <see cref="Corner"/> angled toward the provided point in gameboard space.
        /// </summary>
        /// <param name="pos_UGBD"></param>
        /// <param name="playerIndex"></param>
        /// <param name="nearestCorner"></param>
        /// <param name="angleToPointFromNearestCorner"></param>
        /// <returns>Returns true if the nearest corner to the specified position was successfully determined; false otherwise.
        /// Use this return value to determine whether <paramref name="nearestCorner"/> and <paramref name="angleToPointFromNearestCorner"/> are valid</returns>
        public static bool TryGetCornerNearestGameboardPosition(this GameBoard gameboard, Vector3 pos_UGBD, PlayerIndex playerIndex, out Corner nearestCorner, out float angleToPointFromNearestCorner)
        {
            if (!GameBoard.TryGetGameboardType(playerIndex, out var gameboardType)
                || !GameBoard.TryGetGameboardExtents(gameboardType, out var gameboardExtents))
            {
                nearestCorner = Corner.NearLeft;
                angleToPointFromNearestCorner = 0f;
                return false;
            }

            // Project onto the gameboard surface plane by discarding the Y position component.
            var posXZ_UGBD = new Vector3(pos_UGBD.x, 0f, pos_UGBD.z);
            var gameboardCenter = Vector3.forward * (gameboardExtents.OriginOffsetZ.ToMeters);
            var pos_UGBDPhysical = posXZ_UGBD - gameboardCenter;

            angleToPointFromNearestCorner = 180f;
            nearestCorner = Corner.FarRight;

            for (int i = 0; i < 4; i++)
            {
                var currentCorner = (Corner)i;
                var currentCornerPosUGBDPhysical = gameboardExtents.GetCornerPositionInGameboardSpace(currentCorner) - gameboardCenter;
                var angleToSpecifiedPoint = Vector3.Angle(currentCornerPosUGBDPhysical, pos_UGBDPhysical);

                if (angleToSpecifiedPoint < angleToPointFromNearestCorner)
                {
                    nearestCorner = currentCorner;
                    angleToPointFromNearestCorner = angleToSpecifiedPoint;
                }
            }

            return true;
        }
    }
}
