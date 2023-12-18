//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(PlayerIdent))]
    public class ApplyPlayerColor : MonoBehaviour
    {
        [Tooltip("The GameObject from which the player colors are queried")]
        public PlayerColors playerColors;

        void Start()
        {
            PlayerIdent playerIdent = GetComponent<PlayerIdent>();
            Color color = playerColors.GetPlayerColor(playerIdent.playerIndex);

            {
                Renderer[] renderers = GetComponents<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material.color = color;
                }
            }

            {
                TMPro.TMP_Text[] texts = GetComponents<TMPro.TMP_Text>();
                foreach (var text in texts)
                {
                    text.color = color;
                }
            }
        }
    }

}