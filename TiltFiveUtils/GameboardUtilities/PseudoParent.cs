//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    public class PseudoParent : MonoBehaviour
    {
        private List<GameObject> _children = new List<GameObject>();

        private void Awake()
        {
            string name = gameObject.name;
            gameObject.name = "Pseudo " + name;
            GameObject newParent = GameObject.Find(name);

            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                _children.Add(child.gameObject);
                child.parent = newParent.transform;
            }
        }

        private void OnDestroy()
        {
            foreach (GameObject child in _children)
            {
                GameObject.Destroy(child);
            }
        }
    }

}