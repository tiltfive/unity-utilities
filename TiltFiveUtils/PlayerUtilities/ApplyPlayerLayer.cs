//
// Copyright (C) 2023 Tilt Five, Inc.
//

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TiltFiveUtils
{
    [RequireComponent(typeof(PlayerIdent))]
    public class ApplyPlayerLayer : MonoBehaviour
    {
        public PlayerLayers playerLayers;

        private PlayerIdent _playerIdent;

        private void Awake()
        {
            _playerIdent = GetComponent<PlayerIdent>();
        }

        void Start()
        {
            int playerLayer = GetLayer(_playerIdent.playerIndex);
            foreach (Transform childTransform in GetComponentsInChildren<Transform>())
            {
                childTransform.gameObject.layer = playerLayer;
            }
            gameObject.layer = playerLayer;
        }

        private int GetLayer(TiltFive.PlayerIndex playerIndex)
        {
            switch (playerIndex)
            {
                case TiltFive.PlayerIndex.None:
                    return playerLayers.playerNoneLayer;

                case TiltFive.PlayerIndex.One:
                    return playerLayers.playerOneLayer;

                case TiltFive.PlayerIndex.Two:
                    return playerLayers.playerTwoLayer;

                case TiltFive.PlayerIndex.Three:
                    return playerLayers.playerThreeLayer;

                case TiltFive.PlayerIndex.Four:
                    return playerLayers.playerFourLayer;
            }

            throw new System.ArgumentException("Invalid playerIndex argument.");
        }
    }

}