//
// Copyright (C) 2023 Tilt Five, Inc.
//

﻿using UnityEngine;

namespace TiltFiveUtils
{
    [System.Serializable]
    public struct TransformLocalValue
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public void Apply(Transform transform)
        {
            BoardRotate boardRotate = transform.gameObject.GetComponent<BoardRotate>();
            if (boardRotate != null)
            {
                boardRotate.SetLocalPositionAndRotation(localPosition, localRotation);
            }
            else
            {
                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
            }
            transform.localScale = localScale;
        }

        public static TransformLocalValue FromTransform(Transform transform)
        {
            TransformLocalValue value;
            value.localPosition = transform.localPosition;
            value.localScale = transform.localScale;

            BoardRotate boardRotate = transform.gameObject.GetComponent<BoardRotate>();
            if (boardRotate != null)
            {
                value.localRotation = boardRotate.localRotation;
            }
            else
            {
                value.localRotation = transform.localRotation;
            }

            return value;
        }

        public static TransformLocalValue Lerp(TransformLocalValue a, TransformLocalValue b, float t)
        {
            TransformLocalValue result;
            t = Mathf.Clamp01(t);
            result.localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, t);
            result.localRotation = Quaternion.SlerpUnclamped(a.localRotation, b.localRotation, t);
            result.localScale = Vector3.LerpUnclamped(a.localScale, b.localScale, t);
            return result;
        }
    }

}
