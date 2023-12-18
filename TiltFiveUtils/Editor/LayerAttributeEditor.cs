//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TiltFiveUtils
{
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
    #endif
}
